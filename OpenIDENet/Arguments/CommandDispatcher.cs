using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace OpenIDENet.Arguments
{
	class CommandDispatcher : OpenIDENet.Arguments.ICommandDispatcher
	{
		private ICommandHandler[] _handlers;
		private ICommandHandler[] _pluginHandlers;
		private Func<IEnumerable<ICommandHandler>> _pluginHandlerFactory;
		
		public CommandDispatcher(ICommandHandler[] handlers, Func<IEnumerable<ICommandHandler>> pluginHandlerFactory)
		{
			_handlers = handlers;
			_pluginHandlerFactory = pluginHandlerFactory;
		}
		
		public void For(string name, string[] arguments)
		{
			var command = _handlers.FirstOrDefault(x => x.Command.Equals(name));
			if (command == null)
			{
				if (_pluginHandlers == null)
					_pluginHandlers = _pluginHandlerFactory().ToArray();
				command = _pluginHandlers.FirstOrDefault(x => x.Command.Equals(name));
				if (command == null)
				{
					Console.WriteLine("Invalid arguments. Unsupported command {0}", name);
					return;
				}
			}
			command.Execute(arguments);
		}
	}
}

