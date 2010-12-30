using System;
namespace OpenIDENet.Versioning
{
	public interface IResolveProjectVersion
	{
		IProvideVersionedTypes ResolveFor(string fullPath);
	}
}

