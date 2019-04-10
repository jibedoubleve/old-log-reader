using Microsoft.Win32;
using Probel.JsonReader.Presentation.Constants;
using Probel.JsonReader.Presentation.Helpers;
using Probel.JsonReader.Presentation.Properties;
using Probel.JsonReader.Presentation.Services;
using Probel.JsonReader.Presentation.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Probel.JsonReader.Presentation.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        #region Fields

        private const int CS_LENTH = 120;
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

        private string LatestFile { get; set; }
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

                ViewModel.RefillCategories(categories);

                ViewModel.FilterCommand.TryExecute(ViewModel.FilterMinutes.ToString());
            }
        }

        private void OnClickHistory(object sender, RoutedEventArgs e) => Refresh();

        private void OnFileMenuOpenMenu(object sender, RoutedEventArgs e)
        {
            //Refactore this.It smells pattern to be applied ;-)
            bool? result = null;
            string path = null;

            if (ViewModel.Settings.RepositoryType == RepositoryType.Csv)
            {
                var ofd = new OpenFileDialog { InitialDirectory = ViewModel.DefaultDirectory };
                result = ofd.ShowDialog();
                path = ofd.FileName;
            }
            else if (ViewModel.Settings.RepositoryType == RepositoryType.OracleDatabase)
            {
                var view = new DatabaseView { Owner = this };
                result = view.ShowDialog();
                path = view.ConnectionString;
            }

            if (result.HasValue && result.Value) { OpenFile(path); }
            else { ViewModel.Status = Messages.Status_Ready; }
        }

        private void OnFileMenuQuitMenu(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void OnMenuClick(object sender, RoutedEventArgs e) => OpenFile((sender as MenuItem).Tag.ToString());

        private void OnOpenInExplorer(object sender, RoutedEventArgs e)
        {
            var lastFile = ViewModel.GetLastFile();
            var dir = string.Empty;

            dir = (File.Exists(lastFile))
                ? Path.GetDirectoryName(lastFile)
                : Path.GetDirectoryName(LatestFile);

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

        private void OnSelectCsvSource(object sender, RoutedEventArgs e)
        {
            ViewModel.SetMode(RepositoryType.Csv);
            BtnOpenDir.Visibility = Visibility.Visible;
            RefreshSource();
        }

        private void OnSelectOracleDbSource(object sender, RoutedEventArgs e)
        {
            ViewModel.SetMode(RepositoryType.OracleDatabase);
            BtnOpenDir.Visibility = Visibility.Collapsed;
            RefreshSource();
        }

        private void OnShowColumn(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                mi.IsChecked = !mi.IsChecked;
            }
        }

        private async void OnWindowLoad(object sender, RoutedEventArgs e)
        {
            BtnOpenDir.Visibility = (ViewModel.Settings.RepositoryType == RepositoryType.Csv)
                ? Visibility.Visible
                : Visibility.Collapsed;

            await ViewModel.Load();

            Refresh();
        }

        private async void OpenFile(string path)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            if (ViewModel.Settings.RepositoryType == RepositoryType.Csv)
            {
                if (File.Exists(path))
                {
                    LatestFile = path;
                    ViewModel.Title = path;
                    await ViewModel.OpenAsync(path);
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
                Mouse.OverrideCursor = null;
            }
            else if (ViewModel.Settings.RepositoryType == RepositoryType.OracleDatabase)
            {
                try
                {
                    LatestFile = path;
                    ViewModel.Title = path.Substring(0, (path.Length > CS_LENTH) ? CS_LENTH : path.Length);
                    await ViewModel.OpenAsync(path);
                    Refresh();
                }
                catch (Exception ex)
                {
                    var toRemove = (from f in ViewModel.Settings.OracleDbHistory
                                    where f == path
                                    select f).FirstOrDefault();
                    if (toRemove != null)
                    {
                        ViewModel.Settings.OracleDbHistory.Remove(toRemove);
                        Refresh();
                    }

                    _logger.Error(ex.Message, ex);
                    var msg = ex.Message;
                    MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally { Mouse.OverrideCursor = null; }
            }
        }

        private void Refresh()
        {
            RefreshFileHistory();
            RefreshCategories();
            RefreshSource();
            RefreshDays();
        }

        private void RefreshDays()
        {
            _menuDays.Items.Clear();
            var days = ViewModel.GetDays();
            for (int i = 0; i < days.Count(); i++)
            {
                var btn = new MenuItem
                {
                    Header = days.ElementAt(i).ToString("dd-MMM-yyyy"),
                    IsChecked = false,
                };
                _menuDays.Items.Insert(i, btn);
            }
        }

        private void RefreshCategories()
        {
            _menuCategories.Items.Clear();
            var categories = ViewModel.GetCategories();

            for (var i = 0; i < categories.Count(); i++)
            {
                var btn = new MenuItem
                {
                    Header = categories.ElementAt(i),
                    IsChecked = true
                };
                btn.Click += OnCategoryClick;

                _menuCategories.Items.Insert(i, btn);
            }
        }

        private void RefreshFileHistory()
        {
            _menuHistory.Items.Clear();
            if (ViewModel.Settings.RepositoryType == RepositoryType.Csv)
            {
                var history = ViewModel.Settings.FileHistory.OrderBy(h => h).ToList();
                var i = 0;

                for (i = 0; i < history.Count(); i++)
                {
                    var btn = new MenuItem() { Header = history[i], Tag = history[i] };
                    btn.Click += OnMenuClick;

                    _menuHistory.Items.Insert(i, btn);
                }
            }
            else if (ViewModel.Settings.RepositoryType == RepositoryType.OracleDatabase)
            {
                var history = ViewModel.Settings.OracleDbHistory.OrderBy(h => h).ToList();
                var i = 0;

                for (i = 0; i < history.Count(); i++)
                {
                    var h = history[i];
                    var btn = new MenuItem() { Header = h.Substring(0, h.Length > CS_LENTH ? CS_LENTH : h.Length), Tag = h };
                    btn.Click += OnMenuClick;

                    _menuHistory.Items.Insert(i, btn);
                }
            }
        }

        private void RefreshSource()
        {
            switch (ViewModel.Settings.RepositoryType)
            {
                case RepositoryType.Csv:
                    IsCsvFile.IsChecked = true;
                    IsOracleDb.IsChecked = false;
                    break;

                case RepositoryType.OracleDatabase:
                    IsCsvFile.IsChecked = false;
                    IsOracleDb.IsChecked = true;
                    break;

                default: throw new NotSupportedException($"The repository type '{ViewModel.Settings.RepositoryType}' is not supported!");
            }

            RefreshFileHistory();
        }

        #endregion Methods
    }
}