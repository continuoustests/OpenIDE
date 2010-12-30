using System;
using System.IO;
namespace OpenIDENet.FileSystem
{
	public class FS : IFS
	{
		public bool FileExists(string file)
		{
			return File.Exists(file);
		}
	}
}

