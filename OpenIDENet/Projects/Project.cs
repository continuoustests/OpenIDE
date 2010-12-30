using System;
using System.Collections.Generic;
namespace OpenIDENet.Projects
{
	public class Project : IProject
	{
		public string Fullpath { get; private set; }
		public object Content { get; private set; }
		public bool IsModified { get; private set; }
		
		public Project(string fullPath, string xml)
		{
			Fullpath = fullPath;
			Content = xml;
		}
		
		public void SetContent(object content)
		{
			if (Content.Equals(content))
				return;
			IsModified = true;
			Content = content;
		}
	}
}