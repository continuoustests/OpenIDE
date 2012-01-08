using System;
using CSharp.Versioning;
using CSharp.Files;
using CSharp.Projects;

namespace CSharp.Projects.Removers
{
	public interface IRemoveFiles
	{
		bool SupportsProject<T>() where T : IAmProjectVersion;
		bool SupportsFile(IFile file);
		void Remove(Project project, IFile file);
	}
}

