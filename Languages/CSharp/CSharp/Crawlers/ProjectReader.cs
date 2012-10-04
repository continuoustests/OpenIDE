using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using CSharp.FileSystem;

namespace CSharp.Crawlers
{
	public class ProjectReader
	{
		private string _path;
		private XmlDocument _xml = null;
		private XmlNamespaceManager _nsManager = null;
		
		public ProjectReader(string path)
		{
			_path = path;
		}

        public List<string> ReadReferences()
        {
            if (!readXml())
                return new List<string>();
            return getReferences();
        }
		
		public List<string> ReadFiles()
		{
			if (!readXml())
				return new List<string>();
			return getFiles();
		}
		
		private bool readXml()
		{
            if (_xml != null)
                return true;
			_xml = new XmlDocument();
			return tryOpen(_xml, File.ReadAllText(_path));
		}
		
		private List<string> getFiles()
		{
			var files = new List<string>();
            try {
			    files.AddRange(getFiles(_xml.SelectNodes("b:Project/b:ItemGroup/b:Compile", _nsManager)));
			    files.AddRange(getFiles(_xml.SelectNodes("b:Project/b:ItemGroup/b:Content", _nsManager)));
			    files.AddRange(getFiles(_xml.SelectNodes("b:Project/b:ItemGroup/b:None", _nsManager)));
            } catch {
            }
			return files;
		}

		private IEnumerable<string> getFiles(XmlNodeList nodes)
		{
			var files = new List<string>();
			foreach (XmlNode node in nodes)
			{
				var relativePath = node.Attributes["Include"];
				if (relativePath == null)
					continue;
				var file = new PathParser(relativePath.InnerText.Replace('\\', Path.DirectorySeparatorChar)).ToAbsolute(Path.GetDirectoryName(_path));
				if (!File.Exists(file))
					continue;
				files.Add(file);
			}
			return files;
		}

        private List<string> getReferences()
        {
            var refs = new List<string>();
            try {
                foreach (XmlNode node in _xml.SelectNodes("b:Project/b:ItemGroup/b:Reference", _nsManager))
                {
                    var relativePath = node.Attributes["Include"];
                    if (relativePath == null)
                        continue;
                    var reference = relativePath.InnerText;
                    var hintPath = node.SelectSingleNode("b:HintPath", _nsManager);
                    if (hintPath != null)
                        reference = hintPath.InnerText;
                    var referenceFile = 
                        new PathParser(reference.Replace('\\', Path.DirectorySeparatorChar))
                        .ToAbsolute(Path.GetDirectoryName(_path));
                    if (File.Exists(referenceFile))
                        refs.Add(referenceFile);
                    else
                        refs.Add(reference);
                }
            } catch {

            }
            return refs;
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
	}
}

