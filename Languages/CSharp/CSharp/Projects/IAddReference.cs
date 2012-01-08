using System;
using CSharp.Versioning;
using CSharp.Files;

namespace CSharp.Projects
{
	public interface IAddReference
	{
		bool SupportsProject<T>() where T : IAmProjectVersion;
		bool SupportsFile(IFile file);
		void Reference(Project project, IFile file);	
	}
}
