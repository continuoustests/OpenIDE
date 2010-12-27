using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Projects.Appenders
{
	public interface IAppendFiles<T> where T : IAmVisualStudioVersion
	{
		void Append(string fullPath);
	}
}

