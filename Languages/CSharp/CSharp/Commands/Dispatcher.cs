using System;
using System.Linq;
using System.Collections.Generic;

namespace CSharp.Commands
{
	public class Dispatcher
	{
		private List<ICommandHandler> _handlers = new List<ICommandHandler>();

		public void Register(ICommandHandler handler)
		{
			_handlers.Add(handler);
		}

		public IEnumerable<ICommandHandler> GetHandlers()
		{
			return _handlers;
		}

		public ICommandHandler GetHandler(string command)
		{
			return _handlers.FirstOrDefault(x => x.Command.Equals(command));
		}
	}
}
