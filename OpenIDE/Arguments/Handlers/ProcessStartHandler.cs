using System;
using System.Text;
using System.Diagnostics;
using OpenIDE.Core.Language;
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
						"Starts a process");
					usage.Add("FILE", "Path to the file you want started")
						.Add("Arguments", "Command line arguments")
							.Add("[--process-hidden]", "Starts the process in the background");
					return usage;
			}
		}
	
		public string Command { get { return "process-start"; } }
		
		public void Execute(string[] arguments)
		{
			if (arguments.Length < 1)
				return;
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
	}
}
