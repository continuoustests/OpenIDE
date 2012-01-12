using System;
using OpenIDENet.Core.Language;
using OpenIDENet.Core.Windowing;

namespace OpenIDENet.Arguments.Handlers
{
	public class BringToForegroundHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Sets the chose window to foreground window (MS Windows)");
				usage.Add("HWND", "Window handle of the window you want set to the foreground");
				usage.Add(
					"PROCESS_NAME",
					"The name of the process that contains the window")
					.Add("WINDOW_TITLE", "Text that is contained in the window title you want shown");
				return usage;
			}
		}

		public string Command { get { return "set-to-foreground"; } }
		
		public void Execute (string[] arguments)
		{
			try {
				if (arguments.Length == 1)
					BringToForeGround.hWnd(new IntPtr(int.Parse(arguments[0])));
				if (arguments.Length == 2)
					BringToForeGround.ProcAndName(arguments[0], arguments[1]);
			} catch {
				Console.WriteLine("Invalid command arguments");
			}
		}
	}
}
