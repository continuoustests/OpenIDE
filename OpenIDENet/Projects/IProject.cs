using System;
using System.Collections.Generic;
namespace OpenIDENet.Projects
{
	public interface IProject
	{
		string Name { get; }
		IEnumerable<IFile> Files { get; }
	}
}

