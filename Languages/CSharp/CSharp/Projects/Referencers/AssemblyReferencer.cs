using System;
using System.Xml;
using System.Collections.Generic;
using CSharp.Versioning;
using CSharp.Files;
using CSharp.FileSystem;

namespace CSharp.Projects.Referencers
{
	public class AssemblyReferencer : ProjectXML, IAddReference, IRemoveReference
	{
		private IFS _fs;
		
		public AssemblyReferencer(IFS fs)
		{
			_fs = fs;
		}

		public bool SupportsProject<T>() where T : IAmProjectVersion
		{
			return typeof(T).Equals(typeof(VS2010));
		}

		public bool SupportsFile(IFile file)
		{
			return file.GetType().Equals(typeof(AssemblyFile));
		}
		
		public void Reference(Project project, IFile file)
		{
			if (!_fs.FileExists(file.Fullpath))
			{
				Console.WriteLine("Could not reference unexisting assembly {0}", file.Fullpath);
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
			var parent = getReferenceGroup(project.File);
			var node = _document.CreateNode(XmlNodeType.Element, "Reference", parent.NamespaceURI);
			var fileAttribute = _document.CreateAttribute("Include");
			fileAttribute.Value = relativePath;
			node.Attributes.Append(fileAttribute);
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

		private bool alreadyReferenced(string path)
		{
			var nodes = _document
				.SelectNodes(
					nsPrefix("||NS||Project/||NS||ItemGroup/||NS||Reference", path), _nsManager);
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
					nsPrefix("||NS||Project/||NS||ItemGroup/||NS||Reference", name), _nsManager);
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
				.SelectSingleNode(nsPrefix("||NS||Project/||NS||ItemGroup/||NS||Reference"), _nsManager);
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
