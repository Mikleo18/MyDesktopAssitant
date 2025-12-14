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

    public partial class Music : UserControl
    {
        private MusicViewModel _viewModel;

        public Music()
        {
            InitializeComponent();
            // ViewModel bağlandığında olayları dinlemeye başla
            this.DataContextChanged += Music_DataContextChanged;
        }

        private void Music_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is MusicViewModel vm)
            {
                _viewModel = vm;
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;

                
                _viewModel.RequestSeek += (seconds) =>
                {
                    if (mediaElement.NaturalDuration.HasTimeSpan)
                    {
                        mediaElement.Position = TimeSpan.FromSeconds(seconds);
                    }
                };
            }
        }

        // ViewModel'deki "IsPlaying" değişirse MediaElement'i kontrol et
        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MusicViewModel.IsPlaying))
            {
                if (_viewModel.IsPlaying)
                    mediaElement.Play();
                else
                    mediaElement.Pause();
            }
        }

        // Şarkı dosyası başarıyla açıldığında çalışır
        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan && _viewModel != null)
            {
                // 1. Slider'ın bitiş noktasını ayarla (YOKSA SLIDER İLERLEMEZ!)
                _viewModel.SliderMaximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;

                // 2. Otomatik çalmayı başlat
                _viewModel.IsPlaying = true;
            }
        }

        // Şarkı bittiğinde sonraki şarkıya geç
        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {

            if (!mediaElement.NaturalDuration.HasTimeSpan) return;

            if (_viewModel != null && _viewModel.NextSongCommand.CanExecute(null))
            {
                _viewModel.NextSongCommand.Execute(null);
            }
        }

        private void slider_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            double newTime = slider.Value;

            mediaElement.Position = TimeSpan.FromSeconds(newTime);

             _viewModel.IsPlaying = true; 
        }
    }
}
