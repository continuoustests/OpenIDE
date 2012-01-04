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
		}
		
		public static ICommandDispatcher GetDispatcher()
		{
			return _container.GetDispatcher();
		}

		public static IEnumerable<ICommandHandler> GetCommandHandlers()
		{
			return _container.ICommandHandlers();
		}
	}
}
