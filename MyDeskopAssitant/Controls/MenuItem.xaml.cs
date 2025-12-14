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

namespace MyDeskopAssitant.Controls
{
    /// <summary>
    /// Interaction logic for MenuItem.xaml
    /// </summary>
    public partial class MenuItem : UserControl
    {
        public MenuItem()
        {
            InitializeComponent();
        }

        public PathGeometry Icon
        {
            get { return (PathGeometry)GetValue(IconDataProperty); }
            set { SetValue(IconDataProperty, value); }
        }

        public static readonly DependencyProperty IconDataProperty =
            DependencyProperty.Register("Icon", typeof(PathGeometry), typeof(MenuItem), new PropertyMetadata(null));

        public int IconWidth
        {
            get { return (int)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }

        public static readonly DependencyProperty IconWidthProperty =
           DependencyProperty.Register("IconWidth", typeof(int), typeof(MenuItem));

        public SolidColorBrush IconBrush
        {
            get { return (SolidColorBrush)GetValue(IndicatorBrushProperty);}
            set { SetValue(IndicatorBrushProperty,value); }
        }

        public static readonly DependencyProperty IndicatorBrushProperty=
            DependencyProperty.Register("IndicatorBrushProperty",typeof(SolidColorBrush),typeof(MenuItem),new PropertyMetadata(null));

        public int IndicatorIndicatorCornerRadius
        {
            get { return (int)GetValue(IndicatorIndicatorCornerRadiusProperty); }
            set { SetValue(IndicatorIndicatorCornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty IndicatorIndicatorCornerRadiusProperty=
            DependencyProperty.Register("IndicatorIndicatorCornerRadius",typeof(int),typeof(MenuItem),new PropertyMetadata(null));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty,value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("TextProperty",typeof(string),typeof(MenuItem),new PropertyMetadata(null));

        public new Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        public static new readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register("Padding", typeof(Thickness), typeof(MenuItem));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

       
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(MenuItem));



        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        
        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(string), typeof(MenuItem));

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }
    }

}
