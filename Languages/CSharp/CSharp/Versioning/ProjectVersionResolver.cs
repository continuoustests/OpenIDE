using System;
namespace CSharp.Versioning
{
	public class ProjectVersionResolver : IResolveProjectVersion
	{
		private IProvideVersionedTypes[] _typeProviders;
		
		public ProjectVersionResolver(IProvideVersionedTypes[] typeProviders)
		{
			_typeProviders = typeProviders;
		}
		
		public IProvideVersionedTypes ResolveFor(string fullPath)
		{
			if (new VS2010().IsValid(fullPath))
				return getProvider<VS2010>();
			return null;
		}
		
		private IProvideVersionedTypes getProvider<T>() where T : IAmProjectVersion
		{
			foreach (var provider in _typeProviders)
			{
				if (provider.GetType().Equals(typeof(VersionedTypeProvider<T>)))
					return provider;
			}
			return null;
		}
	}
}

