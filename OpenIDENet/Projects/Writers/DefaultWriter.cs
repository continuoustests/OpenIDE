using System;
using OpenIDENet.Versioning;
using System.IO;
using System.Xml;
namespace OpenIDENet.Projects.Writers
{
	public class DefaultWriter : IWriteProjectFiles<VS2010>
	{
		public void Write(IProject project)
		{
			if (project.IsModified)
			{
				var xml = new XmlDocument();
				xml.LoadXml(project.Xml);
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.NewLineChars = Environment.NewLine;
				using (XmlWriter writer = XmlTextWriter.Create(project.Fullpath, settings))
				{
				    xml.Save(writer);
				}
			}
		}
	}
}

