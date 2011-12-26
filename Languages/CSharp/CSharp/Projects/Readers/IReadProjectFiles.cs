using System;
using CSharp.Versioning;

namespace CSharp.Projects.Readers
{
	public interface IReadProjectsFor
	{
		bool SupportsProject<T>() where T : IAmProjectVersion;
		Project Read(string fullPath);
	}
}

