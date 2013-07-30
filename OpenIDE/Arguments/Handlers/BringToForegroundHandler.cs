using System;
using OpenIDE.Core.Language;
using OpenIDE.Core.Windowing;

namespace OpenIDE.Arguments.Handlers
{
	public class BringToForegroundHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Sets the chosen window to foreground window (MS Windows)");
				var hwnd = usage.Add("window", "Bring a spesific window to foreground");
				hwnd.Add("HWND", "Window handle of the window you want set to the foreground");
				hwnd.Add(
					"PROCESS_NAME",
					"The name of the process that contains the window")
					.Add("[WINDOW_TITLE]", "Text that is contained in the window title you want shown");
				var process = usage.Add("process", "Bring a spesific process's main window to foreground");
				process.Add("PID", "Process id");

				return usage;
			}
		}

		public string Command { get { return "set-to-foreground"; } }
		
		public void Execute (string[] arguments)
		{
			try {
				if (arguments[0] == "window") {
					int hwnd;
					if (int.TryParse(arguments[1], out hwnd)) {
						BringToForeGround.ByHWnd(new IntPtr(int.Parse(arguments[1])));
					} else if (arguments.Length == 2) {
						BringToForeGround.ByProc(arguments[1]);
					} else if (arguments.Length == 3) {
						BringToForeGround.ByProcAndName(arguments[1], arguments[2]);
					}
				} else if (arguments[0] == "process") {
					BringToForeGround.ByProcess(int.Parse(arguments[1]));
				}
			} catch {
				Console.WriteLine("Invalid command arguments");
			}
		}
	}
}
