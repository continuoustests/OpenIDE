using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Readers
{
	public interface IReadProjectFiles<T> where T : IAmVisualStudioVersion
	{
		string Read(string fullPath);
	}
}

