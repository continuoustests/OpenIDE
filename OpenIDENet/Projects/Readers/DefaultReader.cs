using System;
using OpenIDENet.Versioning;
using System.IO;
namespace OpenIDENet.Readers
{
	public class DefaultReader : IReadProjectFiles<VS2010>
	{
		public string Read(string fullPath)
		{
			return File.ReadAllText(fullPath);
		}
	}
}

