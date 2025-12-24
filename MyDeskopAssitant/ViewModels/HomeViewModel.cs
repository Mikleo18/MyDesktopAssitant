using MyDeskopAssitant.Core;
using MyDeskopAssitant.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MyDeskopAssitant.ViewModels
{
    public class HomeViewModel:BaseViewModel
    {
        private string _greetingText;
        private string _currentDate;
        private string _currentTime;

        // Özet Bilgiler
        private string _todoSummary;
        private string _calendarSummary;
        private string _calendarTimeSummary;

        private DispatcherTimer _timer;

        // Dosya Yolları 
        private readonly string _todoJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "todo_data.json");
        private readonly string _calendarJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "calendar_data.json");

        private string _weatherTemp;
        private string _weatherDesc;
        private string _weatherIconUrl;
        private string _weatherCity;

        // API BİLGİLERİ
        private const string ApiKey = "YOUR API KEY";
        private const string City = "Istanbul"; 
        private const string Unit = "metric";

        private DateTime _lastTodoFileUpdate;
        private DateTime _lastCalendarFileUpdate;

        public MusicViewModel SharedMusic { get; }

        public HomeViewModel()
        {

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            GetWeatherAsync();
            UpdateTimeData();
            LoadSummaries();
            CheckForUpdates();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTimeData();
            CheckForUpdates();
        }

        private void UpdateTimeData()
        {
            var now = DateTime.Now;
            CurrentDate = now.ToString("dd MMMM yyyy, dddd");
            CurrentTime = now.ToString("HH:mm");

            // Saate göre selamlama
            if (now.Hour < 12) GreetingText = "Günaydın, Efendim.";
            else if (now.Hour < 18) GreetingText = "Tünaydın, Efendim.";
            else GreetingText = "İyi Akşamlar, Efendim.";
        }

        private void LoadSummaries()
        {
            try
            {
                if (File.Exists(_todoJsonPath))
                {
                    var todos = JsonSerializer.Deserialize<ObservableCollection<ToDoModel>>(File.ReadAllText(_todoJsonPath));
                    var activeCount = todos.Count(t => !t.IsCompleted);
                    TodoSummary = $"{activeCount} Görev Bekliyor";
                }
                else TodoSummary = "Görev Yok";
            }
            catch { TodoSummary = "Veri Okunamadı"; }

            try
            {
                if (File.Exists(_calendarJsonPath))
                {
                    var events = JsonSerializer.Deserialize<ObservableCollection<CalendarEventModel>>(File.ReadAllText(_calendarJsonPath));

                    var nextEvent = events.Where(e => e.Date >= DateTime.Now)
                                          .OrderBy(e => e.Date)
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
                else
                {
                    CalendarSummary = "Plan Yok";
                    CalendarTimeSummary = "-";
                }
            }
            catch { CalendarSummary = "Hata"; }
        }

        private void CheckForUpdates()
        {
            // 1. TO-DO DOSYASI KONTROLÜ
            if (File.Exists(_todoJsonPath))
            {
                // Dosyanın son değiştirilme tarihini al
                DateTime currentWriteTime = File.GetLastWriteTime(_todoJsonPath);

                // Eğer bizim bildiğimiz tarihten daha yeniyse -> YÜKLE
                if (currentWriteTime > _lastTodoFileUpdate)
                {
                    LoadTodoSummary();
                    _lastTodoFileUpdate = currentWriteTime; // Tarihi güncelle
                }
            }
            else
            {
                TodoSummary = "Görev Yok";
            }

            // 2. TAKVİM DOSYASI KONTROLÜ
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


        private async void GetWeatherAsync()
        {
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={City}&appid={ApiKey}&units={Unit}&lang=tr";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // İsteği yap
                    var response = await client.GetAsync(url);

                    // Eğer sunucu 200 OK demezse (örn: 401 Unauthorized), hata fırlat
                    response.EnsureSuccessStatusCode();

                    // Veriyi oku
                    var jsonString = await response.Content.ReadAsStringAsync();

                    // JSON Ayarları (Harf duyarlılığını kapat)
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
            catch (HttpRequestException httpEx)
            {
                // İnternet veya API Key hatası varsa buraya düşer
                System.Diagnostics.Debug.WriteLine($"API Hatası: {httpEx.Message}");

                WeatherTemp = "-";
                WeatherDesc = "Bağlantı Hatası"; 
                WeatherCity = "Hata";
            }
            catch (Exception ex)
            {
                // JSON çevirme hatası vs.
                System.Diagnostics.Debug.WriteLine($"Genel Hata: {ex.Message}");

                WeatherTemp = "-";
                WeatherDesc = "Veri Yok";
                WeatherCity = "Hata";
            }
        }

        // --- PROPERTYLER ---
        public string GreetingText
        {
            get => _greetingText;
            set { _greetingText = value; OnPropertyChanged(); }
        }

        public string CurrentDate
        {
            get => _currentDate;
            set { _currentDate = value; OnPropertyChanged(); }
        }

        public string CurrentTime
        {
            get => _currentTime;
            set { _currentTime = value; OnPropertyChanged(); }
        }

        public string TodoSummary
        {
            get => _todoSummary;
            set { _todoSummary = value; OnPropertyChanged(); }
        }

        public string CalendarSummary
        {
            get => _calendarSummary;
            set { _calendarSummary = value; OnPropertyChanged(); }
        }

        public string CalendarTimeSummary
        {
            get => _calendarTimeSummary;
            set { _calendarTimeSummary = value; OnPropertyChanged(); }
        }

        public string WeatherTemp
        {
            get => _weatherTemp;
            set { _weatherTemp = value; OnPropertyChanged(); }
        }

        public string WeatherDesc
        {
            get => _weatherDesc;
            set { _weatherDesc = value; OnPropertyChanged(); }
        }

        public string WeatherIconUrl
        {
            get => _weatherIconUrl;
            set { _weatherIconUrl = value; OnPropertyChanged(); }
        }

        public string WeatherCity
        {
            get => _weatherCity;
            set { _weatherCity = value; OnPropertyChanged(); }
        }

        public string GetWeatherApiKey()
        {
            // Dosya yolu
            string configFile = "appsettings.json";

            if (File.Exists(configFile))
            {
                // Dosyayı oku
                string jsonString = File.ReadAllText(configFile);

                // JSON'u parse et (Basitçe JsonDocument kullanarak)
                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    return doc.RootElement
                              .GetProperty("OpenWeatherMap")
                              .GetProperty("ApiKey")
                              .GetString();
                }
            }

            return string.Empty; // Dosya yoksa boş dön
        }
    }
}
