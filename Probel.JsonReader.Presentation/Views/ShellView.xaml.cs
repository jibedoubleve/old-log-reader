using Microsoft.Win32;
using Probel.JsonReader.Presentation.Helpers;
using Probel.JsonReader.Presentation.Properties;
using Probel.JsonReader.Presentation.Services;
using Probel.JsonReader.Presentation.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

        private const string UNSELECT_ALL = "unselect_all";
        private readonly ILogService _logger;

        #endregion Fields

        #region Constructors

        public ShellView(ILogService logger)
        {
            _logger = logger;
            InitializeComponent();
            _encodingLabel.Content = _menuIsWindows1252.Header;
        }

        #endregion Constructors

        #region Properties

        private string LastestFile { get; set; }

        private ShellViewModel ViewModel => DataContext as ShellViewModel;

        #endregion Properties

        #region Methods

        private MenuItem BuildUnselectAll()
        {
            var btn = new MenuItem()
            {
                Header = "Unselect all",
                IsChecked = false,
            };
            btn.Click += OnUnselectAllCategories;
            return btn;
        }

        private Encoding GetEncoding()
        {
            if (_menuIsUtf8.IsChecked) { return Encoding.UTF8; }
            else if (_menuIsWindows1252.IsChecked) { return Encoding.GetEncoding("Windows-1252"); }
            else { return Encoding.UTF8; }
        }

        private void OnCategoryClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                mi.IsChecked = !mi.IsChecked;

                var collection = (from m in _menuCategories.Items.Cast<object>()
                                  where m is MenuItem
                                  select m).Cast<MenuItem>().ToList();

                var categories = (from m in collection
                                  where m.IsChecked == true
                                  select (string)m.Header).ToList();

                ViewModel.RefillCategories(categories);

                ViewModel.FilterCommand.TryExecute(ViewModel.FilterMinutes.ToString());
            }
        }

        private void OnClickHistory(object sender, RoutedEventArgs e) => Refresh();

        private void OnEncodingClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {
                var value = item.IsChecked;

                foreach (MenuItem i in _menuEncoding.Items)
                {
                    if (i == sender) { i.IsChecked = !value; }
                    else { i.IsChecked = value; }
                }
            }
        }

        private void OnEncodingSelection(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi) { _encodingLabel.Content = mi.Header; }
        }

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

        private void OnOpenInExplorer(object sender, RoutedEventArgs e)
        {
            var lastFile = ViewModel.GetLastFile();
            var dir = string.Empty;

            dir = (File.Exists(lastFile))
                ? Path.GetDirectoryName(lastFile)
                : Path.GetDirectoryName(LastestFile);

            if (Directory.Exists(dir)) { Process.Start(dir); }
            else { _logger.Warn($"Directory '{dir}' does not exist."); }
        }

        private void OnSaveExceptionInClipboard(object sender, RoutedEventArgs e)
        {
            var txt = tbException.Text;
            Clipboard.SetText(txt);
        }

        private void OnSaveMessageInClipboard(object sender, RoutedEventArgs e)
        {
            var txt = tbMessage.Text;
            Clipboard.SetText(txt);
        }

        private void OnShowColumn(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                mi.IsChecked = !mi.IsChecked;
            }
        }

        private void OnUnselectAllCategories(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
            {
                var categories = new List<string>();
                ViewModel.RefillCategories(categories);
                ViewModel.FilterCommand.TryExecute(ViewModel.FilterMinutes.ToString());

                foreach (var menu in _menuCategories.Items)
                {
                    if (menu is MenuItem mi) { mi.IsChecked = false; }
                }
            }
        }

        private async void OnWindowLoad(object sender, RoutedEventArgs e)
        {
            await ViewModel.Load(GetEncoding());
            Refresh();
        }

        private async void OpenFile(string path)
        {
            if (File.Exists(path))
            {
                LastestFile = path;
                ViewModel.Title = path;
                await ViewModel.OpenFileAsync(path, GetEncoding());
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

        private void Refresh()
        {
            RefreshFileHistory();
            RefreshCategories();
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

            _menuCategories.Items.Add(new Separator());
            _menuCategories.Items.Add(BuildUnselectAll());
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
        }

        #endregion Methods
    }
}