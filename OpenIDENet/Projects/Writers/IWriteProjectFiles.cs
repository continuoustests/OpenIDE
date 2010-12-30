using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Projects.Writers
{
	public interface IWriteProjectFiles<T> where T : IAmVisualStudioVersion
	{
		void Write(IProject project);
	}
}

