using System;
using OpenIDENet.Versioning;
using OpenIDENet.Files;

namespace OpenIDENet.Projects
{
	public interface IAddReference
	{
		bool SupportsProject<T>() where T : IAmProjectVersion;
		bool SupportsFile(IFile file);
		void Reference(IProject project, IFile file);	
	}
}
