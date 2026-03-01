using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using tulo.CommonMVVM.Commands;

namespace tulo.ResourcesWpfLib.Commands
{
    public class SaveScreenshotAsPngCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            // Expect: [0] FrameworkElement, [1] bool highResolution, optional [2] string filename (unit test)
            if (parameter is not object[] args || args.Length < 2)
                return;

            if (args[0] is not FrameworkElement element)
                return;

            if (args[1] is not bool highResolution)
                return;

            var tempPath = Path.GetTempPath();

            // Optional filename override for unit tests (args.Length == 3)
            var fileName = (args.Length >= 3 && args[2] is string unitTestName && !string.IsNullOrWhiteSpace(unitTestName))
                ? unitTestName
                : $"{DateTime.Now:yyyy-MM-dd HHmmss}{(highResolution ? "HighResolution" : string.Empty)}";

            var filePath = Path.Combine(tempPath, $"{fileName}.png");

            // Decide DPI and pixel size
            const double normalDpi = 96.0;
            const double hiDpi = 300.0;

            var dpi = highResolution ? hiDpi : normalDpi;

            int pixelWidth = (int)Math.Round(element.RenderSize.Width * dpi / normalDpi);
            int pixelHeight = (int)Math.Round(element.RenderSize.Height * dpi / normalDpi);

            // Guard against invalid sizes (can happen if element isn't measured/arranged yet)
            if (pixelWidth <= 0 || pixelHeight <= 0)
                return;

            // Optional: EdgeMode only affects certain rendering scenarios; keep if you really need it.
            RenderOptions.SetEdgeMode(element, EdgeMode.Aliased);

            var rtb = new RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32);
            rtb.Render(element);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using var fileStream = File.Create(filePath);
            encoder.Save(fileStream);
        }

    }
}
