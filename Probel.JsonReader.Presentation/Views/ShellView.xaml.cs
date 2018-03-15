using Microsoft.Win32;
using Probel.JsonReader.Presentation.Helpers;
using Probel.JsonReader.Presentation.Properties;
using Probel.JsonReader.Presentation.ViewModels;
using System.IO;
using System.Linq;
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

        private void OnFileMenuOpenMenu(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                InitialDirectory = ViewModel.DefaultDirectory
            };
            var result = ofd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                OpenFile(ofd.FileName);
            }
            else { ViewModel.Status = Messages.Status_Ready; }
        }

        private void OnFileMenuQuitMenu(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void OnMenuClick(object sender, RoutedEventArgs e) => OpenFile((sender as MenuItem).Header.ToString());

        private void OnShowColumn(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                mi.IsChecked = !mi.IsChecked;
            }
        }

        private void OnWindowLoad(object sender, RoutedEventArgs e) => RefreshFileHistory();

        private void OpenFile(string path)
        {
            if (File.Exists(path))
            {
                ViewModel.Title = path;
                ViewModel.OpenCommand.TryExecute(path);
                RefreshFileHistory();
            }
            else
            {
                var msg = Messages.Message_FileNotExist;
                MessageBox.Show(msg, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);

                var toRemove = (from f in ViewModel.Settings.FileHistory
                                where f == path
                                select f).FirstOrDefault();
                if (toRemove != null)
                {
                    ViewModel.Settings.FileHistory.Remove(toRemove);
                    RefreshFileHistory();
                }
            }
        }

        private void RefreshFileHistory()
        {
            var to = FileMenu.Items.Cast<object>().Count() - 3;
            for (var i = 2; i <= to; i++)
            {
                var item = FileMenu.Items.Cast<object>().ElementAt(2);
                if (item is MenuItem mi) { mi.Click -= OnMenuClick; }
                FileMenu.Items.RemoveAt(2);
            }
            foreach (var path in ViewModel.Settings.FileHistory)
            {
                var menu = new MenuItem() { Header = path };
                menu.Click += OnMenuClick;
                FileMenu.Items.Insert(2, menu);
            }
        }

        #endregion Methods
    }
}