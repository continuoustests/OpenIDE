using System;
using System.IO;

namespace OpenIDENet.Files
{
	public class AssemblyFile : IFile
	{
		public string Fullpath { get; private set; }

		public AssemblyFile(string fullpath)
		{
			Fullpath = Path.GetFullPath(fullpath);
		}
	}
}
