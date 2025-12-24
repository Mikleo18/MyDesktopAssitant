using MyDeskopAssitant.Core;
using MyDeskopAssitant.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;

namespace MyDeskopAssitant.ViewModels
{
    public class CalenderViewModel : BaseViewModel
    {
        private ObservableCollection<CalendarEventModel> _allEvents;
        private ObservableCollection<CalendarEventModel> _displayEvents;
        private DateTime _selectedDate;
        private string _eventInput;
        private DateTime _selectedEventTime = DateTime.Now;

        private readonly string _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "calendar_data.json");

        public CalenderViewModel()
        {
            _allEvents = new ObservableCollection<CalendarEventModel>();
            DisplayEvents = new ObservableCollection<CalendarEventModel>();

            // Hours ve Minutes listelerini sildik, HandyControl buna ihtiyaç duymaz.

            SelectedDate = DateTime.Now;

            AddEventCommand = new RelayCommand(ExecuteAddEvent, CanAddEvent);
            DeleteEventCommand = new RelayCommand(ExecuteDeleteEvent);

            LoadEvents();
            RefreshDisplayList();
        }

        public ObservableCollection<CalendarEventModel> AllEvents
        {
            get => _allEvents;
            set { _allEvents = value; OnPropertyChanged(); }
        }

        public DateTime SelectedEventTime
        {
            get => _selectedEventTime;
            set
            {
                _selectedEventTime = value;
                OnPropertyChanged();
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                RefreshDisplayList();
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
            DateTime finalDateTime = SelectedDate.Date + SelectedEventTime.TimeOfDay;

            var newEvent = new CalendarEventModel
            {
                Title = EventInput,
                Date = finalDateTime
            };

            AllEvents.Add(newEvent);
            EventInput = "";
            RefreshDisplayList();

            OnPropertyChanged(nameof(AllEvents));
            SaveEvents();
        }

        private bool CanAddEvent(object obj) => !string.IsNullOrWhiteSpace(EventInput);

        private void RefreshDisplayList()
        {
            var filtered = _allEvents.Where(x => x.Date.Date == SelectedDate.Date).ToList();
            DisplayEvents = new ObservableCollection<CalendarEventModel>(filtered);
        }

        private void ExecuteDeleteEvent(object obj)
        {
            if (obj is CalendarEventModel eventToDelete)
            {
                AllEvents.Remove(eventToDelete);
                RefreshDisplayList();
                OnPropertyChanged(nameof(AllEvents));
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
            catch { }
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
            catch { }
        }
    }
}