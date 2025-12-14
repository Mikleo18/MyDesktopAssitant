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

        public object CurrentView
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        public ICommand NavigateCommand { get; }

        public MainViewModel()
        {
            MusicVm = App.MusicVM;
            NavigateCommand = new RelayCommand(ExecuteNavigate);
            CurrentView = MusicVm; // Başlangıçta Music ViewModel'i yüklüyoruz

        }

        private void ExecuteNavigate(object parameter)
        {
            string viewName = parameter as string;

            // Komut parametresine göre hangi ViewModel'in aktif olacağına karar veriyoruz
            switch (viewName)
            {
                case "Home":
                    // CurrentView = new HomeViewModel(); // Eğer varsa
                    break;
                case "Music":
                    CurrentView = MusicVm; // Music ViewModel'i yüklüyoruz
                    break;
                case "Video":
                    // CurrentView = new VideoViewModel();
                    break;
                    // ... Diğer sayfalar ...
            }
        }

    }
}
