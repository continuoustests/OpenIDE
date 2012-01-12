using System;
using System.IO;
using System.Linq;
using CSharp.Versioning;

namespace CSharp.Files
{
	public class VSFileTypeResolver : IResolveFileTypes
	{
		private IFile[] _fileTypes;

		public VSFileTypeResolver(IFile[] fileTypes)
		{
			_fileTypes = fileTypes;
		}

		public bool SupportsProject<T>() where T : IAmProjectVersion
		{
			return typeof(T).Equals(typeof(VS2010));
		}
		
		public IFile Resolve(string fullPath)
		{
			var extension = Path.GetExtension(fullPath);
			var type = _fileTypes.FirstOrDefault(x => x.SupportsExtension(extension));
			if (type == null)
				return null;
			return type.New(fullPath);
		}
	}
}

