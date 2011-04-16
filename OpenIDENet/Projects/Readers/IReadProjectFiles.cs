using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Projects.Readers
{
	public interface IReadProjectsFor
	{
		bool SupportsProject<T>() where T : IAmProjectVersion;
		IProject Read(string fullPath);
	}
}

