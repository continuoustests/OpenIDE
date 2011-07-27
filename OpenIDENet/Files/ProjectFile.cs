using System;
using System.IO;

namespace OpenIDENet.Files
{
	class ProjectFile : IFile
	{
		public string Fullpath { get; private set; }

		public ProjectFile(string fullpath)
		{
			Fullpath = Path.GetFullPath(fullpath);
		}

		public static bool Supports(string fullpath)
		{
			var extension = Path.GetExtension(fullpath).ToLower();
			return extension == ".csproj" ||
				   extension == ".vbproj";
		}
	}
}
