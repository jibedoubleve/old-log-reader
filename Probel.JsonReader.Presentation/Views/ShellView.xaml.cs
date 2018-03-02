using Microsoft.Win32;
using Probel.JsonReader.Presentation.Helpers;
using Probel.JsonReader.Presentation.Properties;
using Probel.JsonReader.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Probel.JsonReader.Presentation.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        #region Constructors

        public ShellView()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        private ShellViewModel ViewModel => DataContext as ShellViewModel;

        #endregion Properties

        #region Methods

        private void OnOpenFile(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                InitialDirectory = ViewModel.DefaultDirectory
            };
            var result = ofd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                ViewModel.Title = ofd.FileName;
                ViewModel.OpenCommand.TryExecute(ofd.FileName);
            }
            else { ViewModel.Status = Messages.Status_Ready; }
        }

        #endregion Methods

        private void OnShowColumn(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                mi.IsChecked = !mi.IsChecked;
            }
        }

        private void OnQuit(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}