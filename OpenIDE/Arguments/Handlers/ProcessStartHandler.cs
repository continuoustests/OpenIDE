using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using OpenIDE.Core.Language;
using OpenIDE.Core.Windowing;
using CoreExtensions;

namespace OpenIDE.Arguments.Handlers
{
	class ProcessStartHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Process handling");
				usage
					.Add("start", "Starts a process")
						.Add("FILE", "Path to the file you want started")
							.Add("Arguments", "Command line arguments")
								.Add("[--process-hidden]", "Starts the process in the background");

				var foreground = usage
					.Add("set-to-foreground", "Sets the chosen window to foreground window (MS Windows)");
				var hwnd = foreground.Add("window", "Bring a spesific window to foreground");
				hwnd.Add("HWND", "Window handle of the window you want set to the foreground");
				hwnd.Add(
					"PROCESS_NAME",
					"The name of the process that contains the window")
					.Add("[WINDOW_TITLE]", "Text that is contained in the window title you want shown");
				var process = foreground.Add("process", "Bring a spesific process's main window to foreground");
				process.Add("PID", "Process id");

				return usage;
			}
		}
	
		public string Command { get { return "process"; } }
		
		public void Execute(string[] arguments)
		{
			if (arguments.Length < 1)
				return;
			if (arguments[0] == "start")
				start(arguments.Skip(1).ToArray());
			if (arguments[0] == "set-to-foreground")
				setToForeground(arguments.Skip(1).ToArray());
		}

		private void start(string[] arguments)
		{
			var visible = true;
			var sb = new StringBuilder();
			for (int i = 1; i < arguments.Length; i++)
			{
				if (arguments[i] == "--process-hidden")
				{
					visible = false;
					continue;
				}
				sb.Append("\"" + arguments[i] + "\" ");
			}
			var proc = new Process();
			proc.Run(
				arguments[0],
				sb.ToString().Trim(),
				visible,
				Environment.CurrentDirectory);
		}

		private void setToForeground(string[] arguments)
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
