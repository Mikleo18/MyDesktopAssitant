using MyDeskopAssitant.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyDeskopAssitant.ViewModels
{
    public class SettingsViewModel:BaseViewModel
    {
        private string _hexInput;
        private bool _isDarkMode;

        public SettingsViewModel()
        {
            // Mevcut ayarları ekrana getir
            HexInput = ThemeManager.CurrentSettings.PrimaryColorHex;
            IsDarkMode = ThemeManager.CurrentSettings.IsDarkMode;

            ApplyColorCommand = new RelayCommand(ExecuteApplyColor);
            ToggleThemeCommand = new RelayCommand(ExecuteToggleTheme);
        }

        public string HexInput
        {
            get => _hexInput;
            set { _hexInput = value; OnPropertyChanged(); }
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                _isDarkMode = value;
                OnPropertyChanged();
                // RadioButton değiştiği an temayı uygula
                ThemeManager.ApplyTheme(value);
            }
        }

        public ICommand ApplyColorCommand { get; }
        public ICommand ToggleThemeCommand { get; }

        private void ExecuteApplyColor(object obj)
        {
            if (!string.IsNullOrEmpty(HexInput))
            {
                // Başında # yoksa biz ekleyelim
                string finalHex = HexInput.StartsWith("#") ? HexInput : "#" + HexInput;
                ThemeManager.ApplyPrimaryColor(finalHex);
            }
        }

        private void ExecuteToggleTheme(object obj)
        {
            // Bu metot butonla tetiklenirse kullanılır (Opsiyonel)
        }

    }
}
