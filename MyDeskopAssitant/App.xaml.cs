using MyDeskopAssitant.Core;
using MyDeskopAssitant.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

namespace MyDeskopAssitant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MusicViewModel MusicVM { get; } = new MusicViewModel();

        protected override void OnStartup(StartupEventArgs e)
        {
            
            ThemeManager.LoadAndApplyTheme();

            base.OnStartup(e);
        }
    }

}
