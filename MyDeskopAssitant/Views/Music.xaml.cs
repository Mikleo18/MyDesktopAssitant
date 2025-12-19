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
            this.DataContextChanged += Music_DataContextChanged;
        }

        private void Music_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is MusicViewModel vm)
            {
                _viewModel = vm;
                // Play/Pause durumunu dinle
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;

                _viewModel.RequestSeek += (seconds) =>
                {
                    // MediaElement hazırsa pozisyonu güncelle
                    if (mediaElement.NaturalDuration.HasTimeSpan)
                    {
                        mediaElement.Position = TimeSpan.FromSeconds(seconds);
                    }
                };
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ViewModel'de IsPlaying değişince burası çalışır
            if (e.PropertyName == nameof(MusicViewModel.IsPlaying))
            {
                if (_viewModel.IsPlaying)
                    mediaElement.Play();
                else
                    mediaElement.Pause();
            }
        }

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan && _viewModel != null)
            {
                // Slider maksimum değerini ayarla
                _viewModel.SliderMaximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;

                // Şarkı yüklenince otomatik çal
                _viewModel.IsPlaying = true;
            }
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Şarkı bitince sonrakine geç
            if (_viewModel != null && _viewModel.NextSongCommand.CanExecute(null))
            {
                _viewModel.NextSongCommand.Execute(null);
            }
        }

        // Slider ile sarma işlemi
        private void slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Slider slider)
            {
                mediaElement.Position = TimeSpan.FromSeconds(slider.Value);
                // Sarma bitince çalmaya devam et
                _viewModel.IsPlaying = true;
            }
        }
    }
}
