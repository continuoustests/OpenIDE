using System;
using System.IO;

namespace CSharp.Files
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
		
		public static string DefaultExtensionFor(SupportedLanguage type)
		{
			if (type == SupportedLanguage.CSharp)
				return ".cs";
			throw new Exception(string.Format("Unhandled project type {0}", type.ToString()));
		}
	}
}

