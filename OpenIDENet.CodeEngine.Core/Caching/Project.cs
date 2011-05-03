using System;
using System.Collections.Generic;
namespace OpenIDENet.CodeEngine.Core.Caching
{
	public class Project
	{
		public string Fullpath { get; private set; }
		public List<string> Files { get; private set; }
		
		public Project(string fullpath)
		{
			Fullpath = fullpath;
			Files = new List<string>();
		}
	}
}

