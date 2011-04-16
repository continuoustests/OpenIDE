using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Projects.Writers
{
	public interface IWriteProjectFileToDiskFor
	{
		bool SupportsProject<T>() where T : IAmProjectVersion;
		void Write(IProject project);
	}
}

