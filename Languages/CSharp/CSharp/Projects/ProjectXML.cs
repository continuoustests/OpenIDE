using System;
using System.Xml;
using System.Xml.Schema;

namespace CSharp.Projects
{
	public class ProjectXML
	{
		protected XmlDocument _document = new XmlDocument();
		protected XmlNamespaceManager _nsManager = null;
		
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
				Console.WriteLine("Invalid project file {0}", xml);
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
