using Microsoft.Practices.Unity;
using Prism.Unity;
using Probel.JsonReader.Business;
using Probel.JsonReader.Business.Data;
using Probel.JsonReader.Presentation.Helpers;
using Probel.JsonReader.Presentation.Services;
using Probel.JsonReader.Presentation.Views;
using System.Windows;

namespace Probel.JsonReader.Presentation
{
    public class Bootstrapper : UnityBootstrapper
    {
        #region Methods

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            Container.RegisterType<IStatusService, StatusService>();
            Container.RegisterType<ICommandBuilder, DelegateCommandBuilder>();
            Container.RegisterType<ILogRepositoryFactory, LogRepositoryFactory>();
            Container.RegisterType<IConfigurationService, ConfigurationService>();
            Container.RegisterType<ISerialisationService, SerialisationService>();
            Container.RegisterType<ILogService, NLogService>();

            var vm = Container.Resolve<ISerialisationService>();
            Container.RegisterInstance(vm.DeserialiseSettings());
        }

        protected override DependencyObject CreateShell()
        {
            var shell = Container.Resolve<ShellView>();
            return shell;
        }

        protected override void InitializeShell() => Application.Current.MainWindow.Show();

        #endregion Methods
    }
}