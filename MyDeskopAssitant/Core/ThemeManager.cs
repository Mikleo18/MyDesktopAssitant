using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;


namespace MyDeskopAssitant.Core
{

    public class AppSettings
    {
        public bool IsDarkMode { get; set; } = true;
        public string PrimaryColorHex { get; set; } = "#FF007ACC"; // Varsayılan Mavi
    }

    public class ThemeManager
    {
        private static readonly string _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        public static AppSettings CurrentSettings { get; private set; } = new AppSettings();

        public static void LoadAndApplyTheme()
        {
            // 1. Kayıtlı ayarı yükle
            if (File.Exists(_settingsPath))
            {
                try
                {
                    var json = File.ReadAllText(_settingsPath);
                    CurrentSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch { }
            }

            // 2. Temayı Uygula
            ApplyTheme(CurrentSettings.IsDarkMode);
            ApplyPrimaryColor(CurrentSettings.PrimaryColorHex);
        }
    

    public static void SaveSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(CurrentSettings);
                File.WriteAllText(_settingsPath, json);
            }
            catch  {  }
        }

        public static void ApplyTheme(bool isDark)
        {
            CurrentSettings.IsDarkMode = isDark;
            var dict = Application.Current.Resources;

            if (isDark)
            {
                // KOYU TEMA RENKLERİ
                dict["PrimaryBackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#181735"));
                dict["SecundaryBackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#201F40"));
                dict["PrimaryTextColor"] = new SolidColorBrush(Colors.White);
                dict["SecundaryIconColor"] = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                // AÇIK TEMA RENKLERİ
                dict["PrimaryBackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F2F5"));
                dict["SecundaryBackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                dict["PrimaryTextColor"] = new SolidColorBrush(Colors.Black);
                dict["SecundaryIconColor"] = new SolidColorBrush(Colors.DarkGray);
            }

            SaveSettings();
        }

        public static void ApplyPrimaryColor(string hex)
        {
            try
            {
                // Hex kodunu (örn: #FF5555) fırçaya çevir
                var color = (Color)ColorConverter.ConvertFromString(hex);
                var brush = new SolidColorBrush(color);

                // Uygulamanın ana rengini güncelle
                Application.Current.Resources["PrimaryBlueColor"] = brush;

                CurrentSettings.PrimaryColorHex = hex;
                SaveSettings();
            }
            catch
            {
                
            }
        } 
    }

}
