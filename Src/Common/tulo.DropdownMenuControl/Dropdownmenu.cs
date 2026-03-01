using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace tulo.DropdownMenuControl
{
    [TemplatePart(Name = PART_POPUP_NAME, Type = typeof(Popup))]
    [TemplatePart(Name = PART_TOGGLE_NAME, Type = typeof(CheckBox))]
    public class DropdownMenu : ContentControl
    {
        private const string PART_POPUP_NAME = "PART_Popup";
        private const string PART_TOGGLE_NAME = "PART_Toggle";

        private Popup _popup;
        private CheckBox _toggle;

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool),
                typeof(DropdownMenu),
                new PropertyMetadata(false));


        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        static DropdownMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropdownMenu), 
                new FrameworkPropertyMetadata(typeof(DropdownMenu)));
        }

        public override void OnApplyTemplate()
        {
            _popup = (Popup)Template.FindName(PART_POPUP_NAME, this);
            if (_popup != null)
            {
                _popup.Closed += PopupClosed;
            }

            _toggle = (CheckBox)Template.FindName(PART_TOGGLE_NAME, this);

            base.OnApplyTemplate();
        }

        private void PopupClosed(object sender, EventArgs e)
        {
            if (!_toggle.IsMouseOver)
            {
                IsOpen = false;
            }
        }
    }
}
