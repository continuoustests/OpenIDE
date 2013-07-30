using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenIDE.Core.Windowing
{
	public class BringToForeGround
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
		
		public static void ByHWnd(IntPtr hwnd)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix ||
				Environment.OSVersion.Platform == PlatformID.MacOSX)
				return;
			setForegroundByHwnd(hwnd);
		}

		public static void ByProcess(int pid)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix ||
				Environment.OSVersion.Platform == PlatformID.MacOSX)
				return;
			setForegroundByPid(pid);
		}

		public static void ByProcAndName(string process, string title)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix ||
				Environment.OSVersion.Platform == PlatformID.MacOSX)
				return;
			setForegroundByHwnd(findWindow(process, title));
		}

		public static void ByProc(string process)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix ||
				Environment.OSVersion.Platform == PlatformID.MacOSX)
				return;
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
			if (proc != null)
				setForegroundByHwnd(proc.MainWindowHandle);
		}

		private static void setForegroundByHwnd(IntPtr hWnd)
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
}
