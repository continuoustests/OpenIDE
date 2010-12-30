using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Projects.Readers
{
	public interface IReadProjectsFor
	{
		bool SupportsVersion<T>();
		IProject Read(string fullPath);
	}
}

