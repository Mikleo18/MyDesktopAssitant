using MyDeskopAssitant.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyDeskopAssitant.Views
{

    public partial class Music : UserControl
    {
        private MusicViewModel _viewModel;

        public Music()
        {
            InitializeComponent();
            this.DataContextChanged += Music_DataContextChanged;
        }

        private void Music_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is MusicViewModel vm)
            {
                _viewModel = vm;
                // BURADA ARTIK HİÇBİR OLAYI (EVENT) DİNLEMİYORUZ.
                // ÇÜNKÜ O İŞİ ARTIK MAINWINDOW YAPIYOR.
            }
        }


        // Slider ile sarma işlemi
        private void slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Slider slider && this.DataContext is MusicViewModel vm)
            {
                // Doğrudan ViewModel'deki yeni komutu çalıştırıyoruz
                if (vm.SeekCommand.CanExecute(slider.Value))
                {
                    vm.SeekCommand.Execute(slider.Value);
                }
            }
        }
    }
}
