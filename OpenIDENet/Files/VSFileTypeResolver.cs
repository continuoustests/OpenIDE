using System;
using System.IO;
using OpenIDENet.Versioning;
namespace OpenIDENet.Files
{
	public class VSFileTypeResolver : IResolveFileTypes
	{
		public bool SupportsProject<T>() where T : IAmProjectVersion
		{
			return typeof(T).Equals(typeof(VS2010));
		}
		
		public IFile Resolve(string fullPath)
		{
			var extension = Path.GetExtension(fullPath);
			if (CompileFile.SupportsExtension(extension))
				return new CompileFile(fullPath);
			if (AssemblyFile.SupportsExtension(extension))
				return new AssemblyFile(fullPath);
			if (ProjectFile.SupportsExtension(extension))
				return new ProjectFile(fullPath);
			return null;
		}
	}
}

