using System;
using CSharp.Versioning;
using CSharp.Files;
namespace CSharp.Projects.Appenders
{
	public interface IAppendFiles
	{
		bool SupportsProject<T>() where T : IAmProjectVersion;
		bool SupportsFile(IFile file);
		void Append(IProject project, IFile file);
	}
}

