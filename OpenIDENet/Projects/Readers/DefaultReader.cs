using System;
using OpenIDENet.Versioning;
using System.IO;
namespace OpenIDENet.Projects.Readers
{
	public class DefaultReader : IReadProjectsFor
	{
		public bool SupportsVersion<T>()
		{
			return typeof(T).Equals(typeof(VS2010));
		}
		
		public IProject Read(string fullPath)
		{
			return new Project(Path.GetFullPath(fullPath), File.ReadAllText(fullPath));
		}
	}
}

