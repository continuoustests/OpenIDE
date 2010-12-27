using System;
using System.Collections.Generic;
namespace OpenIDENet.Projects
{
	public class Project : IProject
	{
		public string Fullpath { get; private set; }
		public string Xml { get; private set; }
		
		public Project(string fullPath, string xml)
		{
			Fullpath = fullPath;
			Xml = xml;
		}
	}
}