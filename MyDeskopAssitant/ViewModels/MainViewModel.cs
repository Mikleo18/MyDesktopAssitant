using MyDeskopAssitant.Core;
using MyDeskopAssitant.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyDeskopAssitant.ViewModels
{
    public class MainViewModel:BaseViewModel
    {
        private object _currentViewModel;
        public MusicViewModel MusicVm { get; set; }
        public VideoViewModel VideoVm { get; set; }
        public CalenderViewModel CalenderVm { get; set; }
        public ToDoViewModel ToDoListVm { get; set; }
        public HomeViewModel HomeVm { get; set; }
        public SettingsViewModel SettingsVm { get; set; }

        public object CurrentView
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        public ICommand NavigateCommand { get; }

        public MainViewModel()
        {


            //MusicVm = App.MusicVM;
            MusicVm = new MusicViewModel();
            VideoVm = new VideoViewModel();
            CalenderVm = new CalenderViewModel();
            ToDoListVm = new ToDoViewModel();
            HomeVm = new HomeViewModel();
            SettingsVm = new SettingsViewModel();
            NavigateCommand = new RelayCommand(ExecuteNavigate);


            CurrentView = HomeVm; 

        }

        private void ExecuteNavigate(object parameter)
        {
            string page = parameter?.ToString();

            switch (page)
            {
                case "Home":
                    CurrentView = HomeVm;
                    break;
                case "Music":
                    CurrentView = MusicVm; 
                    break;
                case "Video":
                    CurrentView = VideoVm; 
                    break;
                case "Calendar":
                     CurrentView = CalenderVm;
                    break;
                case "ToDo":
                    CurrentView = ToDoListVm;
                    break;
                case "Settings":
                    CurrentView = SettingsVm;
                    break;
            }
        }

    }
}
