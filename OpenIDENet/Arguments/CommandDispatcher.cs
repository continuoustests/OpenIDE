using System;
using System.Linq;
using System.IO;

namespace OpenIDENet.Arguments
{
	class CommandDispatcher : OpenIDENet.Arguments.ICommandDispatcher
	{
		private ICommandHandler[] _handlers;
		
		public CommandDispatcher(ICommandHandler[] handlers)
		{
			_handlers = handlers;
		}
		
		public void For(string name, string[] arguments)
		{
			var command = _handlers.FirstOrDefault(x => x.Command.Equals(name));
			if (command == null)
			{
				Console.WriteLine("Invalid arguments. Unsupported command {0}", name);
				return;
			}
			command.Execute(arguments);
		}
	}
}

