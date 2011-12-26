using System;
using CSharp.Versioning;
using System.Xml;
using CSharp.FileSystem;
using CSharp.Files;
using System.IO;
using System.Xml.Schema;

namespace CSharp.Projects.Appenders
{
	public class VSFileAppender : IAppendFiles
	{
		private IFS _fs;
		private XmlNamespaceManager _nsManager = null;
		
		public VSFileAppender(IFS fs)
		{
			_fs = fs;
		}
		
		public bool SupportsProject<T>() where T : IAmProjectVersion
		{
			return typeof(T).Equals(typeof(VS2010));
		}
		
		public bool SupportsFile(IFile file)
		{
			return file.GetType().Equals(typeof(CompileFile));
		}
		
		public void Append(Project project, IFile file)
		{
			var document = new XmlDocument();
			if (!tryOpen(document, project.Content.ToString()))
			{
				_bus.Publish(new FailMessage(string.Format("Could not append file. Invalid project file {0}", project.Fullpath)));
				return;
			}
			
			if (!_fs.FileExists(file.Fullpath))
			{
				_bus.Publish(new FailMessage(string.Format("Could not append unexisting file {0}", file.Fullpath)));
				return;
			}
			
			var relativePath = PathExtensions.GetRelativePath(project.Fullpath, file.Fullpath).Replace("/", "\\");
			if (exists(document, relativePath))
				return;
			var node = getCompileItemGroup(document, project.Fullpath);
			if (node == null)
				return;
			appendFile(document, node, relativePath);
			project.SetContent(document.OuterXml);
		}
			    
		private bool exists(XmlDocument document, string file)
		{
            var node = document.SelectSingleNode(nsPrefix(string.Format("||NS||Project/||NS||ItemGroup/||NS||Compile[contains(@Include,'{0}')]", file)), _nsManager);
			return node != null;
		}
		
		private XmlNode getCompileItemGroup(XmlDocument document, string project)
		{
            var node = document.SelectSingleNode(nsPrefix("||NS||Project/||NS||ItemGroup/||NS||Compile"), _nsManager);
			if (node == null)
				node = createCompileGroup(document);
			else
				node = node.ParentNode;
			if (node == null)
				_bus.Publish(new FailMessage(string.Format("Could not append file. Project file is not of known format {0}", project)));
			return node;
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
		
		private XmlNode createCompileGroup(XmlDocument document)
		{
            var element = document.SelectSingleNode(nsPrefix("||NS||Project"), _nsManager);
			
			if (element == null)
				return null;
		 	var node = document.CreateNode(XmlNodeType.Element, "ItemGroup", element.NamespaceURI);
			element.AppendChild(node);
			return node;
		}
		
		private void appendFile(XmlDocument document, XmlNode parent, string file)
		{
			var node = document.CreateNode(XmlNodeType.Element, "Compile", parent.NamespaceURI);
			var fileAttribute = document.CreateAttribute("Include");
			fileAttribute.Value = file;
			node.Attributes.Append(fileAttribute);
			parent.AppendChild(node);
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

