using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenIDE.Core.Windowing
{
	public class BringToForeGround
	{
		[DllImportAttribute("User32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);
		
		public static void hWnd(IntPtr hwnd)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix ||
				Environment.OSVersion.Platform == PlatformID.MacOSX)
				return;
			setForegroundByHwnd(hwnd);
		}

		public static void ProcAndName(string process, string title)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix ||
				Environment.OSVersion.Platform == PlatformID.MacOSX)
				return;
			setForegroundByHwnd(findWindow(process, title));
		}

		private static IntPtr findWindow(string process, string title)
		{
			var proc = Process.GetProcessesByName(process)
				.FirstOrDefault(x => x.MainWindowTitle.Contains(title));
			if (proc != null)
				return proc.MainWindowHandle;
			return new IntPtr(0);
		}

		private static void setForegroundByHwnd(IntPtr hwnd)
		{
			if (hwnd == new IntPtr(0))
				return;
			SetForegroundWindow(hwnd);
		}
	}
}
