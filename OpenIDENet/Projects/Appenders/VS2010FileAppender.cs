using System;
using OpenIDENet.Versioning;
using System.Xml;
using OpenIDENet.Messaging;
using OpenIDENet.Messaging.Messages;
namespace OpenIDENet.Projects.Appenders
{
	public class VS2010FileAppender : IAppendFiles<VS2010>
	{
		private IMessageBus _bus;
		
		public VS2010FileAppender(IMessageBus bus)
		{
			_bus = bus;
		}
		
		public void Append(IProject project, string file)
		{
			// Pull file relative path from project path
			try
			{
				var document = new XmlDocument();
				document.LoadXml(project.Xml);
				var node = document.SelectSingleNode("Project/ItemGroup/Compile");
			}
			catch
			{
				_bus.Publish(new FailMessage(string.Format("Could not append file. Invalid project file {0}", project.Fullpath)));
				return;
			}
			
			/*
			if (node == null)
				// Create item group for compile files
			else
				// check wether file exists already
				// if not add it
			*/
		}
	}
}

