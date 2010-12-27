using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Writers
{
	public interface IWriteProjectFiles<T> where T : IAmVisualStudioVersion
	{
		void Write(string fullPath);
	}
}

