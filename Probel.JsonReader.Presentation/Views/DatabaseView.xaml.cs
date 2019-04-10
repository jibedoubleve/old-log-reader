using Probel.JsonReader.Presentation.ViewModels;
using System.Windows;

namespace Probel.JsonReader.Presentation.Views
{
    /// <summary>
    /// Interaction logic for DatabaseView.xaml
    /// </summary>
    public partial class DatabaseView : Window
    {
        #region Constructors

        public DatabaseView()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        public string ConnectionString => ViewModel.ConnectionString;
        private DatabaseViewModel ViewModel => DataContext as DatabaseViewModel;

        #endregion Properties

        #region Methods

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            ViewModel.ProcessOk();
            DialogResult = true;
            Close();
        }

        #endregion Methods
    }
}