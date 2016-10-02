using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace ZombieStandardTime
{
    public partial class MainWindow : Window
    {
        private readonly ViewModel _model;

        public MainWindow()
        {
            InitializeComponent();

            _model = new ViewModel();
            _model.Initialize();

            Loaded += _model.MainWindowLoaded;
            Closing += _model.MainWindowClosing;
            DataContext = _model;
        }

        private void OverridePathToGame_Click(object sender, RoutedEventArgs e)
        {
            _model.BrowseForPathToGame();
        }

        private void OverridePathToUserData_Click(object sender, RoutedEventArgs e)
        {
            _model.BrowseForPathToUserData();
        }

        private void Launch_Click(object sender, RoutedEventArgs e)
        {
            _model.Launch();
        }

        private void RestoreAndLaunch_Click(object sender, RoutedEventArgs e)
        {
            _model.RestoreAndLaunch();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
