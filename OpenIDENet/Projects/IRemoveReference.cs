using System;
using OpenIDENet.Versioning;
using OpenIDENet.Files;

namespace OpenIDENet.Projects
{
	public interface IRemoveReference
	{
		bool SupportsProject<T>() where T : IAmProjectVersion;
		bool SupportsFile(IFile file);
		void Dereference(IProject project, IFile file);	
	}
}
