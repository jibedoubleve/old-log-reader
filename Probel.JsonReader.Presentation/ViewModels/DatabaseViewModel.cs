using Microsoft.Practices.Unity;
using Prism.Mvvm;
using Probel.JsonReader.Presentation.Helpers;

namespace Probel.JsonReader.Presentation.ViewModels
{
    public class DatabaseViewModel : BindableBase
    {
        #region Fields

        private string _connectionString;

        private SettingsViewModel _settings;

        #endregion Fields

        #region Constructors

        public DatabaseViewModel(ICommandBuilder commandBuilder)
        {
        }

        #endregion Constructors

        #region Properties

        public string ConnectionString
        {
            get => _connectionString;
            set => SetProperty(ref _connectionString, value, nameof(ConnectionString));
        }

        [Dependency]
        public SettingsViewModel Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value, nameof(Settings));
        }

        #endregion Properties

        #region Methods

        public void ProcessOk()
        {
            if (!string.IsNullOrEmpty(ConnectionString))
            {
                Settings.OracleDbHistory.Add(ConnectionString);
            }
        }

        #endregion Methods
    }
}