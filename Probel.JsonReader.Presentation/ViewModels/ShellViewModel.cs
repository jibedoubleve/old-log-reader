﻿using Microsoft.Practices.Unity;
using Prism.Mvvm;
using Probel.JsonReader.Business;
using Probel.JsonReader.Business.Data;
using Probel.JsonReader.Presentation.Helpers;
using Probel.JsonReader.Presentation.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Probel.JsonReader.Presentation.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        #region Fields

        public readonly string DefaultDirectory;
        private const string DEFAULT_DIRECTORY = @"%appdata%\Probel\Perdeval\Logs\";
        private const string TITLE_PREFIX = "Log reader";
        private readonly ILogRepository LogRepository;
        private int _filterMinutes = 0;
        private ObservableCollection<LogModel> _logs = new ObservableCollection<LogModel>();
        private SettingsViewModel _settings;
        private string _status = Messages.Status_Ready;
        private string _statusItemsCount = string.Format(Messages.Status_xxItems, 0);
        private string _title = TITLE_PREFIX;
        private string _version;
        private string FilePath;

        private FileSystemWatcher FileWatcher;

        #endregion Fields

        #region Constructors

        public ShellViewModel(ILogRepository logRepository, ICommandBuilder commandBuilder)
        {
            var v = Assembly.GetEntryAssembly().GetName().Version;
            Version = string.Format(Messages.Status_Version, v.ToString(3));

            LogRepository = logRepository;
            DefaultDirectory = Environment.ExpandEnvironmentVariables(DEFAULT_DIRECTORY);

            OpenCommand = commandBuilder.BuildAsyncCommand<string>(Open, CanOpen);
            FilterCommand = commandBuilder.BuildAsyncCommand<string>(FilterAsync, CanFilter);
        }

        #endregion Constructors

        #region Properties

        public ICommand FilterCommand { get; }

        public int FilterMinutes
        {
            get => _filterMinutes;
            set => SetProperty(ref _filterMinutes, value, nameof(FilterMinutes));
        }

        public ObservableCollection<LogModel> Logs
        {
            get => _logs;
            set => SetProperty(ref _logs, value, nameof(Logs));
        }

        public ICommand OpenCommand { get; }

        [Dependency]
        public SettingsViewModel Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value, nameof(Settings));
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value, nameof(Status));
        }

        public string StatusItemsCount
        {
            get => _statusItemsCount;
            set => SetProperty(ref _statusItemsCount, value, nameof(StatusItemsCount));
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, $"{TITLE_PREFIX} - [{value}]", nameof(Title));
        }

        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value, nameof(Version));
        }

        private IEnumerable<LogModel> BufferLogs { get; set; }

        #endregion Properties

        #region Methods

        private bool CanFilter(string arg)
        {
            var isNumber = (arg == null) ? true : int.TryParse(arg, out var r);
            var hasLogs = BufferLogs != null && BufferLogs.Count() != 0;
            return isNumber && hasLogs;
        }

        private bool CanOpen(string filePath)
        {
            var notEmptyPath = !string.IsNullOrEmpty(filePath);
            var exist = File.Exists(filePath);
            return notEmptyPath && exist;
        }

        private void FillLogs(IEnumerable<LogModel> models)
        {
            Logs = new ObservableCollection<LogModel>(models);
            SetItemsCount();
        }

        private async Task FilterAsync(string arg)
        {
            var now = DateTime.Now;
            if (int.TryParse(arg, out var value))
            {
                FilterMinutes = value;
            }

            var logs = await BufferLogs.FilterAsync(FilterMinutes, Settings);
            Logs = new ObservableCollection<LogModel>(logs);
            SetItemsCount();
        }

        private void ListenToFileChange()
        {
            if (FileWatcher != null)
            {
                FileWatcher.Changed -= OnFileChanged;
                FileWatcher = null;
            }

            var path = Path.GetDirectoryName(FilePath);
            FileWatcher = new FileSystemWatcher()
            {
                Path = path,
                Filter = Path.GetFileName(FilePath),
            };
            FileWatcher.Changed += OnFileChanged;
            FileWatcher.EnableRaisingEvents = true;
        }

        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            FilterCommand.RaiseCanExecuteChanged();
            BufferLogs = await Task.Run(() => LogRepository.GetAllLogs(FilePath));
            FillLogs(BufferLogs.Filter(FilterMinutes, Settings));
            Status = Messages.Status_FileChanged + $" - [{DateTime.Now.ToLongTimeString()}]";
        }

        private async Task Open(string filePath)
        {
            FilterMinutes = 0;
            FilePath = filePath;
            BufferLogs = await Task.Run(() => LogRepository.GetAllLogs(filePath));
            ListenToFileChange();

            FilterCommand.RaiseCanExecuteChanged();
            if (CanFilter("0")) { await FilterAsync("0"); }
            Status = Messages.Status_FileLoaded;
        }

        private void SetItemsCount() => StatusItemsCount = string.Format(Messages.Status_xxItems, Logs.Count);

        #endregion Methods
    }
}