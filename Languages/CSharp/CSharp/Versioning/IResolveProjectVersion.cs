using System;
namespace CSharp.Versioning
{
	public interface IResolveProjectVersion
	{
		IProvideVersionedTypes ResolveFor(string fullPath);
	}
}

