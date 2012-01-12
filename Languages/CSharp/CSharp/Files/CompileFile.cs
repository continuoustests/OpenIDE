using System;
using System.IO;

namespace CSharp.Files
{
	public class CompileFile : IFile
	{
		public string Fullpath { get; private set; }
		
		public CompileFile() { }
		
		public CompileFile(string fullPath)
		{
			Fullpath = Path.GetFullPath(fullPath);
		}

		public IFile New(string fullPath)
		{
			return new CompileFile(fullPath);
		}
		
		public bool SupportsExtension(string extension)
		{
			extension = extension.ToLower();
			return extension.Equals(".cs");
		}
	}
}

