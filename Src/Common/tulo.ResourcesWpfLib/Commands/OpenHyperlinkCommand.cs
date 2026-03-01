using System;
using System.Diagnostics;
using tulo.CommonMVVM.Commands;

namespace tulo.ResourcesWpfLib.Commands;
public class OpenHyperlinkCommand : BaseCommand
{
    public override bool CanExecute(object parameter)
    {
        return parameter is string;
    }
    public override void Execute(object parameter)
    {
        string url = parameter as string;

        if (!string.IsNullOrWhiteSpace(url))
        {
            try
            {
                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Link konnte nicht geöffnet werden: {ex.Message}");
            }
        }
    }
}
