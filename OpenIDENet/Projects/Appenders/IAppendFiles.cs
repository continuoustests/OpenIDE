using System;
using OpenIDENet.Versioning;
using OpenIDENet.Files;
namespace OpenIDENet.Projects.Appenders
{
	public interface IAppendFiles
	{
		bool SupportsProject<T>() where T : IAmProjectVersion;
		bool SupportsFile(IFile file);
		void Append(IProject project, IFile file);
	}
}

