using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenIDE.Windows.BringToFront
{
	class Program
	{
		[DllImportAttribute("User32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		public static void Main(string[] args)
		{
			try {
				if (args[0] == "--hwnd")
					setForegroundByHwnd(new IntPtr(int.Parse(args[1])));
				if (args[0] == "--window-name")
					setForegroundByHwnd(findWindow(args[1], args[2]));
			} catch {
				Console.WriteLine("Usage");
				Console.WriteLine("\t--hwnd HWND");
				Console.WriteLine("\t--window-name PROC_NAME STRING_FOR_WINDOW_NAME_TO_CONTAIN");
			}
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
			Console.WriteLine("Set foreground " + hwnd.ToString());
			SetForegroundWindow(hwnd);
		}
	}
}
