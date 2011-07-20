using System;
using System.Xml;
using System.Xml.Schema;
using OpenIDENet.Messaging;
using OpenIDENet.Messaging.Messages;

namespace OpenIDENet.Projects
{
	public class ProjectXML
	{
		protected IMessageBus _bus;
		protected XmlDocument _document = new XmlDocument();
		protected XmlNamespaceManager _nsManager = null;
		
		public ProjectXML(IMessageBus bus)
		{
			_bus = bus;
		}

		protected bool tryOpen(string xml)
		{
			try
			{
				_document.LoadXml(xml);
				if (xml.Contains("http://schemas.microsoft.com/developer/msbuild/2003"))
				{
					_nsManager = new XmlNamespaceManager(_document.NameTable);
					_nsManager.AddNamespace("b", "http://schemas.microsoft.com/developer/msbuild/2003");
				}
				return true;
			}
			catch
			{
				_bus.Publish(new FailMessage(
					string.Format("Invalid project file {0}", xml)));
				return false;
			}
		}	

		protected string nsPrefix(string text, params object[] args)
        {
            if (_nsManager == null)
                return string.Format(text, args).Replace("||NS||", "");
            else
                return string.Format(text, args).Replace("||NS||", "b:");
        }	
	}
}
