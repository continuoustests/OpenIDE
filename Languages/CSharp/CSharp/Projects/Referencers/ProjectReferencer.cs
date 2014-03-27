using System;
using System.Xml;
using System.IO;
using CSharp.Versioning;
using CSharp.Files;
using CSharp.FileSystem;

namespace CSharp.Projects.Referencers
{
	public class ProjectReferencer : ProjectXML, IAddReference, IRemoveReference
	{
		private IFS _fs;
		private Func<string, ProviderSettings> _getTypesProviderByProject;

		public ProjectReferencer(IFS fs, Func<string, ProviderSettings> getTypesProviderByProject)
		{
			_fs = fs;
			_getTypesProviderByProject = getTypesProviderByProject;
		}

		public bool SupportsProject<T>() where T : IAmProjectVersion
		{
			return typeof(T).Equals(typeof(VS2010));
		}

		public bool SupportsFile(IFile file)
		{
			return file.GetType().Equals(typeof(VSProjectFile));
		}
		
		public void Reference(Project project, IFile file)
		{
			if (!_fs.FileExists(file.Fullpath))
			{
				Console.WriteLine("Could not reference unexisting project {0}", file.Fullpath);
				return;
			}

			if (!tryOpen(project.Content.ToString()))
				return;
			
			var relativePath =
				new PathParser(file.Fullpath)
					.RelativeTo(project.File)
					.Replace("/", "\\");

			if (alreadyReferenced(relativePath))
				return;

			var handler = new ProjectHandler();
			handler.Read(file.Fullpath, _getTypesProviderByProject);
			
			var parent = getReferenceGroup(project.File);
			var node = _document.CreateNode(XmlNodeType.Element, "ProjectReference", parent.NamespaceURI);
			var fileAttribute = _document.CreateAttribute("Include");
			fileAttribute.Value = relativePath;
			node.Attributes.Append(fileAttribute);
			node.AppendChild(createNode("Project", "{" + handler.Guid.ToString() + "}", parent));
			node.AppendChild(createNode("Name", Path.GetFileNameWithoutExtension(file.Fullpath), parent));
			parent.AppendChild(node);
			project.SetContent(_document.OuterXml);
		}
		
		public void Dereference(Project project, IFile file)
		{
			if (!tryOpen(project.Content.ToString()))
				return;
			
			var relativePath =
				new PathParser(file.Fullpath)
					.RelativeTo(project.File)
					.Replace("/", "\\");
			
			removeReferences(relativePath);
			project.SetContent(_document.OuterXml);
		}

		private XmlNode createNode(string name, string nodeValue, XmlNode parent)
		{
			var node = _document.CreateNode(XmlNodeType.Element, name, parent.NamespaceURI);
			node.InnerText = nodeValue;
			return node;
		}
		
		private bool alreadyReferenced(string path)
		{
			var nodes = _document
				.SelectNodes(
					nsPrefix("||NS||Project/||NS||ItemGroup/||NS||ProjectReference", path), _nsManager);
			foreach (XmlNode node in nodes)
			{
				var attr = node.Attributes["Include"];
				if (attr == null)
					continue;
				if (attr.InnerText.Contains(path))
					return true;
			}
			return false;
		}
		
		private void removeReferences(string name)
		{
			var nodes = _document
				.SelectNodes(
					nsPrefix("||NS||Project/||NS||ItemGroup/||NS||ProjectReference", name), _nsManager);
			foreach (XmlNode node in nodes)
			{
				var attr = node.Attributes["Include"];
				if (attr == null)
					continue;
				if (attr.InnerText.Contains(name))
					node.ParentNode.RemoveChild(node);
			}
		}

		private XmlNode getReferenceGroup(string project)
		{
            var node = _document
				.SelectSingleNode(
					nsPrefix("||NS||Project/||NS||ItemGroup/||NS||ProjectReference"), _nsManager);
			if (node == null)
				node = createReferenceGroup(_document);
			else
				node = node.ParentNode;
			if (node == null)
				Console.WriteLine("Could not reference file. Project file is not of known format {0}", project);
			return node;
		}

		private XmlNode createReferenceGroup(XmlDocument document)
		{
            var element = document.SelectSingleNode(nsPrefix("||NS||Project"), _nsManager);
			
			if (element == null)
				return null;
		 	var node = document.CreateNode(XmlNodeType.Element, "ItemGroup", element.NamespaceURI);
			element.AppendChild(node);
			return node;
		}
	}
}
