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
    public class ToDoViewModel:BaseViewModel
    {
        private ObservableCollection<ToDoModel> _allTasks;      // Tüm görevler
        private ObservableCollection<ToDoModel> _displayTasks;  // Ekranda görünenler (Arama için)

        private string _newTaskContent;
        private string _searchText;
        private string _searchType = "All"; // Varsayılan arama tipi

        // Düzenleme (Popup) için
        private bool _isEditPopupOpen;
        private string _editContent;
        private ToDoModel _taskToEdit;

        private readonly string _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "todo_data.json");

        public ToDoViewModel()
        {
            _allTasks = new ObservableCollection<ToDoModel>();
            DisplayTasks = new ObservableCollection<ToDoModel>();

            AddCommand = new RelayCommand(ExecuteAdd, CanAdd);
            DeleteCommand = new RelayCommand(ExecuteDelete);
            EditCommand = new RelayCommand(ExecuteEdit);
            SaveEditCommand = new RelayCommand(ExecuteSaveEdit);
            CancelEditCommand = new RelayCommand(o => IsEditPopupOpen = false);

            // Görev tamamlandığında (Checkbox değişince) kaydetmek için basit bir komut
            CheckCommand = new RelayCommand(ExecuteCheck);

            LoadTasks();
        }

        public ObservableCollection<ToDoModel> DisplayTasks
        {
            get => _displayTasks;
            set { _displayTasks = value; OnPropertyChanged(); }
        }

        public string NewTaskContent
        {
            get => _newTaskContent;
            set { _newTaskContent = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterTasks(); // Her harf yazıldığında filtrele
            }
        }

        public string SearchType
        {
            get => _searchType;
            set
            {
                _searchType = value;
                OnPropertyChanged();
                FilterTasks();
            }
        }

        // --- POPUP PROPERTYLERİ ---
        public bool IsEditPopupOpen
        {
            get => _isEditPopupOpen;
            set { _isEditPopupOpen = value; OnPropertyChanged(); }
        }

        public string EditContent
        {
            get => _editContent;
            set { _editContent = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveEditCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand CheckCommand { get; }

        private void ExecuteAdd(object obj)
        {
            int newId = _allTasks.Count > 0 ? _allTasks.Max(t => t.Id) + 1 : 1;

            var task = new ToDoModel
            {
                Id = newId,
                Content = NewTaskContent,
                IsCompleted = false,
                TaskDate = DateTime.Now.ToString("dd.MM.yyyy"),
                TaskTime = DateTime.Now.ToString("HH:mm")
            };

            _allTasks.Add(task);
            NewTaskContent = ""; // Kutuyu temizle
            FilterTasks(); // Listeyi güncelle
            SaveTasks();
        }

        private bool CanAdd(object obj) => !string.IsNullOrWhiteSpace(NewTaskContent);

        private void ExecuteDelete(object obj)
        {
            if (obj is ToDoModel task)
            {
                _allTasks.Remove(task);
                FilterTasks();
                SaveTasks();
            }
        }

        private void ExecuteEdit(object obj)
        {
            if (obj is ToDoModel task)
            {
                _taskToEdit = task;
                EditContent = task.Content; // Mevcut yazıyı kutuya al
                IsEditPopupOpen = true;     // Popup'ı aç
            }
        }

        private void ExecuteSaveEdit(object obj)
        {
            if (_taskToEdit != null)
            {
                _taskToEdit.Content = EditContent;
                IsEditPopupOpen = false;
                FilterTasks(); // Listeyi yenile
                SaveTasks();
            }
        }

        private void ExecuteCheck(object obj)
        {
            // Checkbox işaretlenince sadece kaydet
            SaveTasks();
        }

        private void FilterTasks()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                DisplayTasks = new ObservableCollection<ToDoModel>(_allTasks);
                return;
            }

            var query = SearchText.ToLower();
            var filtered = _allTasks.Where(t =>
            {
                switch (SearchType)
                {
                    case "Id": return t.Id.ToString().Contains(query);
                    case "Date": return t.TaskDate.Contains(query);
                    default: return t.Content.ToLower().Contains(query); // Content ve All
                }
            }).ToList();

            DisplayTasks = new ObservableCollection<ToDoModel>(filtered);
        }

        // --- JSON KAYIT ---
        private void SaveTasks()
        {
            try { File.WriteAllText(_jsonPath, JsonSerializer.Serialize(_allTasks)); } catch { }
        }

        private void LoadTasks()
        {
            if (!File.Exists(_jsonPath)) return;
            try
            {
                var data = JsonSerializer.Deserialize<ObservableCollection<ToDoModel>>(File.ReadAllText(_jsonPath));
                if (data != null) { _allTasks = data; FilterTasks(); }
            }
            catch { }
        }
    }
}
