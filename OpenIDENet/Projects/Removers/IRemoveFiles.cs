using System;
using OpenIDENet.Versioning;
using OpenIDENet.Files;
using OpenIDENet.Projects;
namespace OpenIDENet.Projects.Removers
{
	public interface IRemoveFiles
	{
		bool SupportsProject<T>() where T : IAmProjectVersion;
		bool SupportsFile(IFile file);
		void Remove(IProject project, IFile file);
	}
}

