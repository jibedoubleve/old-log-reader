using Microsoft.Practices.Unity;
using Prism.Mvvm;
using Probel.JsonReader.Business;
using Probel.JsonReader.Presentation.Constants;
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
        private readonly IFormatProvider _formatProvider = new CultureInfo("en-US");
        private readonly ILogService _logger;
        private readonly ILogRepositoryFactory _logRepositoryFactory;
        private ObservableCollection<string> _categories = new ObservableCollection<string>();
        private string _currentDay = Messages.Label_NoDate;
        private decimal _filterMinutes = 0;
        private bool _loadFailed;
        private ObservableCollection<LogModel> _logs = new ObservableCollection<LogModel>();
        private SettingsViewModel _settings;
        private string _status = Messages.Status_Ready;
        private string _statusItemsCount = string.Format(Messages.Status_xxItems, 0);
        private string _title = TITLE_PREFIX;
        private string _version;
        private FileSystemWatcher FileWatcher;

        #endregion Fields

        #region Constructors

        public ShellViewModel(ILogRepositoryFactory logRepositoryFactory, ICommandBuilder commandBuilder, ILogService logger)
        {
            _logger = logger;
            var v = Assembly.GetEntryAssembly().GetName().Version;
            Version = string.Format(Messages.Status_Version, v.ToString(3));

            _logRepositoryFactory = logRepositoryFactory;
            LogRepository = logRepositoryFactory.Get();
            DefaultDirectory = Environment.ExpandEnvironmentVariables(DEFAULT_DIRECTORY);

            OpenCommand = commandBuilder.BuildAsyncCommand<string>(OpenAsync, CanOpen);
            FilterCommand = commandBuilder.BuildAsyncCommand(FilterAsync);
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value, nameof(Categories));
        }

        public string CurrentDay
        {
            get => _currentDay;
            set => SetProperty(ref _currentDay, value, nameof(CurrentDay));
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

        public object Message { get; private set; }

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

        private ILogRepository LogRepository { get; set; }

        #endregion Properties

        #region Methods

        public async Task FilterAsync()
        {
            var logs = await Task.Run(() => LogRepository.Filter(Categories, 0, Settings));
            Logs = new ObservableCollection<LogModel>(logs);
            SetStatusBar();
            SetReady();
            _loadFailed = false;
        }

        public async Task FilterDay(IEnumerable<string> categories, DateTime day)
        {
            SetLoading();

            var logs = await Task.Run(() => LogRepository.Filter(categories, day, Settings));
            Logs = new ObservableCollection<LogModel>(logs);
            SetStatusBar();
            SetReady();
            _loadFailed = false;
        }

        public IEnumerable<DateTime> GetDays()
        {
            if (_loadFailed) { return new List<DateTime>(); }

            SetLoading();
            var result = LogRepository.GetDays();
            SetReady();
            return result;
        }

        public string GetLastFile()
        {
            if (Settings.RepositoryType == RepositoryType.Csv)
            {
                return (Settings.FileHistory.Count > 0)
                    ? Settings.FileHistory.OrderBy(e => e).Last()
                    : string.Empty;
            }
            else if (Settings.RepositoryType == RepositoryType.OracleDatabase)
            {
                return (Settings.OracleDbHistory.Count > 0)
                    ? Settings.OracleDbHistory.OrderBy(e => e).Last()
                    : string.Empty;
            }
            else { throw new NotSupportedException($"Repository of type '{Settings.RepositoryType}' is not supported!"); }
        }

        public async Task Load()
        {
            var lastFile = GetLastFile();
            if (File.Exists(lastFile))
            {
                _logger.Debug($"Load last opened file. Path: '{lastFile}'");
                await OpenAsync(lastFile);
                Title = lastFile;
                _loadFailed = false;
            }
            else if(Settings.RepositoryType == RepositoryType.OracleDatabase)
            {
                _logger.Debug($"Repository is Oracle Database. Let user loading it manually.");
                Title = lastFile.ToReadableCString();
                LogRepository.Setup(lastFile) ;
                _loadFailed = false;
            }
            else
            {
                _logger.Warn($"Cannot load last opened file. Path: '{(lastFile ?? "<empty>")}'");
                _loadFailed = true;
            }
        }

        public async Task OpenAsync(string filePath)
        {
            SetLoading();
            AddFileInHistory(filePath);
            FilterMinutes = 0;

            LogRepository.Setup(filePath);

            var logs = await Task.Run(() => LogRepository.GetLogs());
            Logs = new ObservableCollection<LogModel>(logs);
            RefillCategories(GetCategories());
            ListenToFileChange();

            await FilterAsync();
            Status = Messages.Status_FileLoaded;
        }

        public void RefillCategories(IEnumerable<string> categories)
        {
            Categories.Clear();
            Categories.AddRange(categories);
        }

        internal void FilterCategories(IEnumerable<string> categories)
        {
            SetLoading();
            FillLogs(LogRepository.Filter(categories, Settings));
            SetReady();
        }

        internal IEnumerable<string> GetCategories()
        {
            if (_loadFailed) { return new List<string>(); }

            SetLoading();
            var result = LogRepository.GetCategories();
            SetReady();
            return result;
        }

        internal void SetMode(RepositoryType mode)
        {
            Settings.RepositoryType = mode;
            LogRepository = _logRepositoryFactory.Get();
        }

        private void AddFileInHistory(string filePath)
        {
            if (Settings.RepositoryType == RepositoryType.Csv)
            {
                var doubloon = (from f in Settings.FileHistory
                                where f == filePath
                                select f).Count() > 0;

                if (!doubloon) { Settings.FileHistory.Add(filePath); }
            }
            else if (Settings.RepositoryType == RepositoryType.OracleDatabase)
            {
                var doubloon = (from cs in Settings.OracleDbHistory
                                where cs == filePath
                                select filePath).Count() > 0;

                if (!doubloon) { Settings.OracleDbHistory.Add(filePath); }
            }
            else { throw new NotSupportedException($"Cannot save history for repository of type '{Settings.RepositoryType}'"); }
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
            SetStatusBar();
        }

        private void ListenToFileChange()
        {
            if (FileWatcher != null)
            {
                FileWatcher.Changed -= OnFileChanged;
                FileWatcher = null;
            }

            if (Settings.RepositoryType == RepositoryType.Csv)
            {
                var path = Path.GetDirectoryName(LogRepository.GetSource());
                FileWatcher = new FileSystemWatcher()
                {
                    Path = path,
                    Filter = Path.GetFileName(LogRepository.GetSource()),
                };
                FileWatcher.Changed += OnFileChanged;
                FileWatcher.EnableRaisingEvents = true;
            }
        }

        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            SetLoading();
            var logs = await Task.Run(() => LogRepository.GetLogs());
            Logs = new ObservableCollection<LogModel>(logs);

            FillLogs(LogRepository.Filter(Categories, FilterMinutes, Settings));

            Status = Messages.Status_FileChanged + $" - [{DateTime.Now.ToLongTimeString()}]";
        }

        private void SetCurrentDay()
        {
            var day = (from l in Logs ?? new ObservableCollection<LogModel>()
                       select l.Time.Date).Distinct().ToList();

            if (day.Count <= 0) { CurrentDay = Messages.Label_NoDate; }
            else if (day.Count == 1) { CurrentDay = string.Format(Messages.Label_CurrentDate, day[0].ToString("dd-MMM-yyyy")); }
            else { CurrentDay = Messages.Label_MultipleDates; }
        }

        private void SetItemsCount() => StatusItemsCount = string.Format(Messages.Status_xxItems, Logs.Count);

        private void SetLoading() => Status = Messages.Status_Loading;

        private void SetReady() => Status = Messages.Status_Ready;

        private void SetStatusBar()
        {
            SetItemsCount();
            SetCurrentDay();
        }

        #endregion Methods
    }
}