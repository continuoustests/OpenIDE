using System;
using CSharp.Files;
using CSharp.FileSystem;
using System.Xml;
using CSharp.Versioning;

namespace CSharp.Projects.Removers
{
	public class DefaultRemover : IRemoveFiles
	{
		private IFS _fs;
		private XmlNamespaceManager _nsManager = null;
		
		public DefaultRemover(IFS fs)
		{
			_fs = fs;
		}
		
		public bool SupportsProject<T>() where T : IAmProjectVersion
		{
			return typeof(T).Equals(typeof(VS2010));
		}

		public bool SupportsFile(IFile file)
		{
			return file.GetType().Equals(typeof(CompileFile)) ||
				   file.GetType().Equals(typeof(NoneFile));
		}

		public void Remove(Project project, IFile file)
		{
			var document = new XmlDocument();
			if (!tryOpen(document, project.Content.ToString()))
			{
				Console.WriteLine("Could not remove file. Invalid project file {0}", project.File);
				return;
			}
			
			var relativePath = PathExtensions.GetRelativePath(project.File, file.Fullpath).Replace("/", "\\");
			var node = getNode(document, relativePath);
			if (node == null)
				return;
			node.ParentNode.RemoveChild(node);
			project.SetContent(document.OuterXml);
		}
			    
		private XmlNode getNode(XmlDocument document, string file)
		{
			var node = document.SelectSingleNode(string.Format("b:Project/b:ItemGroup/b:Compile[contains(@Include,'{0}')]", file), _nsManager);
			if (node != null)
				return node;
			node = document.SelectSingleNode(string.Format("b:Project/b:ItemGroup/b:None[contains(@Include,'{0}')]", file), _nsManager);
			if (node != null)
				return node;
			return null;
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

