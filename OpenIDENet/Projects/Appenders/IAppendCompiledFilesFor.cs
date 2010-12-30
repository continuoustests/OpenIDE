using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Projects.Appenders
{
	public interface IAppendCompiledFilesFor
	{
		bool SupportsVersion<T>();
		void Append(IProject project, string file);
	}
}

