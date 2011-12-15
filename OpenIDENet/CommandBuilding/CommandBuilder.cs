using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDENet.Arguments;

namespace OpenIDENet.CommandBuilding
{
	public class CommandBuilder
	{
		private List<CommandHandlerParameter> _parameters;

		public CommandHandlerParameter Current { get; private set; }
		public string Path { get; private set; }
		public CommandHandlerParameter[] AvailableCommands {
			get { 
				return commandsFromCurrernt().ToArray(); 
			} 
		}

		public CommandBuilder(IEnumerable<CommandHandlerParameter> parameters)
		{
			_parameters = parameters.ToList();
		}

		public void Select(string command)
		{
			var cmd = commandsFromCurrernt()
				.FirstOrDefault(x => x.Name.Equals(command));
			if (cmd == null)
				throw new InvalidCommandException(command, Path);
			Current = cmd;
			addToPath(cmd);
		}

		private IEnumerable<CommandHandlerParameter> commandsFromCurrernt()
		{
			if (Current == null)
				return _parameters;
			return null;
		}

		private void addToPath(CommandHandlerParameter command)
		{
		}
	}

	public class InvalidCommandException : Exception {
		public InvalidCommandException(string command, string currentPath) :
			base(string.Format("Command {0} is not available as an option for {1}", command, currentPath)) {}
	}
}
