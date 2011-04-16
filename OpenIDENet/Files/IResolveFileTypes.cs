using System;
using OpenIDENet.Files;
using OpenIDENet.Versioning;
namespace OpenIDENet.Files
{
	public interface IResolveFileTypes
	{
		bool SupportsProject<T>() where T : IAmProjectVersion;
		IFile Resolve(string fullPath);
	}
}

