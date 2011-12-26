using System;
using System.Collections.Generic;
namespace OpenIDENet.CodeEngine.Core.Caching
{
	public class ProjectFile
	{
		public string File { get; private set; }
		public string Project { get; private set; }
		
		public ProjectFile(string file, string project)
		{
			File = file;
			Project = project;
		}
	}

	public class Project
	{
		public string File { get; private set; }
		
		public Project(string file)
		{
			File = file;
		}
	}
}

