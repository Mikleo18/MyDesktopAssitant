using MyDeskopAssitant.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MyDeskopAssitant
{
    public partial class MainWindow : Window
    {
        private bool _isDragging = false;
        private Point _startMousePosition;   
        private Point _startWidgetPosition;  

        public MainWindow()
        {
            InitializeComponent();

            var vm = new MainViewModel();
            this.DataContext = vm;


        }


        private void DragRow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Eğer tıklanan yer Widget ise PENCEREYİ sürüklemeyi engelle
            if (e.OriginalSource is FrameworkElement element && (element.Name == "MusicWidget" || IsParentWidget(element)))
            {
                return;
            }

            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Yardımcı Metot: Tıklanan şey Widget'ın içinde mi?
        private bool IsParentWidget(DependencyObject obj)
        {
            while (obj != null)
            {
                if (obj is FrameworkElement fe && fe.Name == "MusicWidget") return true;
                obj = VisualTreeHelper.GetParent(obj);
            }
            return false;
        }


        private void btnMinimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;



        private void MusicWidget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            e.Handled = true;
            _isDragging = true;
            _startMousePosition = e.GetPosition(this);
            _startWidgetPosition = new Point(WidgetTransform.X, WidgetTransform.Y);

            MusicWidget.CaptureMouse();
        }

        private void MusicWidget_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentMousePosition = e.GetPosition(this);

                // Mutlak Mesafe Hesabı (Fırlamayı önler)
                double deltaX = currentMousePosition.X - _startMousePosition.X;
                double deltaY = currentMousePosition.Y - _startMousePosition.Y;

                WidgetTransform.X = _startWidgetPosition.X + deltaX;
                WidgetTransform.Y = _startWidgetPosition.Y + deltaY;
            }
        }

        private void MusicWidget_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                MusicWidget.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MyNotifyIcon.ShowBalloonTip("Rika", "Arka planda çalışmaya devam ediyorum.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }


        private void MyNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show(); 
            this.WindowState = WindowState.Normal; 
            this.Activate(); 
        }


        private void ShowApp_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        
        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            MyNotifyIcon.Dispose();
            Application.Current.Shutdown();
        }
    }
}