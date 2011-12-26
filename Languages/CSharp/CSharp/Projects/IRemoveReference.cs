using System;
using CSharp.Versioning;
using CSharp.Files;

namespace CSharp.Projects
{
	public interface IRemoveReference
	{
		bool SupportsProject<T>() where T : IAmProjectVersion;
		bool SupportsFile(IFile file);
		void Dereference(Project project, IFile file);	
	}
}
