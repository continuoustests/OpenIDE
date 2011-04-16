using System;
using System.IO;
namespace OpenIDENet.Files
{
	public class CompileFile : IFile
	{
		public string Fullpath { get; private set; }
		
		public CompileFile(string fullPath)
		{
			Fullpath = Path.GetFullPath(fullPath);
		}
		
		public static bool SupportsExtension(string extension)
		{
			extension = extension.ToLower();
			return extension.Equals(".cs");
		}
		
		public static string DefaultExtensionFor(ProjectType type)
		{
			if (type == ProjectType.CSharp)
				return ".cs";
			throw new Exception(string.Format("Unhandled project type {0}", type.ToString()));
		}
	}
}

