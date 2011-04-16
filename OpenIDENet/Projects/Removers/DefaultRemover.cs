using System;
using OpenIDENet.Files;
using OpenIDENet.Messaging;
using OpenIDENet.FileSystem;
using System.Xml;
using OpenIDENet.Versioning;
using OpenIDENet.Messaging.Messages;
namespace OpenIDENet.Projects.Removers
{
	class DefaultRemover : IRemoveFiles
	{
		private IMessageBus _bus;
		private IFS _fs;
		private XmlNamespaceManager _nsManager = null;
		
		public DefaultRemover(IMessageBus bus, IFS fs)
		{
			_bus = bus;
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

		public void Remove(IProject project, IFile file)
		{
			var document = new XmlDocument();
			if (!tryOpen(document, project.Content.ToString()))
			{
				_bus.Publish(new FailMessage(string.Format("Could not remove file. Invalid project file {0}", project.Fullpath)));
				return;
			}
			
			if (!_fs.FileExists(file.Fullpath))
				return;
			
			var relativePath = PathExtensions.GetRelativePath(project.Fullpath, file.Fullpath).Replace("/", "\\");
			var node = getNode(document, relativePath);
			if (node == null)
				return;
			node.ParentNode.RemoveChild(node);
			project.SetContent(document.OuterXml);
		}
			    
		private XmlNode getNode(XmlDocument document, string file)
		{
			return document.SelectSingleNode(string.Format("b:Project/b:ItemGroup/b:Compile[contains(@Include,'{0}')]", file), _nsManager);
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

