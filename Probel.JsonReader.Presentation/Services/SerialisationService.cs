using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Probel.JsonReader.Presentation.ViewModels;
using System;
using System.IO;

namespace Probel.JsonReader.Presentation.Services
{
    public interface ISerialisationService
    {
        #region Methods

        SettingsViewModel DeserialiseSettings();

        void Serialise(SettingsViewModel obj);

        #endregion Methods
    }

    public class SerialisationService : ISerialisationService
    {
        #region Fields

        private const string FILE_NAME = "settings.json";
        private readonly string AppDirectory;
        private readonly IUnityContainer Container;
        private readonly JsonSerializer Serializer = new JsonSerializer();

        #endregion Fields

        #region Constructors

        public SerialisationService(IConfigurationService cfgService, IUnityContainer container)
        {
            Container = container;
            AppDirectory = Environment.ExpandEnvironmentVariables(cfgService.AppDirectory);
        }

        #endregion Constructors

        #region Properties

        private string FilePath => Path.Combine(AppDirectory, FILE_NAME);

        #endregion Properties

        #region Methods

        public SettingsViewModel DeserialiseSettings()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var r = JsonConvert.DeserializeObject<SerialisedSettings>(json);
                var vm = Container.Resolve<SettingsViewModel>();
                r.Fill(vm);
                return vm;
            }
            else { return Container.Resolve<SettingsViewModel>(); }
        }

        public void Serialise(SettingsViewModel obj)
        {
            CheckDirectory();
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        private void CheckDirectory()
        {
            if (!Directory.Exists(AppDirectory)) { Directory.CreateDirectory(AppDirectory); }
        }

        #endregion Methods
    }

    public class SerialisedSettings
    {
        #region Properties

        [JsonProperty]
        public bool IsLoggerVisible
        {
            get; set;
        }

        [JsonProperty]
        public bool IsSortAscending { get; set; }

        [JsonProperty]
        public bool IsThreadIdVisible
        {
            get; set;
        }

        [JsonProperty]
        public bool ShowDebug
        {
            get; set;
        }

        [JsonProperty]
        public bool ShowError
        {
            get; set;
        }

        [JsonProperty]
        public bool ShowFatal
        {
            get; set;
        }

        [JsonProperty]
        public bool ShowInfo
        {
            get; set;
        }

        [JsonProperty]
        public bool ShowTrace { get; set; }

        [JsonProperty]
        public bool ShowWarning
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public void Fill(SettingsViewModel vm)
        {
            vm.IsLoggerVisible = IsLoggerVisible;
            vm.IsThreadIdVisible = IsThreadIdVisible;
            vm.ShowDebug = ShowDebug;
            vm.ShowInfo = ShowInfo;
            vm.ShowWarning = ShowWarning;
            vm.ShowTrace = ShowTrace;
            vm.ShowError = ShowError;
            vm.ShowFatal = ShowFatal;
            vm.IsSortAscending = IsSortAscending;
        }

        #endregion Methods
    }
}