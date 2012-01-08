using System;
using CSharp.Files;
using CSharp.Versioning;

namespace CSharp.Files
{
	public interface IResolveFileTypes
	{
		bool SupportsProject<T>() where T : IAmProjectVersion;
		IFile Resolve(string fullPath);
	}
}

