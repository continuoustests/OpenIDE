using System;
using System.IO;

namespace CSharp.Files
{
	public class NoneFile : IFile
	{
		public string Fullpath { get; private set; }
		
		public NoneFile()
		{
		}

		public NoneFile(string path)
		{
			Fullpath = Path.GetFullPath(path);
		}

		public IFile New(string fullpath)
		{
			return new NoneFile(fullpath);
		}

		public bool SupportsExtension(string extension)
		{
			return true;
		}
	}
}
