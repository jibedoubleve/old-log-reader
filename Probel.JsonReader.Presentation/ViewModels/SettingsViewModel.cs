using Microsoft.Practices.Unity;
using Prism.Mvvm;
using Probel.JsonReader.Business.Data;
using Probel.JsonReader.Presentation.Constants;
using Probel.JsonReader.Presentation.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Probel.JsonReader.Presentation.ViewModels
{
    public class SettingsViewModel : BindableBase, IFilter
    {
        #region Fields

        private bool _isLoggerVisible = false;

        private bool _isSortAscending = false;

        private bool _isThreadIdVisible = false;

        private RepositoryType _repositoryType = RepositoryType.Csv;

        private bool _showDebug = true;

        private bool _showError = true;

        private bool _showFatal = true;

        private bool _showInfo = true;

        private bool _showTrace;

        private bool _showWarning = true;

        #endregion Fields

        #region Constructors

        public SettingsViewModel()
        {
            FileHistory = new ObservableCollection<string>();
            OracleDbHistory = new ObservableCollection<string>();
            PropertyChanged += OnPropertyChanged;
            FileHistory.CollectionChanged += OnFileCollectionChanged;
            OracleDbHistory.CollectionChanged += OnDbCollectionChanged;
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<string> FileHistory { get; }

        public bool IsLoggerVisible
        {
            get => _isLoggerVisible;
            set => SetProperty(ref _isLoggerVisible, value, nameof(IsLoggerVisible));
        }

        public bool IsSortAscending
        {
            get => _isSortAscending;
            set => SetProperty(ref _isSortAscending, value, nameof(IsSortAscending));
        }

        public bool IsThreadIdVisible
        {
            get => _isThreadIdVisible;
            set => SetProperty(ref _isThreadIdVisible, value, nameof(IsThreadIdVisible));
        }

        public ObservableCollection<string> OracleDbHistory { get; }

        public RepositoryType RepositoryType
        {
            get => _repositoryType;
            set => SetProperty(ref _repositoryType, value, nameof(RepositoryType));
        }

        [Dependency]
        public ISerialisationService SerialisationService { get; set; }

        public bool ShowDebug
        {
            get => _showDebug;
            set => SetProperty(ref _showDebug, value, nameof(ShowDebug));
        }

        public bool ShowError
        {
            get => _showError;
            set => SetProperty(ref _showError, value, nameof(ShowError));
        }

        public bool ShowFatal
        {
            get => _showFatal;
            set => SetProperty(ref _showFatal, value, nameof(ShowFatal));
        }

        public bool ShowInfo
        {
            get => _showInfo;
            set => SetProperty(ref _showInfo, value, nameof(ShowInfo));
        }

        public bool ShowTrace
        {
            get => _showTrace;
            set => SetProperty(ref _showTrace, value, nameof(ShowTrace));
        }

        public bool ShowWarning
        {
            get => _showWarning;
            set => SetProperty(ref _showWarning, value, nameof(ShowWarning));
        }

        #endregion Properties

        #region Methods

        private void OnDbCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var count = OracleDbHistory.Count();
            if (count > 5)
            {
                for (var i = 0; i < count - 5; i++) { OracleDbHistory.RemoveAt(i); }
            }

            SerialisationService.Serialise(this);
        }

        private void OnFileCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var count = FileHistory.Count();
            if (count > 5)
            {
                for (var i = 0; i < count - 5; i++) { FileHistory.RemoveAt(i); }
            }

            SerialisationService.Serialise(this);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SerialisationService.Serialise(this);
        }

        #endregion Methods
    }
}