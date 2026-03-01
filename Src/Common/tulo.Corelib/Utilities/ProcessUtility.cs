using System.Diagnostics;
using System.Runtime.InteropServices;

namespace tulo.CoreLib.Utilities
{
    public static class ProcessUtility
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(nint handle);

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(nint handle, int nCmdShow);

        [DllImport("User32.dll")]
        private static extern bool IsIconic(nint handle);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern nint FindWindow(string className, string windowTitle);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

        public static bool BringProcessToFront(Process p, string windowTitle)
        {
            // note:
            // when dsg help is minimized, the main window title of process is empty and 
            // there is an another window handle which can be found by title

            // get window handle find by title
            var windowHandle = FindWindow(null!, windowTitle);
            if (windowHandle == default)
                return false;

            // find process id by window handle
            var result = GetWindowThreadProcessId(windowHandle, out uint processId);
            if (result == default)
                return false;

            // does the window belongs to the dsg help process?
            if (p.Id != processId)
                return false;

            // determines whether the specified window is minimized (iconic)
            if (IsIconic(windowHandle))
                ShowWindow(windowHandle, 9); // SW_RESTORE

            // bring window to front
            return SetForegroundWindow(windowHandle);
        }
    }
}
