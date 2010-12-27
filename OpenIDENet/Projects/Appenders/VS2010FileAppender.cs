using System;
using OpenIDENet.Versioning;
using System.Xml;
namespace OpenIDENet.Projects.Appenders
{
	public class VS2010FileAppender : IAppendFiles<VS2010>
	{
		public void Append(IProject project, string file)
		{
			/*
			// Pull file relative path from project path
			var document = new XmlDocument();
			document.LoadXml(project.Xml);
			var node = document.SelectSingleNode("Project/ItemGroup/Compile");
			if (node == null)
				// Create item group for compile files
			else
				// check wether file exists already
				// if not add it
			*/
		}
	}
}

