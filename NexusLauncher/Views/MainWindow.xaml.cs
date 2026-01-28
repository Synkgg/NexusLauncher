using Game_Launcher.Core;
using Game_Launcher.ViewModels;
using Game_Launcher.Views;
using System;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Game_Launcher
{


    public partial class MainWindow : Window
    {

        public static MainWindow Instance;


        private bool _OptionsOpen = false;

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            Instance = this;

            UpdateLauncherMenuVisibility();
        }

        private void GamesButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new GamesViewModel();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new HomeViewModel();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DataContext = new HomeViewModel();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnFullscreen_Click(object sender, RoutedEventArgs e)
        {
            //if (WindowState == WindowState.Maximized)
            //{
            //    WindowState = WindowState.Normal;
            //    btnFullscreen.Content = "🗖";
            //}
            //else
            //{
            //    WindowState = WindowState.Maximized;
            //    btnFullscreen.Content = "🗗";
            //}
        }
        
        private void NewsButton_Click(Object sender, RoutedEventArgs e)
        {
            DataContext = new NewsViewModel();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void LogoButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _OptionsOpen = !_OptionsOpen;
            LogoButton.ContextMenu.PlacementTarget = LogoButton;
            LogoButton.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            LogoButton.ContextMenu.IsOpen = _OptionsOpen;
        }

        private void Settings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var settings = new Views.SettingsWindow
            {
                Owner = this
            };
            settings.ShowDialog();
        }

        private void Update_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LauncherUpdateManager.StartUpdate();
            Application.Current.Shutdown();
        }

        private void UpdateLauncherMenuVisibility()
        {
            if (LauncherUpdateManager.UpdateAvailable)
            {
                UpdateLauncherMenuItem.Header = $"Update Launcher ({LauncherUpdateManager.RemoteVersion})";
                UpdateLauncherMenuItem.Visibility = Visibility.Visible;
                UpdateBadge.Visibility = Visibility.Visible;
            }
            else
            {
                UpdateLauncherMenuItem.Visibility = Visibility.Collapsed;
                UpdateBadge.Visibility = Visibility.Collapsed;
            }
        }

    }
}


