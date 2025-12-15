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
        private MusicViewModel MusicVm { get; set; }
        private VideoViewModel VideoVm { get; set; }
        private CalenderViewModel CalenderVm { get; set; }

        public object CurrentView
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        public ICommand NavigateCommand { get; }

        public MainViewModel()
        {
            MusicVm = App.MusicVM;
            VideoVm = new VideoViewModel();
            CalenderVm = new CalenderViewModel();
            NavigateCommand = new RelayCommand(ExecuteNavigate);
            CurrentView = VideoVm; 

        }

        private void ExecuteNavigate(object parameter)
        {
            string page = parameter?.ToString();

            switch (page)
            {
                case "Home":
                    //CurrentView = new HomeViewModel(); // Veya tanımlıysa HomeVm
                    break;
                case "Music":
                    CurrentView = MusicVm; // Zaten tanımlıydı
                    break;
                case "Video":
                    CurrentView = VideoVm; // Yeni tanımladığımız
                    break;
                case "Calendar":
                     CurrentView = CalenderVm;
                    break;
                    // ... diğerleri
            }
        }

    }
}
