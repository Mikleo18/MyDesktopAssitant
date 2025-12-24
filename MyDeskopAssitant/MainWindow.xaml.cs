using MyDeskopAssitant.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MyDeskopAssitant
{
    public partial class MainWindow : Window
    {
        // --- WIDGET SÜRÜKLEME DEĞİŞKENLERİ ---
        private bool _isDragging = false;
        private Point _startMousePosition;   // Tıklama anındaki fare konumu
        private Point _startWidgetPosition;  // Tıklama anındaki widget konumu

        public MainWindow()
        {
            InitializeComponent();

            // 1. ViewModel Bağlantısı
            var vm = new MainViewModel();
            this.DataContext = vm;

            // 2. Olayları Başlat
            SetupPlayerEvents(vm);
        }

        // --- PLAYER OLAYLARI ---
        private void SetupPlayerEvents(MainViewModel mainVm)
        {
            // SEEK (Sarma)
            mainVm.MusicVm.RequestSeek += (seconds) =>
            {
                if (GlobalPlayer.NaturalDuration.HasTimeSpan)
                    GlobalPlayer.Position = TimeSpan.FromSeconds(seconds);
            };

            // PLAY/PAUSE
            mainVm.MusicVm.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(MusicViewModel.IsPlaying))
                {
                    if (mainVm.MusicVm.IsPlaying) GlobalPlayer.Play();
                    else GlobalPlayer.Pause();
                }
            };
        }

        private void GlobalPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel mainVm && GlobalPlayer.NaturalDuration.HasTimeSpan)
            {
                mainVm.MusicVm.SetDuration(GlobalPlayer.NaturalDuration.TimeSpan);
                mainVm.MusicVm.IsPlaying = true;
                GlobalPlayer.Play();
            }
        }

        private void GlobalPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel mainVm)
            {
                mainVm.MusicVm.OnSongEnded();
            }
        }

        // --- PENCERE KONTROLLERİ ---

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

        private void btnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void btnMinimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;


        // --- 👇 WIDGET SÜRÜKLEME KODLARI (SON HALİ) 👇 ---

        private void MusicWidget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 1. KRİTİK NOKTA: Bu tıklamanın pencereyi de sürüklemesini engelle
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
                e.Handled = true; // Olayı burada bitir
            }
        }
    }
}