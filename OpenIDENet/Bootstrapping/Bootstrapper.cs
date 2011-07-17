using System;
using OpenIDENet.Arguments;
using OpenIDENet.Versioning;
using OpenIDENet.Projects;
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
			return new CommandDispatcher(_container.ResolveAll<ICommandHandler>(), _container.Resolve<ILocateClosestProject>(), _container.Resolve<IResolveProjectVersion>());
		}
	}
}