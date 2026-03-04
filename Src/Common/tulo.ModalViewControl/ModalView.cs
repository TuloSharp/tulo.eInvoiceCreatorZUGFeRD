using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace tulo.ModalViewControl
{
    public class ModalView : ContentControl
    {
        public bool IsModalViewOpen
        {
            get { return (bool)GetValue(IsModalViewOpenProperty); }
            set { SetValue(IsModalViewOpenProperty, value); }
        }

        public static readonly DependencyProperty IsModalViewOpenProperty =
            DependencyProperty.Register("IsModalViewOpen", typeof(bool), typeof(ModalView), new PropertyMetadata(false));


        static ModalView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ModalView), new FrameworkPropertyMetadata(typeof(ModalView)));
            BackgroundProperty.OverrideMetadata(typeof (ModalView), new FrameworkPropertyMetadata(CreateDefaultBackground()));
        }

        private static object CreateDefaultBackground()
        {
            return new SolidColorBrush(Colors.LightGray)
            {
                Opacity = 0.5
            };
        }
    }
}
