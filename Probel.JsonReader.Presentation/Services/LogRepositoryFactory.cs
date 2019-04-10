using Probel.JsonReader.Business;
using Probel.JsonReader.Business.Data;
using Probel.JsonReader.Business.Oracle.Data;
using Probel.JsonReader.Presentation.Constants;
using Probel.JsonReader.Presentation.ViewModels;

namespace Probel.JsonReader.Presentation.Services
{
    public class LogRepositoryFactory : ILogRepositoryFactory
    {
        #region Fields

        private readonly SettingsViewModel _settings;

        #endregion Fields

        #region Constructors

        public LogRepositoryFactory(SettingsViewModel settings)
        {
            _settings = settings;
        }

        #endregion Constructors

        #region Methods

        public ILogRepository Get()
        {
            switch (_settings.RepositoryType)
            {
                case RepositoryType.OracleDatabase: return new OracleLogRepository();
                default: return new CsvLogRepository();
            }
        }

        #endregion Methods
    }
}