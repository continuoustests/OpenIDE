using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Projects.Writers
{
	public interface IWriteProjectFileToDiskFor
	{
		bool SupportsVersion<T>();
		void Write(IProject project);
	}
}

