using System;
using System.Collections.Generic;

namespace CSharp.Projects
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
		public object Content { get; private set; }
		public bool IsModified { get; private set; }
		
		public ProjectSettings Settings { get; private set; }
		
		public Project(string file)
		{
			File = file;
			Content = "";
			Settings = null;
		}

		public Project(string file, string xml, ProjectSettings settings)
		{
			File = file;
			Content = xml;
			Settings = settings;
		}
		
		public void SetContent(object content)
		{
			if (Content.Equals(content))
				return;
			IsModified = true;
			Content = content;
		}
	}

	public class ProjectSettings
	{
		public string Type { get; set; }
		public string DefaultNamespace { get; set; }
		public Guid Guid { get; set; }
	}
}
