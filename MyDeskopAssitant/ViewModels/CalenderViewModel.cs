using MyDeskopAssitant.Core;
using MyDeskopAssitant.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

        public CalenderViewModel()
        {
            _allEvents = new ObservableCollection<CalendarEventModel>();
            DisplayEvents = new ObservableCollection<CalendarEventModel>();

            // Başlangıçta bugünü seç
            SelectedDate = DateTime.Now;

            AddEventCommand = new RelayCommand(ExecuteAddEvent, CanAddEvent);

            // Örnek Veriler (Test için)
            _allEvents.Add(new CalendarEventModel { Title = "Proje Teslimi", Date = DateTime.Now.AddDays(2) });
            _allEvents.Add(new CalendarEventModel { Title = "Eski Toplantı", Date = DateTime.Now.AddDays(-5) }); // Geçmiş

            RefreshDisplayList();
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

        private void ExecuteAddEvent(object obj)
        {
            var newEvent = new CalendarEventModel
            {
                Title = EventInput,
                Date = SelectedDate
            };

            _allEvents.Add(newEvent);
            EventInput = ""; // Kutuyu temizle
            RefreshDisplayList(); // Listeyi güncelle
        }

        private bool CanAddEvent(object obj) => !string.IsNullOrWhiteSpace(EventInput);

        // Seçilen tarihe göre listeyi filtrele
        private void RefreshDisplayList()
        {
            // Sadece seçili güne ait olanları getir
            var filtered = _allEvents.Where(x => x.Date.Date == SelectedDate.Date).ToList();

            DisplayEvents = new ObservableCollection<CalendarEventModel>(filtered);
        }

    }
}
