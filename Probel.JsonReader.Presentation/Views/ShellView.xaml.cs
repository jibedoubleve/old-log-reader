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
        #region Fields

        private readonly ILogService _logger;

        #endregion Fields

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

        private void OnCategoryClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                mi.IsChecked = !mi.IsChecked;

                var categories = (from m in _menuCategories.Items.Cast<MenuItem>()
                                  where m.IsChecked == true
                                  select (string)m.Header).ToList();

                ViewModel.FilterCategories(categories);
            }
        }

        private void OnClickHistory(object sender, RoutedEventArgs e) => Refresh();

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
            Refresh();
        }

        private async void OpenFile(string path)
        {
            if (File.Exists(path))
            {
                ViewModel.Title = path;
                await ViewModel.OpenFileAsync(path);
                Refresh();
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
                    Refresh();
                }
            }
        }

        private void RefreshCategories()
        {
            _menuCategories.Items.Clear();
            var categories = ViewModel.GetCategories();

            for (var i = 0; i < categories.Count(); i++)
            {
                var btn = new MenuItem()
                {
                    Header = categories.ElementAt(i),
                    IsChecked = true
                };
                btn.Click += OnCategoryClick;

                _menuCategories.Items.Insert(i, btn);
            }
        }
        private void Refresh()
        {
            RefreshFileHistory();
            RefreshCategories();
        }
        private void RefreshFileHistory()
        {
            _menuHistory.Items.Clear();
            var history = ViewModel.Settings.FileHistory.OrderBy(h => h).ToList();
            var i = 0;

            for (i = 0; i < history.Count(); i++)
            {
                var btn = new MenuItem() { Header = history[i] };
                btn.Click += OnMenuClick;

                _menuHistory.Items.Insert(i, btn);
            }

            //var btn_all = new MenuItem() { Header = Messages.Menu_Category_All };

            //var btn_none = new MenuItem() { Header = Messages.Menu_Category_None };

            //_menuHistory.Items.Insert(++i, btn_all);
            //_menuHistory.Items.Insert(++i, btn_none);

        }

        #endregion Methods

    }
}