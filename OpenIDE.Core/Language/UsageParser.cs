using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.Logging;

namespace OpenIDE.Core.Language
{
	public class UsageParser
	{
		private string _usage;

		public UsageParser(string usage)
		{
			_usage = usage
				.Replace(Environment.NewLine, " ")
				.Replace("\t", "");
		}

		public BaseCommandHandlerParameter[] Parse()
		{
			var commands = new List<BaseCommandHandlerParameter>();
			int index = 0;
			while (true)
			{
				var command = parse(ref index);
				if (command == null)
					break;
				commands.Add(command);
			}
			return commands.ToArray();
		}

		private BaseCommandHandlerParameter parse(ref int index)
		{
			bool isTerminated;
			var command = getCommand(ref index);
			if (command == null)
				return null;
			var description = getDescription(ref index, out isTerminated);
			var cmd = new BaseCommandHandlerParameter(command, description);
			if (!isTerminated)
				getSubCommands(ref index).ForEach(x => cmd.Add(x));
			return cmd;
		}

		private List<BaseCommandHandlerParameter> getSubCommands(ref int index)
		{
			var commands = new List<BaseCommandHandlerParameter>();
			while (true)
			{
				var cmd = parse(ref index);
				if (cmd == null)
					throw new Exception(string.Format("Invalid usage. Could not get sub command at {0} in {1}", index, _usage));
				commands.Add(cmd);
				if (nextIsTermination(ref index))
					break;
			}
			return commands;
		}

		private string getCommand(ref int index)
		{
			var end = _usage.IndexOf("|\"", index);
			if (end == -1)
				return null;
			var command = _usage.Substring(index, end - index).Trim();
			index = end + "|\"".Length;
			return command;
		}

		private string getDescription(ref int index, out bool isTerminated)
		{
			var end = _usage.IndexOf("\"", index);
			if (end == -1)
				throw new Exception(string.Format("Invalid usage statement. Error parsing description at offset {0} in {1}", index, _usage));
			var command = _usage.Substring(index, end - index);
			index = end + "\"".Length;
			isTerminated = nextIsTermination(ref index);
			return command.Trim();
		}

		private bool nextIsTermination(ref int index)
		{
			var isTerminated = nextIsTermination(index);
			if (isTerminated)
				index = _usage.IndexOf(" end", index) + " end".Length;
			return isTerminated;
		}

		private bool nextIsTermination(int index)
		{
			var nextCommand = _usage.IndexOf("|\"", index);
			if (nextCommand == -1)
				return true;
			if (_usage.IndexOf(" end", index) < nextCommand)
				return true;
			return false;
		}
	}
}
