using Microsoft.Win32;
using MyDeskopAssitant.Core;
using MyDeskopAssitant.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TagLib;

namespace MyDeskopAssitant.ViewModels
{
    public class MusicViewModel : BaseViewModel
    {
        private DispatcherTimer _timer;
        private bool _isplaying;
        private SongModel _currentSong;
        private double _currentPosition;
        private double _sliderMaximum;
        private string _currentTimeDisplay = "00:00";
        private ObservableCollection<SongModel> _playlist;
        public event Action<double> RequestSeek;
        private bool _isLooping;

        public MusicViewModel()
        {
            PlayPauseCommand = new RelayCommand(ExecutePlayPause);
            NextSongCommand = new RelayCommand(ExecuteNextSong, CanExecuteNextPrevSong);
            PreviousSongCommand = new RelayCommand(ExecutePreviousSong, CanExecuteNextPrevSong);
            AddSongCommand = new RelayCommand(ExecuteAddSong);
            RemoveSongCommand = new RelayCommand(ExecuteRemoveSong);
            SeekCommand = new RelayCommand(ExecuteSeek);
            ForwardCommand = new RelayCommand(ExecuteForward);
            RewindCommand = new RelayCommand(ExecuteRewind);
            MixCommand = new RelayCommand(ExecuteMix);
            LoopCommand = new RelayCommand(ExecuteLoop);
            SongEndedCommand = new RelayCommand(ExecuteSongEnded);


            _playlist = new ObservableCollection<SongModel>();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (CurrentSong == null || !IsPlaying) return;

            CurrentPosition += 1;

            if (CurrentPosition >= SliderMaximum && SliderMaximum > 0)
            {
                // Şarkı bittiğinde
                _timer.Stop();
                ExecuteNextSong(null);
            }
        }

        public bool IsLooping
        {
            get => _isLooping;
            set { _isLooping = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SongModel> Playlist
        {
            get => _playlist;
            set { _playlist = value; OnPropertyChanged(); }
        }

        public SongModel CurrentSong
        {
            get => _currentSong;
            set
            {
                _currentSong = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SongTitle));
                OnPropertyChanged(nameof(ComposerName));

                // Şarkı değiştiğinde pozisyonu ve süreyi sıfırla
                CurrentPosition = 0;
                CurrentTimeDisplay = "00:00";

                // Eğer şarkı null ise (liste temizlendiyse) çalmayı durdur
                if (_currentSong == null)
                {
                    IsPlaying = false;
                    SliderMaximum = 1; // Hata vermemesi için güvenli değer
                }
            }
        }

        public string SongTitle => CurrentSong?.Name ?? "Choose a song"; 
        public string ComposerName => CurrentSong?.Composer ?? "";

        public bool IsPlaying
        {
            get => _isplaying;
            set
            {
                // Şarkı yoksa asla true olamaz
                if (CurrentSong == null) _isplaying = false;
                else _isplaying = value;

                OnPropertyChanged();
                if (_isplaying) _timer.Start(); else _timer.Stop();
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

        private void UpdateTimeDisplay(double seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            CurrentTimeDisplay = $"{(int)timeSpan.TotalMinutes:D2}:{timeSpan.Seconds:D2}";
        } 

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

        private bool CanExecutePlay(object obj) => CurrentSong != null;
        private bool CanExecuteNextPrevSong(object obj) => Playlist != null && Playlist.Count > 0;
        private void ExecutePlayPause(object obj)
        {
            if (CurrentSong != null)
            {
                IsPlaying = !IsPlaying;
            }
        }

        private void ExecutePreviousSong(object obj)
        {
            if (Playlist.Count == 0 || CurrentSong == null) return;

            int currentIndex = Playlist.IndexOf(CurrentSong);
            if (currentIndex == -1) currentIndex = 0;

            int prevIndex = (currentIndex - 1 + Playlist.Count) % Playlist.Count;
            CurrentSong = Playlist[prevIndex];
        }

        private void ExecuteAddSong(object obj)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Audio Files|*.mp3;*.flac;*.wav|All Files|*.*";


            if (openFileDialog.ShowDialog() == true)
            {
                bool wasEmpty = Playlist.Count == 0;

                foreach (string filename in openFileDialog.FileNames)
                {
                    try
                    {
                        // TagLib ile dosyayı oku
                        var tfile = TagLib.File.Create(filename);

                        var song = new SongModel
                        {
                            Id = Playlist.Count + 1,
                            // Eğer Tag'de başlık varsa onu al, yoksa dosya adını al
                            Name = !string.IsNullOrWhiteSpace(tfile.Tag.Title) ? tfile.Tag.Title : Path.GetFileNameWithoutExtension(filename),

                            // Sanatçı bilgisini al
                            Composer = !string.IsNullOrWhiteSpace(tfile.Tag.FirstPerformer) ? tfile.Tag.FirstPerformer : "Bilinmeyen Sanatçı",

                            FilePath = filename,

                            // Süreyi direkt dosyadan al (MediaElement'i beklemeye gerek yok artık!)
                            Duration = tfile.Properties.Duration.ToString(@"mm\:ss"),

                            // Kapak resmini çek (Aşağıdaki yardımcı metodu kullanır)
                            AlbumArt = GetAlbumArt(tfile)
                        };

                        Playlist.Add(song);
                    }
                    catch (Exception ex)
                    {
                        // Dosya bozuksa veya okunamadıysa buraya düşer
                        System.Diagnostics.Debug.WriteLine($"Hata: {filename} okunamadı. {ex.Message}");
                    }
                }

                if (wasEmpty && Playlist.Count > 0)
                {
                    CurrentSong = Playlist[0];
                    // Otomatik başlatmak istersen: IsPlaying = true;
                    // Ama önce SliderMaximum'u ayarlamak için MediaElement'in yüklenmesini beklemek daha sağlıklıdır.
                }
            }
        }

        // Kapak Resmini Dönüştüren Yardımcı Metot
        private ImageSource GetAlbumArt(TagLib.File tfile)
        {
            // Dosyada resim var mı kontrol et
            if (tfile.Tag.Pictures.Length > 0)
            {
                try
                {
                    // İlk resmi al (Genelde kapak resmi 0. indekstir)
                    var bin = tfile.Tag.Pictures[0].Data.Data;

                    // Byte dizisini BitmapImage'a çevir
                    var image = new BitmapImage();
                    using (var mem = new MemoryStream(bin))
                    {
                        mem.Position = 0;
                        image.BeginInit();
                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        image.CacheOption = BitmapCacheOption.OnLoad; // Belleğe yükle
                        image.UriSource = null;
                        image.StreamSource = mem;
                        image.EndInit();
                    }
                    image.Freeze(); // UI Thread dışında erişim için dondur
                    return image;
                }
                catch
                {
                    return LoadDefaultImage();
                }
            }

            // Resim yoksa varsayılan resmi döndür
            return LoadDefaultImage();
        }

        private ImageSource LoadDefaultImage()
        {
            try
            {
                // WPF'te kaynak dosyalarına erişim formatı: pack://application:,,,/KLASÖR_ADI/DOSYA_ADI
                var uri = new Uri("pack://application:/Resources/pp31.jpg", UriKind.Absolute);
                return new BitmapImage(uri);
            }
            catch
            {
                // Eğer resim bulunamazsa null dönsün (program çökmesin)
                return null;
            }
        }

    

        private void ExecuteSeek(object position)
        {

        }

        private void ExecuteNextSong(object obj)
        {
            if(Playlist.Count == 0) return;

            int currentIndex = Playlist.IndexOf(CurrentSong);
            int nextIndex = (currentIndex + 1) % Playlist.Count;
            CurrentSong = Playlist[nextIndex];

            IsPlaying = false;
        }

        private void ExecuteForward(object obj)
        {
            if (CurrentSong == null) return;

            // SliderMaximum'u geçmemeli
            double newPos = CurrentPosition + 10;
            if (newPos > SliderMaximum) newPos = SliderMaximum;

            CurrentPosition = newPos;

            // View'a haber ver: "Medya oynatıcısını bu saniyeye al"
            RequestSeek?.Invoke(newPos);
        }

        private void ExecuteRewind(object obj)
        {
            if (CurrentSong == null) return;

            // 0'ın altına düşmemeli
            double newPos = CurrentPosition - 10;
            if (newPos < 0) newPos = 0;

            CurrentPosition = newPos;

            // View'a haber ver
            RequestSeek?.Invoke(newPos);
        }

        private void ExecuteMix(object obj)
        {
            // Listede karıştırılacak kadar şarkı yoksa çık
            if (Playlist.Count < 2) return;

            // 1. Şu an çalan şarkıyı sakla (Kaybetmeyelim)
            var current = CurrentSong;

            // 2. Listeyi rastgele karıştır (Geçici bir liste oluştur)
            var shuffledList = Playlist.OrderBy(x => Guid.NewGuid()).ToList();

            // 3. YENİ KISIM: ID'leri sırayla yeniden dağıt
            for (int i = 0; i < shuffledList.Count; i++)
            {
                // Listede 0. sıradaki şarkıya ID 1, 1. sıradakine ID 2 ver...
                shuffledList[i].Id = i + 1;
            }

            // 4. Ana listeyi güncelle (UI bunu algılar ve listeyi yeniler)
            Playlist = new ObservableCollection<SongModel>(shuffledList);

            // 5. Çalan şarkı referansını geri yükle (Kesinti olmasın)
            // Not: ID'si değişmiş olabilir ama nesne referansı aynı olduğu için sorun olmaz.
            CurrentSong = current;
        }

        // 2. LOOP (DÖNGÜ) MANTIĞI
        private void ExecuteLoop(object obj)
        {
            // True ise False, False ise True yap (Toggle)
            IsLooping = !IsLooping;
        }

        // 3. ŞARKI BİTİNCE NE OLSUN? (MediaEnded Eventi Burayı Çağıracak)
        private void ExecuteSongEnded(object obj)
        {
            if (IsLooping)
            {
                // Döngü açıksa başa sar ve oynat
                RequestSeek?.Invoke(0); // View'daki olayı tetikler
                IsPlaying = true;
            }
            else
            {
                // Döngü kapalıysa sıradaki şarkıya geç
                ExecuteNextSong(null);
            }
        }

        // 4. TRASH (SİL) MANTIĞI
        private void ExecuteRemoveSong(object parameter)
        {
            // Eğer parametre gelmişse (Button'dan tıklanan satır) onu sil
            // Gelmemişse o an seçili olanı (CurrentSong) sil
            var songToDelete = parameter as SongModel ?? CurrentSong;

            if (songToDelete != null)
            {
                // Eğer silinen şarkı şu an çalan şarkıysa, önce sonrakine geçelim veya durduralım
                if (songToDelete == CurrentSong)
                {
                    if (Playlist.Count > 1)
                    {
                        ExecuteNextSong(null); // Sonrakine geç
                    }
                    else
                    {
                        // Listede tek şarkı varsa ve onu siliyorsak
                        IsPlaying = false;
                        CurrentSong = null;
                    }
                }

                Playlist.Remove(songToDelete);
            }
        }


    }

}

   
