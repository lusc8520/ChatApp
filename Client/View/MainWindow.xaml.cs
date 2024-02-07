using System.Windows;
using System.Windows.Input;
using System.ServiceModel;
using de.hsfl.vs.hul.chatApp.contract;

namespace de.hsfl.vs.hul.chatApp.client.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void TopBarClick(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                var mousePos = e.GetPosition(this);
                Left = mousePos.X - ActualWidth / 2;
                Top = mousePos.Y - 15;
            }
            if (e.LeftButton != MouseButtonState.Pressed) return;
            DragMove();
        }

        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeClick(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                return;
            }
            WindowState = WindowState.Maximized;
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}