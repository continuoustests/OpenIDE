using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenIDENet.Arguments;
namespace OpenIDENet.Bootstrapping
{
	public static class Bootstrapper
	{
		private static DIContainer _container;
		
		public static void Initialize()
		{
			_container = new DIContainer();
			_container.Configure();
		}
		
		public static ICommandDispatcher GetDispatcher()
		{
			return new CommandDispatcher(
				_container.ResolveAll<ICommandHandler>());
		}

		public static IEnumerable<ICommandHandler> GetCommandHandlers()
		{
			return _container.ResolveAll<ICommandHandler>();
		}
	}
}
