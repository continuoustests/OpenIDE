using System;
using CSharp.Versioning;
using System.IO;
using System.Xml;

namespace CSharp.Projects.Writers
{
	public class DefaultWriter : IWriteProjectFileToDiskFor
	{
		public bool SupportsProject<T>() where T : IAmProjectVersion
		{
			return typeof(T).Equals(typeof(VS2010));
		}
		
		public void Write(Project project)
		{
			if (project.IsModified)
			{
				var xml = new XmlDocument();
				xml.LoadXml(project.Content.ToString());
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.NewLineChars = Environment.NewLine;
				using (XmlWriter writer = XmlTextWriter.Create(project.File, settings))
				{
				    xml.Save(writer);
				}
			}
		}
	}
}

