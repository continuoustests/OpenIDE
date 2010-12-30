using System;
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
		
		public static T Resolve<T>()
		{
			return _container.Resolve<T>();
		}
		
		public static T[] ResolveAll<T>()
		{
			return _container.ResolveAll<T>();
		}
	}
}