using System;
using System.Collections.Generic;
using CSharp.Crawlers;

namespace CSharp.Projects
{
    public class FileRef : ISourceItem
	{
        public long ID { get; private set; }
		public string File { get; private set; }
        public Project Project { get; private set; }
        public string Signature { get { return File; } }

        public FileRef(string file, Project project)
		{
			File = file;
			Project = project;
		}

        public void SetID(long id) {
            ID = id;
        }

        public override string ToString() {
            return File;
        }
	}

    public class Project : ISourceItem
	{
        public long ID { get; private set; }
		public string File { get; private set; }
		public object Content { get; private set; }
		public bool IsModified { get; private set; }
        public string JSON { get; private set; }
        public string Signature { get { return File; } }
		
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

        public void SetID(long id) {
            ID = id;
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
