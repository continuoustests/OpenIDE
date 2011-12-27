using System;
using System.IO;

namespace CSharp.Files
{
	class VSProjectFile : IFile
	{
		public string Fullpath { get; private set; }

		public VSProjectFile(string fullpath)
		{
			Fullpath = Path.GetFullPath(fullpath);
		}
		
		public static bool SupportsExtension(string fullpath)
		{
			var extension = Path.GetExtension(fullpath).ToLower();
			return extension == ".csproj" ||
				   extension == ".vbproj";
		}
	}
}
