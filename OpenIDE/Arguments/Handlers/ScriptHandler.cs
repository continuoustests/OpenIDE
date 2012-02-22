using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using OpenIDE.FileSystem;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{						
	class ScriptHandler : ICommandHandler
	{
		private List<Script> _scripts = new List<Script>();
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

		public ScriptHandler(Action<string> dispatch)
		{
			_dispatch = dispatch;
			_scripts.AddRange(new ScriptLocator().GetLocalScripts());
			new ScriptLocator()
				.GetGlobalScripts()
				.Where(x => _scripts.Count(y => x.Name.Equals(y.Name)) == 0).ToList()
				.ForEach(x => _scripts.Add(x));
		}

		public void Execute(string[] arguments)
		{
			var script = _scripts.FirstOrDefault(x => x.Name.Equals(arguments[0]));
			if (script == null || arguments.Length < 1)
				return;

			var sb = new StringBuilder();
			for (int i = 1; i < arguments.Length; i++)
				sb.Append(" \"" + arguments[i] + "\"");

			foreach (var line in script.Run(sb.ToString()))
				_dispatch(line);
		}
	}
}
