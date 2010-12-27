using System;
using OpenIDENet.Versioning;
using System.IO;
namespace OpenIDENet.Projects.Readers
{
	public class DefaultReader : IReadProjectFiles<VS2010>
	{
		public IProject Read(string fullPath)
		{
			return new Project(fullPath, File.ReadAllText(fullPath));
		}
	}
}

