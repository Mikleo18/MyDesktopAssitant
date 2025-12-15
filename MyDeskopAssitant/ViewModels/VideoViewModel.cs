using MyDeskopAssitant.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Threading;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;


namespace MyDeskopAssitant.ViewModels
{
    public class VideoViewModel:BaseViewModel
    {
        private string _videoSource;
        private bool _isPlaying;
        private double _currentPosition;
        private double _sliderMaximum;
        private string _currentTimeDisplay ="00:00";
        private string _videoTitle ="Unknown Title";
        private string _fileSizeInfo;
        private DispatcherTimer _timer;
        private bool _isFullScreen;

        public VideoViewModel()
        {
            OpenVideoCommand = new RelayCommand(ExecuteOpenVideo);
            PlayPauseCommand = new RelayCommand(ExecutePlayPause, CanExecuteControls);
            StopCommand = new RelayCommand(ExecuteStop, CanExecuteControls);
            PropertiesCommand = new RelayCommand(ExecuteProperties, CanExecuteControls);
            FullscreenCommand = new RelayCommand(ExecuteFullscreen);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (IsPlaying && CurrentPosition < SliderMaximum)
            {
                CurrentPosition += 1;
            }
        }

        public string VideoSource
        {
            get => _videoSource;
            set { _videoSource = value; OnPropertyChanged(); }
        }

        public string VideoTitle
        {
            get => _videoTitle;
            set { _videoTitle = value; OnPropertyChanged(); }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged();
                if (_isPlaying) _timer.Start(); else _timer.Stop();
            }
        }

        public double SliderMaximum
        {
            get => _sliderMaximum;
            set { _sliderMaximum = value; OnPropertyChanged(); }
        }

        public double CurrentPosition
        {
            get => _currentPosition;
            set
            {
                _currentPosition = value;
                OnPropertyChanged();
                UpdateTimeDisplay(value);
            }
        }

        public string CurrentTimeDisplay
        {
            get => _currentTimeDisplay;
            set { _currentTimeDisplay = value; OnPropertyChanged(); }
        }

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set { _isFullScreen = value; OnPropertyChanged(); }
        }

        public ICommand OpenVideoCommand { get; }
        public ICommand PlayPauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PropertiesCommand { get; }
        public ICommand FullscreenCommand { get; }


        private bool CanExecuteControls(object obj) => !string.IsNullOrEmpty(VideoSource);

        private void ExecuteOpenVideo(object obj)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv|All Files|*.*";

            if (ofd.ShowDialog() == true)
            {
                VideoSource = ofd.FileName;
                VideoTitle = Path.GetFileName(VideoSource);

                FileInfo fi = new FileInfo(ofd.FileName);
                double sizeMb = fi.Length / (1024.0 * 1024.0);
                _fileSizeInfo = $"File Name: {fi.Name}\nSize: {sizeMb:F2} MB\nPath: {fi.FullName}";

                IsPlaying = true;
            }
        }

        private void ExecutePlayPause(object obj)
        {
            IsPlaying = !IsPlaying;
        }

        private void ExecuteStop(object obj)
        {
            IsPlaying = false;
            CurrentPosition = 0;
        }

        private void ExecuteProperties(object obj)
        {
            MessageBox.Show(_fileSizeInfo,"Video Properties", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateTimeDisplay(double seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            CurrentTimeDisplay = $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        private void ExecuteFullscreen(object obj)
        {
            IsFullScreen = !IsFullScreen;
        }

    }
}
