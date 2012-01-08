using System;
using System.IO;

namespace CSharp.Files
{
	public class AssemblyFile : IFile
	{
		public string Fullpath { get; private set; }

		public AssemblyFile(string fullpath)
		{
			Fullpath = Path.GetFullPath(fullpath);
		}

		public static bool SupportsExtension(string fullpath)
		{
			var extension = Path.GetExtension(fullpath).ToLower();
			return extension == ".dll" ||
				   extension == ".exe";
		}
	}
}
