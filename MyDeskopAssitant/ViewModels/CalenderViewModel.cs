using MyDeskopAssitant.Core;
using MyDeskopAssitant.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyDeskopAssitant.ViewModels
{
    public class CalenderViewModel:BaseViewModel
    {
        private ObservableCollection<CalendarEventModel> _allEvents;
        private ObservableCollection<CalendarEventModel> _displayEvents;
        private DateTime _selectedDate;
        private string _eventInput;

        private string _selectedHour;
        private string _selectedMinute;

        private readonly string _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "calendar_data.json");

        public CalenderViewModel()
        {
            _allEvents = new ObservableCollection<CalendarEventModel>();
            DisplayEvents = new ObservableCollection<CalendarEventModel>();

            Hours = new ObservableCollection<string>(Enumerable.Range(0, 24).Select(i => i.ToString("D2")));
            Minutes = new ObservableCollection<string>(Enumerable.Range(0, 12).Select(i => (i * 5).ToString("D2")));

            SelectedHour = "12";
            SelectedMinute = "00";

            SelectedDate = DateTime.Now;

            AddEventCommand = new RelayCommand(ExecuteAddEvent, CanAddEvent);
            DeleteEventCommand = new RelayCommand(ExecuteDeleteEvent);


            RefreshDisplayList();
            LoadEvents();
        }

        public ObservableCollection<CalendarEventModel> AllEvents // Converter için Public yaptık
        {
            get => _allEvents;
            set { _allEvents = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Hours { get; }
        public ObservableCollection<string> Minutes { get; }

        public string SelectedHour
        {
            get => _selectedHour;
            set { _selectedHour = value; OnPropertyChanged(); }
        }

        public string SelectedMinute
        {
            get => _selectedMinute;
            set { _selectedMinute = value; OnPropertyChanged(); }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                RefreshDisplayList(); // Tarih değişince listeyi güncelle
            }
        }

        public ObservableCollection<CalendarEventModel> DisplayEvents
        {
            get => _displayEvents;
            set { _displayEvents = value; OnPropertyChanged(); }
        }

        public string EventInput
        {
            get => _eventInput;
            set { _eventInput = value; OnPropertyChanged(); }
        }

        public ICommand AddEventCommand { get; }
        public ICommand DeleteEventCommand { get; }

        private void ExecuteAddEvent(object obj)
        {
            DateTime finalDateTime = SelectedDate.Date
                .AddHours(int.Parse(SelectedHour))
                .AddMinutes(int.Parse(SelectedMinute));

            var newEvent = new CalendarEventModel
            {
                Title = EventInput,
                Date = finalDateTime
            };

            AllEvents.Add(newEvent);
            EventInput = "";
            RefreshDisplayList();

            OnPropertyChanged(nameof(AllEvents));

            SaveEvents(); // Kaydet
        }

        private bool CanAddEvent(object obj) => !string.IsNullOrWhiteSpace(EventInput);

        // Seçilen tarihe göre listeyi filtrele
        private void RefreshDisplayList()
        {
            // Sadece seçili güne ait olanları getir
            var filtered = _allEvents.Where(x => x.Date.Date == SelectedDate.Date).ToList();

            DisplayEvents = new ObservableCollection<CalendarEventModel>(filtered);
        }

        private void ExecuteDeleteEvent(object obj)
        {
            // Parametre olarak silinecek olay (CalendarEventModel) gelir
            if (obj is CalendarEventModel eventToDelete)
            {
                // Ana listeden sil
                AllEvents.Remove(eventToDelete);

                // Ekranı güncelle
                RefreshDisplayList();

                OnPropertyChanged(nameof(AllEvents));

                // Değişikliği JSON'a kaydet
                SaveEvents();
            }
        }

        private void SaveEvents()
        {
            try
            {
                var json = JsonSerializer.Serialize(AllEvents);
                File.WriteAllText(_jsonPath, json);
            }
            catch { /* Hata yönetimi */ }
        }

        private void LoadEvents()
        {
            if (!File.Exists(_jsonPath)) return;
            try
            {
                var json = File.ReadAllText(_jsonPath);
                var loaded = JsonSerializer.Deserialize<ObservableCollection<CalendarEventModel>>(json);
                if (loaded != null)
                {
                    AllEvents = loaded;
                    RefreshDisplayList();
                }
            }
            catch { /* Hata yönetimi */ }
        }

    }
}
