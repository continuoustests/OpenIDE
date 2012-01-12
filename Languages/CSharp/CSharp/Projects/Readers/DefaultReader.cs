using System;
using CSharp.Versioning;
using System.IO;
using CSharp.FileSystem;
using System.Xml;

namespace CSharp.Projects.Readers
{
	public class DefaultReader : IReadProjectsFor
	{
		private IFS _fs;
		private XmlNamespaceManager _nsManager = null;
		
		public DefaultReader(IFS fs)
		{
			_fs = fs;
		}
		
		public bool SupportsProject<T>() where T : IAmProjectVersion
		{
			return typeof(T).Equals(typeof(VS2010));
		}
		
		public Project Read(string fullPath)
		{
			var content = _fs.ReadFileAsText(fullPath);
			return new Project(Path.GetFullPath(fullPath), content, getSettings(content));
		}
		
		private ProjectSettings getSettings(string content)
		{
			var ns = "ns";
			var guid = Guid.Empty;
			var document = new XmlDocument();
			if (tryOpen(document, content))
			{
                var node = document
					.SelectSingleNode(
						nsPrefix("||NS||Project/||NS||PropertyGroup/||NS||RootNamespace"), _nsManager);
				if (node != null)
					ns = node.InnerText;
				node = document
					.SelectSingleNode(
						nsPrefix("||NS||Project/||NS||PropertyGroup/||NS||ProjectGuid"), _nsManager);
				if (node != null)
					guid = new Guid(node.InnerText);
			}
			
			return new ProjectSettings()
				{
					Type = "C#",
					DefaultNamespace = ns,
					Guid = guid
				};
		}
		
		private bool tryOpen(XmlDocument document, string xml)
		{
			try
			{
				document.LoadXml(xml);
				if (xml.Contains("http://schemas.microsoft.com/developer/msbuild/2003"))
				{
					_nsManager = new XmlNamespaceManager(document.NameTable);
					_nsManager.AddNamespace("b", "http://schemas.microsoft.com/developer/msbuild/2003");
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

        private string nsPrefix(string text)
        {
            if (_nsManager == null)
                return text.Replace("||NS||", "");
            else
                return text.Replace("||NS||", "b:");
        }
	}
}

