using System;
using System.Linq;
using System.Collections.Generic;
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
		private List<IVSFileAppender> _appenders = new List<IVSFileAppender>();
		
		public VSFileAppender(IFS fs)
		{
			_fs = fs;
			Func<XmlNamespaceManager> getNSManager = () => { return _nsManager; };
			_appenders.AddRange(
				new IVSFileAppender[]
					{
						new CompileFileAppender(getNSManager, nsPrefix),
						new NoneFileAppender(getNSManager, nsPrefix)
					});
		}
		
		public bool SupportsProject<T>() where T : IAmProjectVersion
		{
			return typeof(T).Equals(typeof(VS2010));
		}
		
		public bool SupportsFile(IFile file)
		{
			return _appenders.Count(x => x.AppendsFor(file)) > 0;
		}
		
		public void Append(Project project, IFile file)
		{
			var document = new XmlDocument();
			if (!tryOpen(document, project.Content.ToString()))
			{
				
				Console.WriteLine("Could not append file. Invalid project file {0}", project.File);
				return;
			}
			
			if (!_fs.FileExists(file.Fullpath))
			{
				Console.WriteLine("Could not append unexisting file {0}", file.Fullpath);
				return;
			}
			
			_appenders
				.Where(x => x.AppendsFor(file)).ToList()
				.ForEach(x => x.Append(document, project, file));

			project.SetContent(document.OuterXml);
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

	interface IVSFileAppender
	{
		bool AppendsFor(IFile file);
		void Append(XmlDocument document, Project project, IFile file);
	}

	class CompileFileAppender : IVSFileAppender
	{
		private XmlNamespaceManager _nsManager { get { return getNSManager(); } }
		private Func<XmlNamespaceManager> getNSManager;
		private Func<string,string> nsPrefix;

		public CompileFileAppender(Func<XmlNamespaceManager> nsManager, Func<string,string> prefixer)
		{
			nsPrefix = prefixer;
			getNSManager = nsManager;
		}

		public bool AppendsFor(IFile file)
		{
			return file.GetType().Equals(typeof(CompileFile));
		}

		public void Append(XmlDocument document, Project project, IFile file)
		{
			var relativePath = PathExtensions.GetRelativePath(project.File, file.Fullpath).Replace("/", "\\");
			if (exists(document, relativePath))
				return;
			var node = getCompileItemGroup(document, project.File);
			if (node == null)
				return;
			appendFile(document, node, relativePath);
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
				Console.WriteLine("Could not append file. Project file is not of known format {0}", project);
			return node;
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
	}
	
	class NoneFileAppender : IVSFileAppender
	{
		private XmlNamespaceManager _nsManager { get { return getNSManager(); } }
		private Func<XmlNamespaceManager> getNSManager;
		private Func<string,string> nsPrefix;

		public NoneFileAppender(Func<XmlNamespaceManager> nsManager, Func<string,string> prefixer)
		{
			nsPrefix = prefixer;
			getNSManager = nsManager;
		}

		public bool AppendsFor(IFile file)
		{
			return file.GetType().Equals(typeof(NoneFile));
		}

		public void Append(XmlDocument document, Project project, IFile file)
		{
			var relativePath = PathExtensions.GetRelativePath(project.File, file.Fullpath).Replace("/", "\\");
			if (exists(document, relativePath))
				return;
			var node = getNoneItemGroup(document, project.File);
			if (node == null)
				return;
			appendFile(document, node, relativePath);
		}

		private bool exists(XmlDocument document, string file)
		{
            var node = document.SelectSingleNode(nsPrefix(string.Format("||NS||Project/||NS||ItemGroup/||NS||None[contains(@Include,'{0}')]", file)), _nsManager);
			return node != null;
		}
		
		private XmlNode getNoneItemGroup(XmlDocument document, string project)
		{
            var node = document.SelectSingleNode(nsPrefix("||NS||Project/||NS||ItemGroup/||NS||None"), _nsManager);
			if (node == null)
				node = createNoneGroup(document);
			else
				node = node.ParentNode;
			if (node == null)
				Console.WriteLine("Could not append file. Project file is not of known format {0}", project);
			return node;
		}
		
		private XmlNode createNoneGroup(XmlDocument document)
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
			var node = document.CreateNode(XmlNodeType.Element, "None", parent.NamespaceURI);
			var fileAttribute = document.CreateAttribute("Include");
			fileAttribute.Value = file;
			node.Attributes.Append(fileAttribute);
			parent.AppendChild(node);
		}
	}
}

