using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using OpenIDE.EventIntegration;

namespace OpenIDE.Arguments
{
	class CommandDispatcher : OpenIDE.Arguments.ICommandDispatcher
	{
		private IEventDispatcher _eventDispatcher;
		private ICommandHandler[] _handlers;
		private ICommandHandler[] _pluginHandlers;
		private Func<IEnumerable<ICommandHandler>> _pluginHandlerFactory;
		
		public CommandDispatcher(
			ICommandHandler[] handlers,
			Func<IEnumerable<ICommandHandler>> pluginHandlerFactory,
			IEventDispatcher eventDispatcher)
		{
			_handlers = handlers;
			_pluginHandlerFactory = pluginHandlerFactory;
			_eventDispatcher = eventDispatcher;
		}
		
		public void For(string name, string[] arguments)
		{
			_eventDispatcher.Forward(name, arguments);
			var command = _handlers.FirstOrDefault(x => x.Command.Equals(name));
			if (command == null)
			{
				if (_pluginHandlers == null)
					_pluginHandlers = _pluginHandlerFactory().ToArray();
				command = _pluginHandlers.FirstOrDefault(x => x.Command.Equals(name));
				if (command == null)
					return;
			}
			command.Execute(arguments);
		}
	}
}

