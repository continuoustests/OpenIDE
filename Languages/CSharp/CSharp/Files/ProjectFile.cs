using System;
using System.IO;

namespace CSharp.Files
{
	class VSProjectFile : IFile
	{
		public string Fullpath { get; private set; }

		public VSProjectFile()
		{
		}
		
		public VSProjectFile(string fullpath)
		{
			Fullpath = Path.GetFullPath(fullpath);
		}

		public IFile New(string fullpath)
		{
			return new VSProjectFile(fullpath);
		}
		
		public bool SupportsExtension(string fullpath)
		{
			var extension = Path.GetExtension(fullpath).ToLower();
			return extension == ".csproj" ||
				   extension == ".vbproj";
		}
	}
}
