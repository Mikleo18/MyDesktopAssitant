using MyDeskopAssitant.Core;
using MyDeskopAssitant.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MyDeskopAssitant.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        // --- DEĞİŞKENLER ---
        private string _greetingText;
        private string _currentDate;
        private string _currentTime;

        private string _todoSummary;
        private string _calendarSummary;
        private string _calendarTimeSummary;

        private DispatcherTimer _timer;

        // Dosya Yolları (Programın çalıştığı klasörü baz alır)
        private readonly string _todoJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "todo_data.json");
        private readonly string _calendarJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "calendar_data.json");

        private string _weatherTemp;
        private string _weatherDesc;
        private string _weatherIconUrl;
        private string _weatherCity;

        // API Ayarları (Artık const değil, normal değişken)
        private string _apiKey;
        private const string City = "Istanbul";
        private const string Unit = "metric";

        private DateTime _lastTodoFileUpdate;
        private DateTime _lastCalendarFileUpdate;

        public MusicViewModel SharedMusic { get; }

        // --- CONSTRUCTOR (KURUCU METOT) ---
        public HomeViewModel()
        {
            // 1. API Key'i dosyadan oku
            _apiKey = GetWeatherApiKey();

            // 2. Zamanlayıcıyı başlat
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // 3. Başlangıç verilerini yükle
            GetWeatherAsync();
            UpdateTimeData();
            LoadSummaries();      // İlk açılışta özetleri yükle
            CheckForUpdates();    // Dosya kontrolünü başlat
        }

        // --- ZAMANLAYICI MANTIĞI ---
        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTimeData();
            CheckForUpdates(); // Her saniye dosya değişti mi diye bakar
        }

        private void UpdateTimeData()
        {
            var now = DateTime.Now;
            CurrentDate = now.ToString("dd MMMM yyyy, dddd");
            CurrentTime = now.ToString("HH:mm");

            if (now.Hour < 12) GreetingText = "Günaydın, Efendim.";
            else if (now.Hour < 18) GreetingText = "Tünaydın, Efendim.";
            else GreetingText = "İyi Akşamlar, Efendim.";
        }

        // --- ÖZET YÜKLEME MANTIKLARI ---
        private void LoadSummaries()
        {
            // Başlangıçta var olan dosyaları yüklemeyi dener
            CheckForUpdates();
        }

        private void CheckForUpdates()
        {
            // 1. TO-DO KONTROLÜ
            if (File.Exists(_todoJsonPath))
            {
                DateTime currentWriteTime = File.GetLastWriteTime(_todoJsonPath);
                if (currentWriteTime > _lastTodoFileUpdate)
                {
                    LoadTodoSummary();
                    _lastTodoFileUpdate = currentWriteTime;
                }
            }
            else
            {
                TodoSummary = "Görev Yok";
            }

            // 2. TAKVİM KONTROLÜ
            if (File.Exists(_calendarJsonPath))
            {
                DateTime currentWriteTime = File.GetLastWriteTime(_calendarJsonPath);
                if (currentWriteTime > _lastCalendarFileUpdate)
                {
                    LoadCalendarSummary();
                    _lastCalendarFileUpdate = currentWriteTime;
                }
            }
            else
            {
                CalendarSummary = "Plan Yok";
                CalendarTimeSummary = "-";
            }
        }

        private void LoadTodoSummary()
        {
            try
            {
                var json = File.ReadAllText(_todoJsonPath);
                var todos = JsonSerializer.Deserialize<ObservableCollection<ToDoModel>>(json);

                if (todos != null)
                {
                    var activeCount = todos.Count(t => !t.IsCompleted);
                    TodoSummary = $"{activeCount} Görev Bekliyor";
                }
            }
            catch
            {
                TodoSummary = "Veri Okunamadı";
            }
        }

        private void LoadCalendarSummary()
        {
            try
            {
                var json = File.ReadAllText(_calendarJsonPath);
                var events = JsonSerializer.Deserialize<ObservableCollection<CalendarEventModel>>(json);

                if (events != null)
                {
                    var nextEvent = events.Where(ev => ev.Date >= DateTime.Now)
                                          .OrderBy(ev => ev.Date)
                                          .FirstOrDefault();

                    if (nextEvent != null)
                    {
                        CalendarSummary = nextEvent.Title;
                        if (nextEvent.Date.Date == DateTime.Today)
                            CalendarTimeSummary = $"Bugün, {nextEvent.Date:HH:mm}";
                        else
                            CalendarTimeSummary = $"{nextEvent.Date:dd MMM}, {nextEvent.Date:HH:mm}";
                    }
                    else
                    {
                        CalendarSummary = "Etkinlik Yok";
                        CalendarTimeSummary = "-";
                    }
                }
            }
            catch
            {
                CalendarSummary = "Hata";
            }
        }

        // --- HAVA DURUMU VE API KEY ---

        public string GetWeatherApiKey()
        {
            // AppDomain.CurrentDomain.BaseDirectory -> Exe'nin olduğu klasörü verir.
            string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            if (File.Exists(configFile))
            {
                try
                {
                    string jsonString = File.ReadAllText(configFile);
                    using (JsonDocument doc = JsonDocument.Parse(jsonString))
                    {
                        return doc.RootElement
                                  .GetProperty("OpenWeatherMap")
                                  .GetProperty("ApiKey")
                                  .GetString();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"JSON Okuma Hatası: {ex.Message}");
                    return string.Empty;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"DOSYA BULUNAMADI: {configFile}");
            }

            return string.Empty;
        }

        private async void GetWeatherAsync()
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                WeatherDesc = "API Key Eksik";
                WeatherCity = "Ayarlar";
                WeatherTemp = "-";
                return;
            }

            string url = $"https://api.openweathermap.org/data/2.5/weather?q={City}&appid={_apiKey}&units={Unit}&lang=tr";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var weatherData = JsonSerializer.Deserialize<WeatherRoot>(jsonString, options);

                    if (weatherData != null)
                    {
                        WeatherTemp = $"{Math.Round(weatherData.Main.Temp)}°C";

                        string desc = weatherData.Weather[0].Description;
                        WeatherDesc = char.ToUpper(desc[0]) + desc.Substring(1);

                        WeatherCity = weatherData.CityName;

                        string iconCode = weatherData.Weather[0].Icon;
                        WeatherIconUrl = $"http://openweathermap.org/img/wn/{iconCode}@2x.png";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hava Durumu Hatası: {ex.Message}");
                WeatherTemp = "-";
                WeatherDesc = "Bağlantı Hatası";
                WeatherCity = "Hata";
            }
        }

        // --- PROPERTYLER (Arayüz Bağlantıları) ---
        public string GreetingText { get => _greetingText; set { _greetingText = value; OnPropertyChanged(); } }
        public string CurrentDate { get => _currentDate; set { _currentDate = value; OnPropertyChanged(); } }
        public string CurrentTime { get => _currentTime; set { _currentTime = value; OnPropertyChanged(); } }
        public string TodoSummary { get => _todoSummary; set { _todoSummary = value; OnPropertyChanged(); } }
        public string CalendarSummary { get => _calendarSummary; set { _calendarSummary = value; OnPropertyChanged(); } }
        public string CalendarTimeSummary { get => _calendarTimeSummary; set { _calendarTimeSummary = value; OnPropertyChanged(); } }
        public string WeatherTemp { get => _weatherTemp; set { _weatherTemp = value; OnPropertyChanged(); } }
        public string WeatherDesc { get => _weatherDesc; set { _weatherDesc = value; OnPropertyChanged(); } }
        public string WeatherIconUrl { get => _weatherIconUrl; set { _weatherIconUrl = value; OnPropertyChanged(); } }
        public string WeatherCity { get => _weatherCity; set { _weatherCity = value; OnPropertyChanged(); } }
    }
}