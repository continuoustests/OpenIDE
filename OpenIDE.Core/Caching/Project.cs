using System;
using System.Collections.Generic;
namespace OpenIDE.Core.Caching
{
	public class ProjectFile
	{
		public string File { get; private set; }
		public string Project { get; private set; }

		public bool FileSearch { get; private set; }
		
		public ProjectFile(string file, string project)
		{
			File = file;
			Project = project;
		}

		public ProjectFile SetFileSearch()
		{
			FileSearch = true;
			return this;
		}

		public ProjectFile Update(string project, bool fileSearch)
		{
			Project = project;
			FileSearch = fileSearch;
			return this;
		}
	}

	public class Project
	{
		public string File { get; private set; }

        public string JSON { get; private set; }
		
		public bool FileSearch { get; private set; }
		
		public Project(string file, string json)
		{
			File = file;
            JSON = json;
		}

		public Project SetFileSearch()
		{
			FileSearch = true;
			return this;
		}

		public void Update(string json, bool fileSearch)
		{
			JSON = json;
			FileSearch = fileSearch;
		}
	}
}

