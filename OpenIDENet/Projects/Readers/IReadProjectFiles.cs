using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Projects.Readers
{
	public interface IReadProjectFiles<T> where T : IAmVisualStudioVersion
	{
		IProject Read(string fullPath);
	}
}

