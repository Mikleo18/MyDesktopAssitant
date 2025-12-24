using Microsoft.Win32;
using MyDeskopAssitant.Core;
using MyDeskopAssitant.Models;
using NAudio.Wave; // NAudio Kütüphanesi
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MyDeskopAssitant.ViewModels
{
    public class MusicViewModel : BaseViewModel
    {
        // --- NAUDIO BİLEŞENLERİ ---
        private IWavePlayer _wavePlayer;          // Ses Çıkış Cihazı
        private AudioFileReader _audioFileReader; // Ses Dosyası Okuyucusu

        // --- DEĞİŞKENLER ---
        private DispatcherTimer _timer;
        private bool _isplaying;
        private SongModel _currentSong;
        private double _currentPosition; // Saniye cinsinden
        private double _sliderMaximum;
        private string _currentTimeDisplay = "00:00";
        private ObservableCollection<SongModel> _songs;

        public event Action<double> RequestSeek;
        private bool _isLooping;
        private readonly string _savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "playlist_data.json");

        public MusicViewModel()
        {
            // Komutları Başlat
            PlayPauseCommand = new RelayCommand(ExecutePlayPause);
            NextSongCommand = new RelayCommand(ExecuteNextSong, CanExecuteNextPrevSong);
            PreviousSongCommand = new RelayCommand(ExecutePreviousSong, CanExecuteNextPrevSong);
            AddSongCommand = new RelayCommand(ExecuteAddSong);
            RemoveSongCommand = new RelayCommand(ExecuteRemoveSong);
            SeekCommand = new RelayCommand(ExecuteSeek); // Slider için
            ForwardCommand = new RelayCommand(ExecuteForward);
            RewindCommand = new RelayCommand(ExecuteRewind);
            MixCommand = new RelayCommand(ExecuteMix);
            LoopCommand = new RelayCommand(ExecuteLoop);
            SongEndedCommand = new RelayCommand(ExecuteSongEnded);

            _songs = new ObservableCollection<SongModel>();
            LoadPlaylist();

            // Timer Ayarları
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;
        }

        // --- EKSİK OLAN ICOMMAND ÖZELLİKLERİ (EKLENDİ) ---
        public ICommand PlayPauseCommand { get; }
        public ICommand NextSongCommand { get; }
        public ICommand PreviousSongCommand { get; }
        public ICommand AddSongCommand { get; }
        public ICommand RemoveSongCommand { get; }
        public ICommand SeekCommand { get; }
        public ICommand ForwardCommand { get; }
        public ICommand RewindCommand { get; }
        public ICommand MixCommand { get; }
        public ICommand LoopCommand { get; }
        public ICommand SongEndedCommand { get; }


        // --- PROPERTYLER ---

        public ObservableCollection<SongModel> Songs
        {
            get => _songs;
            set { _songs = value; OnPropertyChanged(); }
        }

        public SongModel CurrentSong
        {
            get => _currentSong;
            set
            {
                if (_currentSong == value) return;

                _currentSong = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SongTitle));
                OnPropertyChanged(nameof(ComposerName));

                // Şarkı değişince NAudio ile çalmayı başlat
                if (_currentSong != null)
                {
                    PlaySongInternal(_currentSong);
                }
                else
                {
                    StopAndDispose();
                }
            }
        }

        public string SongTitle => CurrentSong?.Name ?? "Bir şarkı seçin";
        public string ComposerName => CurrentSong?.Composer ?? "";

        public bool IsPlaying
        {
            get => _isplaying;
            set
            {
                if (_isplaying == value) return;
                _isplaying = value;
                OnPropertyChanged();
            }
        }

        public bool IsLooping
        {
            get => _isLooping;
            set { _isLooping = value; OnPropertyChanged(); }
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
                if (Math.Abs(_currentPosition - value) < 0.1) return;

                _currentPosition = value;
                OnPropertyChanged();
                UpdateTimeDisplay(value);
            }
        }

        public double TotalSeconds => SliderMaximum;
        public double CurrentSeconds => CurrentPosition;

        public string CurrentTimeDisplay
        {
            get => _currentTimeDisplay;
            set { _currentTimeDisplay = value; OnPropertyChanged(); }
        }

        // --- NAUDIO MANTIKLARI ---

        private void PlaySongInternal(SongModel song)
        {
            try
            {
                StopAndDispose();

                if (!File.Exists(song.FilePath)) return;

                _audioFileReader = new AudioFileReader(song.FilePath);
                _wavePlayer = new WaveOutEvent();

                _wavePlayer.Init(_audioFileReader);
                _wavePlayer.PlaybackStopped += OnPlaybackStopped;

                SliderMaximum = _audioFileReader.TotalTime.TotalSeconds;
                OnPropertyChanged(nameof(TotalSeconds));

                _wavePlayer.Play();
                IsPlaying = true;
                _timer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Oynatma Hatası: {ex.Message}");
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_audioFileReader != null)
            {
                CurrentPosition = _audioFileReader.CurrentTime.TotalSeconds;
                OnPropertyChanged(nameof(CurrentSeconds));
            }
        }

        private void StopAndDispose()
        {
            _timer.Stop();
            IsPlaying = false;
            CurrentPosition = 0;
            CurrentTimeDisplay = "00:00";

            if (_wavePlayer != null)
            {
                _wavePlayer.Stop();
                _wavePlayer.Dispose();
                _wavePlayer = null;
            }

            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (_audioFileReader != null && Math.Abs(_audioFileReader.CurrentTime.TotalSeconds - _audioFileReader.TotalTime.TotalSeconds) < 1)
            {
                ExecuteSongEnded(null);
            }
        }

        // --- KOMUT METOTLARI ---

        private bool CanExecuteNextPrevSong(object obj) => Songs != null && Songs.Count > 0;

        private void ExecutePlayPause(object obj)
        {
            if (_wavePlayer == null && CurrentSong != null)
            {
                PlaySongInternal(CurrentSong);
                return;
            }

            if (_wavePlayer != null)
            {
                if (_wavePlayer.PlaybackState == PlaybackState.Playing)
                {
                    _wavePlayer.Pause();
                    IsPlaying = false;
                    _timer.Stop();
                }
                else
                {
                    _wavePlayer.Play();
                    IsPlaying = true;
                    _timer.Start();
                }
            }
        }

        private void ExecuteSeek(object parameter)
        {
            if (_audioFileReader != null && parameter is double seconds)
            {
                _audioFileReader.CurrentTime = TimeSpan.FromSeconds(seconds);
                CurrentPosition = seconds;
            }
        }

        private void ExecuteNextSong(object obj)
        {
            if (Songs.Count == 0) return;

            int currentIndex = Songs.IndexOf(CurrentSong);
            int nextIndex = (currentIndex + 1) % Songs.Count;
            CurrentSong = Songs[nextIndex];
        }

        private void ExecutePreviousSong(object obj)
        {
            if (Songs.Count == 0 || CurrentSong == null) return;

            int currentIndex = Songs.IndexOf(CurrentSong);
            if (currentIndex == -1) currentIndex = 0;

            int prevIndex = (currentIndex - 1 + Songs.Count) % Songs.Count;
            CurrentSong = Songs[prevIndex];
        }

        private void ExecuteForward(object obj)
        {
            if (_audioFileReader == null) return;

            double newTime = _audioFileReader.CurrentTime.TotalSeconds + 10;
            if (newTime > _audioFileReader.TotalTime.TotalSeconds)
                newTime = _audioFileReader.TotalTime.TotalSeconds;

            _audioFileReader.CurrentTime = TimeSpan.FromSeconds(newTime);
            CurrentPosition = newTime;
        }

        private void ExecuteRewind(object obj)
        {
            if (_audioFileReader == null) return;

            double newTime = _audioFileReader.CurrentTime.TotalSeconds - 10;
            if (newTime < 0) newTime = 0;

            _audioFileReader.CurrentTime = TimeSpan.FromSeconds(newTime);
            CurrentPosition = newTime;
        }

        private void ExecuteMix(object obj)
        {
            if (Songs.Count < 2) return;

            var current = CurrentSong;
            List<SongModel> newOrderList;

            if (current != null)
            {
                var otherSongs = Songs.Where(s => s != current)
                                       .OrderBy(x => Guid.NewGuid())
                                       .ToList();
                newOrderList = new List<SongModel>();
                newOrderList.Add(current);
                newOrderList.AddRange(otherSongs);
            }
            else
            {
                newOrderList = Songs.OrderBy(x => Guid.NewGuid()).ToList();
            }

            for (int i = 0; i < newOrderList.Count; i++) newOrderList[i].Id = i + 1;

            Songs = new ObservableCollection<SongModel>(newOrderList);
            CurrentSong = current;
            SavePlaylist();
        }

        private void ExecuteLoop(object obj)
        {
            IsLooping = !IsLooping;
        }

        private void ExecuteSongEnded(object obj)
        {
            if (IsLooping)
            {
                if (_audioFileReader != null)
                {
                    _audioFileReader.CurrentTime = TimeSpan.Zero;
                    _wavePlayer.Play();
                    _timer.Start();
                    IsPlaying = true;
                }
            }
            else
            {
                ExecuteNextSong(null);
            }
        }

        private void ExecuteAddSong(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Audio Files|*.mp3;*.flac;*.wav|All Files|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                bool wasEmpty = Songs.Count == 0;

                foreach (string filename in openFileDialog.FileNames)
                {
                    try
                    {
                        var tfile = TagLib.File.Create(filename);
                        var song = new SongModel
                        {
                            Id = Songs.Count + 1,
                            Name = !string.IsNullOrWhiteSpace(tfile.Tag.Title) ? tfile.Tag.Title : Path.GetFileNameWithoutExtension(filename),
                            Composer = !string.IsNullOrWhiteSpace(tfile.Tag.FirstPerformer) ? tfile.Tag.FirstPerformer : "Bilinmeyen Sanatçı",
                            FilePath = filename,
                            Duration = tfile.Properties.Duration.ToString(@"mm\:ss"),
                            AlbumArt = GetAlbumArt(tfile)
                        };

                        Songs.Add(song);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Dosya Hatası: {filename} - {ex.Message}");
                    }
                }

                if (wasEmpty && Songs.Count > 0)
                {
                    CurrentSong = Songs[0];
                }
                SavePlaylist();
            }
        }

        private void ExecuteRemoveSong(object parameter)
        {
            var songToDelete = parameter as SongModel ?? CurrentSong;

            if (songToDelete != null)
            {
                if (songToDelete == CurrentSong)
                {
                    StopAndDispose();

                    if (Songs.Count > 1) ExecuteNextSong(null);
                    else CurrentSong = null;
                }
                Songs.Remove(songToDelete);
            }
            SavePlaylist();
        }

        // --- YARDIMCI METOTLAR ---

        private void UpdateTimeDisplay(double seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            CurrentTimeDisplay = $"{(int)timeSpan.TotalMinutes:D2}:{timeSpan.Seconds:D2}";
        }

        private ImageSource GetAlbumArt(TagLib.File tfile)
        {
            if (tfile.Tag.Pictures.Length > 0)
            {
                try
                {
                    var bin = tfile.Tag.Pictures[0].Data.Data;
                    var image = new BitmapImage();
                    using (var mem = new MemoryStream(bin))
                    {
                        mem.Position = 0;
                        image.BeginInit();
                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = null;
                        image.StreamSource = mem;
                        image.EndInit();
                    }
                    image.Freeze();
                    return image;
                }
                catch { return LoadDefaultImage(); }
            }
            return LoadDefaultImage();
        }

        private ImageSource LoadDefaultImage()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/Resources/pp31.jpg", UriKind.Absolute);
                return new BitmapImage(uri);
            }
            catch { return null; }
        }

        // --- SAVE / LOAD ---
        public void SavePlaylist()
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(Songs, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_savePath, jsonString);
            }
            catch { }
        }

        public void LoadPlaylist()
        {
            if (!File.Exists(_savePath)) return;
            try
            {
                string jsonString = File.ReadAllText(_savePath);
                var savedSongs = JsonSerializer.Deserialize<ObservableCollection<SongModel>>(jsonString);

                if (savedSongs != null)
                {
                    Songs.Clear();
                    foreach (var song in savedSongs)
                    {
                        if (File.Exists(song.FilePath))
                        {
                            try
                            {
                                var tfile = TagLib.File.Create(song.FilePath);
                                song.AlbumArt = GetAlbumArt(tfile);
                                Songs.Add(song);
                            }
                            catch
                            {
                                song.AlbumArt = LoadDefaultImage();
                                Songs.Add(song);
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}