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
		
		public void For(string name, string[] arguments, Action<string> onNoCommandHandled)
		{
			_eventDispatcher.Forward(name, arguments);
			var command = _handlers.FirstOrDefault(x => x.Command.Equals(name));
			if (command == null)
			{
				if (_pluginHandlers == null)
					_pluginHandlers = _pluginHandlerFactory().ToArray();
				command = _pluginHandlers.FirstOrDefault(x => x.Command.Equals(name));
				if (command == null) {
					var x = _handlers.First(y => y.Command == "x");
					if (x != null && x.Usage != null) {
						if (x.Usage.Parameters.Any(y => y.Name.Equals(name))) {
							command = x;
							var modifiedArguments = new List<string>();
							modifiedArguments.Add(name);
							modifiedArguments.AddRange(arguments);
							arguments = modifiedArguments.ToArray();
						}
					}
				}
				if (command == null) {
					Console.WriteLine(name + " is not a valid OpenIDE command.");
					onNoCommandHandled(name);
					return;
				}
			}
			command.Execute(arguments);
		}
	}
}

