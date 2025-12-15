using MyDeskopAssitant.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyDeskopAssitant.Views
{

    public partial class Video : UserControl
    {
        private VideoViewModel _viewModel; 
        public Video()
        {
            InitializeComponent();
            this.DataContextChanged += Video_DataContextChanged;
            this.Loaded += (s, e) => this.Focus();
        }

        private void Video_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.DataContext is VideoViewModel vm)
            {
                _viewModel = vm;
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(VideoViewModel.IsPlaying))
            {
                if(_viewModel.IsPlaying)
                    player.Play();
                else
                    player.Pause();

            }

            if(e.PropertyName == nameof(VideoViewModel.CurrentPosition) && _viewModel.CurrentPosition == 0 && !_viewModel.IsPlaying)
            {
                player.Stop();
            }

            if (e.PropertyName == nameof(VideoViewModel.IsFullScreen))
            {
                // Ana pencereye ulaş (MainWindow)
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    if (_viewModel.IsFullScreen)
                    {
                        // 1. TAM EKRAN MODU:
                        // Yan menüyü ve üst barı YOK ET (Gizle)
                        mainWindow.GridNav.Visibility = Visibility.Collapsed;
                        mainWindow.Tg_Btn.Visibility = Visibility.Collapsed;

                        // Pencereyi ekranı kaplayacak şekilde büyüt
                        mainWindow.WindowState = WindowState.Maximized;
                    }
                    else
                    {
                        // 2. NORMAL MOD:
                        // Her şeyi geri getir
                        mainWindow.GridNav.Visibility = Visibility.Visible;
                        mainWindow.Tg_Btn.Visibility = Visibility.Visible;


                        // Pencereyi normal boyutuna döndür
                        mainWindow.WindowState = WindowState.Normal;
                    }
                }
            }
        }

        private void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (player.NaturalDuration.HasTimeSpan)
            {
                _viewModel.SliderMaximum = player.NaturalDuration.TimeSpan.TotalSeconds;
            }
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {
            _viewModel.IsPlaying = false;
            _viewModel.CurrentPosition = 0;
            player.Stop();
        }

        private void progressSlider_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (player.NaturalDuration.HasTimeSpan)
            {
                player.Position = TimeSpan.FromSeconds(progressSlider.Value);
                _viewModel.IsPlaying = true;
            }
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _viewModel.IsFullScreen)
            {
                _viewModel.IsFullScreen = false; // ViewModel tetiklenir, yukarıdaki kod çalışır ve düzelir.
            }
        }




    }
}
