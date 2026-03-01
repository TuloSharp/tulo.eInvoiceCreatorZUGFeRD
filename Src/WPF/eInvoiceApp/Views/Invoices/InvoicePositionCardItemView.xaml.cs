using System.Windows;
using System.Windows.Controls;

namespace tulo.eInvoice.eInvoiceApp.Views.Invoices;
/// <summary>
/// Interaction logic for InvoicePositionCardItemView.xaml
/// </summary>
public partial class InvoicePositionCardItemView : UserControl
{
    public InvoicePositionCardItemView()
    {
        InitializeComponent();
    }
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        dropdownmenu.IsOpen = false;
    }
}
