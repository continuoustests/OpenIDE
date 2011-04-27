using System;
namespace OpenIDENet.CodeEngine.Core.Crawlers
{
	public class Project
	{
		public string Fullpath { get; private set; }
		
		public Project(string path)
		{
			Fullpath = path;
		}
	}
}

