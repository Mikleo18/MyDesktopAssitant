using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MyDeskopAssitant.Controls
{
    public partial class MenuItem : UserControl
    {
        public MenuItem()
        {
            InitializeComponent();
        }

        // 1. COMMAND (Tıklama Komutu)
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(MenuItem));


        // 2. COMMAND PARAMETER (Hangi Sayfa?)
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(MenuItem));


        // 3. ICON (Vektörel Çizim)
        public PathGeometry Icon
        {
            get { return (PathGeometry)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(PathGeometry), typeof(MenuItem));


        // 4. TEXT (Menü Yazısı)
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MenuItem));


        // 5. ICON WIDTH (İkon Genişliği - Double olmalı)
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(MenuItem), new PropertyMetadata(20.0));


        // 6. ICON MARGIN (YENİ EKLENDİ - XAML'de kullanılıyor)
        public Thickness IconMargin
        {
            get { return (Thickness)GetValue(IconMarginProperty); }
            set { SetValue(IconMarginProperty, value); }
        }
        public static readonly DependencyProperty IconMarginProperty =
            DependencyProperty.Register("IconMargin", typeof(Thickness), typeof(MenuItem), new PropertyMetadata(new Thickness(0)));


        // 7. ICON BRUSH (İsimlendirme düzeltildi)
        public SolidColorBrush IconBrush
        {
            get { return (SolidColorBrush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty =
            DependencyProperty.Register("IconBrush", typeof(SolidColorBrush), typeof(MenuItem), new PropertyMetadata(null));


        // 8. GROUP NAME (Radyo Buton Gruplaması)
        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }
        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(string), typeof(MenuItem));


        // 9. IS SELECTED (Seçili mi?)
        // Bu özelliği RadioButton kendi içinde yönetir ama dışarıdan okumak istersen kalabilir.
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(MenuItem));

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

            IsSelected = true;
        }
    }
}