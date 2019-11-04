using Microsoft.Practices.Unity;
using Prism.Mvvm;
using Probel.JsonReader.Business;
using Probel.JsonReader.Business.Data;
using Probel.JsonReader.Presentation.Helpers;
using Probel.JsonReader.Presentation.Properties;
using Probel.JsonReader.Presentation.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Probel.JsonReader.Presentation.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        #region Fields

        private const string DEFAULT_DIRECTORY = @"%appdata%\Probel\Perdeval\Logs\";
        private const string TITLE_PREFIX = "Log reader";
        private readonly IFormatProvider _formatProvider = new CultureInfo("en-US");
        private readonly ILogService _logger;
        private readonly ILogRepository LogRepository;
        private ObservableCollection<string> _categories = new ObservableCollection<string>();
        private Encoding _fileEncoding = Encoding.GetEncoding("Windows-1252");
        private decimal _filterMinutes = 0;
        private ObservableCollection<LogModel> _logs = new ObservableCollection<LogModel>();
        private SettingsViewModel _settings;
        private string _status = Messages.Status_Ready;
        private string _statusItemsCount = string.Format(Messages.Status_xxItems, 0);
        private string _title = TITLE_PREFIX;
        private string _version;
        private string FilePath;
        private FileSystemWatcher FileWatcher;
        public readonly string DefaultDirectory;

        #endregion Fields

        #region Constructors

        public ShellViewModel(ILogRepository logRepository, ICommandBuilder commandBuilder, ILogService logger)
        {
            _logger = logger;
            var v = Assembly.GetEntryAssembly().GetName().Version;
            Version = string.Format(Messages.Status_Version, v.ToString(3));

            LogRepository = logRepository;
            DefaultDirectory = Environment.ExpandEnvironmentVariables(DEFAULT_DIRECTORY);

            FilterCommand = commandBuilder.BuildAsyncCommand<string>(FilterAsync, CanFilter);
        }

        #endregion Constructors

        #region Properties

        private IEnumerable<LogModel> BufferLogs { get; set; }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value, nameof(Categories));
        }

        public Encoding FileEncoding
        {
            get => _fileEncoding;
            set => SetProperty(ref _fileEncoding, value, nameof(FileEncoding));
        }

        public ICommand FilterCommand { get; }

        public decimal FilterMinutes
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

        #endregion Properties

        #region Methods

        private void AddFileInHistory(string filePath)
        {
            var doubloon = (from f in Settings.FileHistory
                            where f == filePath
                            select f).Count() > 0;

            if (!doubloon) { Settings.FileHistory.Add(filePath); }
        }

        private bool CanFilter(string arg)
        {
            var isNumber = (arg == null) ? true : decimal.TryParse(arg, NumberStyles.AllowDecimalPoint, _formatProvider, out var r);
            var hasLogs = BufferLogs != null && BufferLogs.Count() != 0;
            return isNumber && hasLogs;
        }

        private void FillLogs(IEnumerable<LogModel> models)
        {
            Logs = new ObservableCollection<LogModel>(models);
            SetItemsCount();
        }

        private async Task FilterAsync(string arg)
        {
            var now = DateTime.Now;
            if (decimal.TryParse(arg, NumberStyles.AllowDecimalPoint, _formatProvider, out var value))
            {
                FilterMinutes = value;
            }

            var logs = await Task.Run(() => BufferLogs.Filter(FilterMinutes, Settings)
                                                      .Filter(Categories, Settings));
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
            BufferLogs = await Task.Run(() => LogRepository.GetAllLogs(FilePath, FileEncoding));
            FillLogs(BufferLogs.Filter(FilterMinutes, Settings).Filter(Categories, Settings));
            Status = Messages.Status_FileChanged + $" - [{DateTime.Now.ToLongTimeString()}]";
        }

        private async Task OpenAsync(string filePath)
        {
            AddFileInHistory(filePath);
            FilterMinutes = 0;
            FilePath = filePath;
            BufferLogs = await Task.Run(() => LogRepository.GetAllLogs(filePath, FileEncoding));
            RefillCategories(GetCategories());
            ListenToFileChange();

            FilterCommand.RaiseCanExecuteChanged();
            if (CanFilter("0")) { await FilterAsync("0"); }
            Status = Messages.Status_FileLoaded;
        }

        private void SetItemsCount() => StatusItemsCount = string.Format(Messages.Status_xxItems, Logs.Count);

        internal void FilterCategories(IEnumerable<string> categories) => FillLogs(BufferLogs.Filter(categories, Settings));

        internal IEnumerable<string> GetCategories() => BufferLogs.GetCategories();

        internal async Task OpenFileAsync(string path) => await Task.Run(() => OpenAsync(path));

        public string GetLastFile()
        {
            return (Settings.FileHistory.Count > 0)
                ? Settings.FileHistory.OrderBy(e => e).Last()
                : string.Empty;
        }

        public async Task Load(Encoding encoding)
        {
            var lastFile = GetLastFile();
            if (File.Exists(lastFile))
            {
                _logger.Debug($"Load last opened file. Path: '{lastFile}'");

                await OpenAsync(lastFile);
                Title = lastFile;
            }
            else { _logger.Warn($"Cannot load last opened file. Path: '{(lastFile ?? "<empty>")}'"); }
        }

        public void RefillCategories(IEnumerable<string> categories)
        {
            Categories.Clear();
            Categories.AddRange(categories);
        }

        #endregion Methods
    }
}