using Prism.Mvvm;
using Probel.JsonReader.Business.Data;
using Probel.JsonReader.Presentation.Services;
using System.ComponentModel;

namespace Probel.JsonReader.Presentation.ViewModels
{
    public class SettingsViewModel : BindableBase, IFilter
    {
        #region Fields

        private readonly ISerialisationService SerialisationService;
        private bool _isLoggerVisible = false;
        private bool _isSortAscending = false;
        private bool _isThreadIdVisible = false;
        private bool _showDebug = true;
        private bool _showError = true;
        private bool _showFatal = true;
        private bool _showInfo = true;
        private bool _showWarning = true;

        #endregion Fields

        #region Constructors

        public SettingsViewModel(ISerialisationService serialisationService)
        {
            SerialisationService = serialisationService;
            PropertyChanged += OnSave;
        }

        #endregion Constructors

        #region Properties

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

        public bool ShowWarning
        {
            get => _showWarning;
            set => SetProperty(ref _showWarning, value, nameof(ShowWarning));
        }

        #endregion Properties

        #region Methods

        private void OnSave(object sender, PropertyChangedEventArgs e) => SerialisationService.Serialise(this);

        #endregion Methods
    }
}