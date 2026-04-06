using System.Windows;

namespace tulo.eInvoiceCreatorZUGFeRD;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(object dataContext)
    {
        InitializeComponent();
        DataContext = dataContext;

        MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
    }
}