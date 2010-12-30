using System;
using OpenIDENet.Versioning;
using System.Xml;
using OpenIDENet.Messaging;
using OpenIDENet.Messaging.Messages;
using OpenIDENet.FileSystem;
using System.IO;
using System.Xml.Schema;
namespace OpenIDENet.Projects.Appenders
{
	public class VS2010FileAppender : IAppendFiles<VS2010>
	{
		private IMessageBus _bus;
		private XmlNamespaceManager _nsManager = null;
		
		public VS2010FileAppender(IMessageBus bus)
		{
			_bus = bus;
		}
		
		public void Append(IProject project, string file)
		{
			var document = new XmlDocument();
			if (!tryOpen(document, project.Xml))
			{
				_bus.Publish(new FailMessage(string.Format("Could not append file. Invalid project file {0}", project.Fullpath)));
				return;
			}
			
			var relativePath = PathExtensions.GetRelativePath(project.Fullpath, file);
			if (exists(document, relativePath))
				return;
			var node = getCompileItemGroup(document, project.Fullpath);
			if (node == null)
				return;
			appendFile(document, node, relativePath);
			project.SetXml(document.OuterXml);
		}
			    
		private bool exists(XmlDocument document, string file)
		{
			var node = document.SelectSingleNode(string.Format("b:Project/b:ItemGroup/b:Compile[contains(@Include,'{0}')]", file), _nsManager);
			return node != null;
		}
		
		private XmlNode getCompileItemGroup(XmlDocument document, string project)
		{
			var node = document.SelectSingleNode("b:Project/b:ItemGroup/b:Compile", _nsManager);
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
			var element = document.SelectSingleNode("b:Project", _nsManager);
			
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
}

