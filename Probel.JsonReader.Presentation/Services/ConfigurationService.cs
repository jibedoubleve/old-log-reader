using System.Configuration;

namespace Probel.JsonReader.Presentation.Services
{
    public interface IConfigurationService
    {
        #region Properties

        string AppDirectory { get; }
        string DefaultLogDirectory { get; }

        #endregion Properties
    }

    public class ConfigurationService : IConfigurationService
    {
        #region Fields

        private const string APP_DIRECTORY = "AppDirectory";
        private const string DEFAULT_LOG_DIRECTORY = "DefaultLogDirectory";

        #endregion Fields

        #region Properties

        public string AppDirectory => ConfigurationManager.AppSettings[APP_DIRECTORY];
        public string DefaultLogDirectory => ConfigurationManager.AppSettings[DEFAULT_LOG_DIRECTORY];

        #endregion Properties
    }
}