using Microsoft.Win32;
using Probel.JsonReader.Presentation.Helpers;
using Probel.JsonReader.Presentation.Properties;
using Probel.JsonReader.Presentation.Services;
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
        private readonly ILogService _logger;
        #region Constructors

        public ShellView(ILogService logger)
        {
            _logger = logger;
            InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        private ShellViewModel ViewModel => DataContext as ShellViewModel;

        #endregion Properties

        #region Methods

        private void OnClickHistory(object sender, RoutedEventArgs e) => RefreshFileHistory();

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
        private async void OnWindowLoad(object sender, RoutedEventArgs e)
        {
            await ViewModel.Load();
            RefreshFileHistory();
        }

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
                _logger.Warn($"Cannot open the logs. File '{path}' does not exist.");
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
            _buttonHistory.Items.Clear();
            var history = ViewModel.Settings.FileHistory.OrderBy(i => i).ToList();

            for (var i = 0; i < history.Count(); i++)
            {
                var btn = new MenuItem() { Header = history[i] };
                btn.Click += OnMenuClick;

                _buttonHistory.Items.Insert(i, btn);
            }
        }

        #endregion Methods
    }
}