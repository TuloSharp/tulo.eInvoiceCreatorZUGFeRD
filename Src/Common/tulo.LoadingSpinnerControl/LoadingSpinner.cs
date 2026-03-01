using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace tulo.LoadingSpinnerControl
{
    public class LoadingSpinner : Control
    {
        #region IsLoading
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool),
                typeof(LoadingSpinner),
                new PropertyMetadata(false, OnIsLoadingChanged));

        private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var spinner = (LoadingSpinner)d;

            if ((bool)e.NewValue)
                _ = spinner.AnimateTextAsync(25);
        }
        #endregion



        #region FullText und DisplayedText (animated)
        public string FullText
        {
            get => (string)GetValue(FullTextProperty);
            set => SetValue(FullTextProperty, value);
        }

        public static readonly DependencyProperty FullTextProperty =
            DependencyProperty.Register("FullText", typeof(string),
                typeof(LoadingSpinner),
                new PropertyMetadata("Loading..."));

        public string DisplayedText
        {
            get => (string)GetValue(DisplayedTextProperty);
            set => SetValue(DisplayedTextProperty, value);
        }

        public static readonly DependencyProperty DisplayedTextProperty =
            DependencyProperty.Register("DisplayedText", typeof(string),
                typeof(LoadingSpinner),
                new PropertyMetadata(""));
        #endregion

        #region Spinner visuals
        public double Diameter
        {
            get { return (double)GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }

        public static readonly DependencyProperty DiameterProperty =
            DependencyProperty.Register("Diameter", typeof(double),
                typeof(LoadingSpinner),
                new PropertyMetadata(100.0));

        public double Thickness
        {
            get { return (double)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof(double),
                typeof(LoadingSpinner),
                new PropertyMetadata(1.0));

        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush),
                typeof(LoadingSpinner),
                new PropertyMetadata(Brushes.OrangeRed));
        #endregion

        static LoadingSpinner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingSpinner), new FrameworkPropertyMetadata(typeof(LoadingSpinner)));
        }

        #region Text animation
        private async Task AnimateTextAsync(int delay)
        {
            while (IsLoading)
            {
                DisplayedText = string.Empty;

                foreach (char c in FullText)
                {
                    DisplayedText += c;
                    await Task.Delay(delay);
                }

                await Task.Delay(250);
            }

            DisplayedText = string.Empty;
        }
        #endregion
    }
}
