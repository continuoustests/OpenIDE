using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CoreExtensions;
using OpenIDE.Core.Logging;

namespace OpenIDE.Core.Windowing
{
	public class BringToForeGround
	{
		public static void ByHWnd(IntPtr hwnd)
		{
			setForegroundByHwnd(hwnd);
		}

		public static void ByProcess(int pid)
		{
			setForegroundByPid(pid);
		}

		public static void ByProcAndName(string process, string title)
		{
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) {
                BringToForeGroundLinux.BringToFront(title);
                return;
            }
			setForegroundByHwnd(findWindow(process, title));
		}

		public static void ByProc(string process)
		{
			setForegroundByHwnd(findWindow(process, null));
		}

		private static IntPtr findWindow(string process, string title)
		{
			var proc = Process.GetProcessesByName(process)
				.FirstOrDefault(x => x.MainWindowHandle != new IntPtr(0) && (title == null || x.MainWindowTitle.Contains(title)));
			if (proc != null)
				return proc.MainWindowHandle;
			return new IntPtr(0);
		}

		private static void setForegroundByPid(int pid)
		{
			var proc = Process.GetProcessById(pid);
			if (proc != null) {
                if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                    BringToForeGroundLinux.BringToFront(pid);
                else
				    setForegroundByHwnd(proc.MainWindowHandle);
            }
		}

		private static void setForegroundByHwnd(IntPtr hWnd)
		{
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                BringToForeGroundLinux.BringToFront(hWnd);
            else
                BringToForeGroundWindows.BringToFront(hWnd);
		}
	}

    class BringToForeGroundWindows
    {
        [DllImport("user32.dll")] private static extern 
            bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] private static extern 
            bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern 
            bool IsIconic(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern 
            bool IsZoomed(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern 
            IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern 
            IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);
        [DllImport("user32.dll")] private static extern 
            IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, int fAttach);

        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_RESTORE = 9;
        private const int SW_SHOWDEFAULT = 10;

        public static void BringToFront(IntPtr hWnd)
        {
            if (hWnd == new IntPtr(0))
                return;
            var ourThread = GetWindowThreadProcessId(Process.GetCurrentProcess().MainWindowHandle, IntPtr.Zero);
            var foregroundThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            var threadToFocus = GetWindowThreadProcessId(hWnd, IntPtr.Zero);
            
            if (foregroundThread != threadToFocus) 
                AttachThreadInput(foregroundThread,threadToFocus,1);
            if (ourThread != threadToFocus) 
                AttachThreadInput(ourThread,threadToFocus,1);

            SetForegroundWindow(ourThread);
            SetForegroundWindow(hWnd);

            if (foregroundThread != threadToFocus) 
                AttachThreadInput(foregroundThread,threadToFocus,0);
            if (ourThread != threadToFocus) 
                AttachThreadInput(ourThread,threadToFocus,0);

            SetForegroundWindow(hWnd);
            
            if (IsIconic(hWnd))
                ShowWindowAsync(hWnd,SW_RESTORE);
        }
    }

    class BringToForeGroundLinux
    {
        public static void BringToFront(IntPtr hWnd)
        {
            Logger.Write("Looking for window with handle " + hWnd);
            var proc = Process.GetProcesses()
                .FirstOrDefault(x => x.MainWindowHandle == hWnd);
            if (proc == null)
                return;
            BringToFront(proc.Id);
        }

        public static void BringToFront(int pid)
        {
            string windowId = null;
            new System.Diagnostics.Process().Query(
                "/usr/bin/wmctrl",
                "-lp",
                false,
                Environment.CurrentDirectory,
                (err, msg) => {
                    if (err) {
                        Logger.Write(msg);
                        return;
                    }
                    var chunks = msg.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                    if (chunks.Length >= 4 && chunks[2] == pid.ToString())
                        windowId = chunks[0];
                }
            );
            if (windowId == null)
                return;

            var args = "-i -a \"" + windowId + "\"";
            Logger.Write("Running wmctrl " + args);
            new System.Diagnostics.Process().Query(
                "/usr/bin/wmctrl",
                args,
                false,
                Environment.CurrentDirectory,
                (err, msg) => Logger.Write(msg));
        }

        public static void BringToFront(string windowTitle)
        {
            var args = "-a \"" + windowTitle + "\"";
            Logger.Write("Running wmctrl " + args);

            new System.Diagnostics.Process().Query(
                "/usr/bin/wmctrl",
                args,
                false,
                Environment.CurrentDirectory,
                (err, msg) => Logger.Write(msg));
        }
    }
}
