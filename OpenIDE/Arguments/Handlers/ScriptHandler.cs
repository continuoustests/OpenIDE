using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{						
	class ScriptHandler : ICommandHandler
	{
		private List<Script> _scripts = new List<Script>();
		private string _token;
		private Action<string> _dispatch;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Executes script");
					_scripts
						.ForEach(x => 
							{
								var cmdUsages = x.Usages.ToList();
								var cmd = new BaseCommandHandlerParameter(x.Name, x.Description);
								cmdUsages.ForEach(y => cmd.Add(y));
								usage.Add(cmd);
							});
					return usage;
			}
		}

		public string Command { get { return "x"; } }

		public ScriptHandler(string token, Action<string> dispatch)
		{
			_token = token;
			_dispatch = dispatch;
			_scripts.AddRange(new ScriptLocator(_token, Environment.CurrentDirectory).GetLocalScripts());
			new ScriptLocator(_token, Environment.CurrentDirectory)
				.GetGlobalScripts()
				.Where(x => _scripts.Count(y => x.Name.Equals(y.Name)) == 0).ToList()
				.ForEach(x => _scripts.Add(x));
		}

		public void Execute(string[] arguments)
		{
			if (arguments.Length == 0) {
				printAvailableCommands();
				return;
			}
			var script = _scripts.FirstOrDefault(x => x.Name.Equals(arguments[0]));
			if (script == null || arguments.Length < 1) {
				printAvailableCommands();
				return;
			}

			var sb = new StringBuilder();
			for (int i = 1; i < arguments.Length; i++)
				sb.Append(" \"" + arguments[i] + "\"");

			script.Run(sb.ToString(), (line) => {
					_dispatch(line);
				});
		}

		private void printAvailableCommands()
		{
			Console.WriteLine("Available commands:");
			var level = 0;
			Usage.Parameters.ToList()
				.ForEach(y =>  printParameter(y, ref level));
			Console.WriteLine("");
		}
		
		private void printParameter(BaseCommandHandlerParameter parameter, ref int level)
		{
			level++;
			var name = parameter.Name;
			if (!parameter.Required)
				name = "[" + name + "]";
			Console.WriteLine("{0}{1} : {2}", "".PadLeft(level, '\t'), name, parameter.Description);
			foreach (var child in parameter.Parameters)
				printParameter(child, ref level);
			level--;
		}
	}
}
