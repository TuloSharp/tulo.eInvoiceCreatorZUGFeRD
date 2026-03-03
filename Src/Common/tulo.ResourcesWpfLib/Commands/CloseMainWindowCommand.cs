using System;
using System.Diagnostics;
using System.Windows;
using tulo.CommonMVVM.Commands;

namespace tulo.ResourcesWpfLib.Commands;

public class CloseMainWindowCommand : BaseCommand
{
    public override void Execute(object parameter)
    {
        if (parameter == null)
        {
            return;
        }

        try
        {
            object[] obj = (object[])parameter;
            Window window = (Window)obj[0];
            bool isMainWindow = false;
            bool isModalOpen = false;

            if (obj.Length > 1 && obj[1] is bool mainWindowFlag)
                isMainWindow = mainWindowFlag;

            if (obj.Length > 2 && obj[2] is bool modalFlag)
                isModalOpen = modalFlag;

            if (window != null && isMainWindow && !isModalOpen)
            {
                window.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"error at execution: {ex.Message}");
        }
    }
}
