using System;
using System.Collections.Generic;
namespace OpenIDENet.Projects
{
	public class Project : IProject
	{
		private List<IFile> _files = new List<IFile>();
		
		public string Name { get; private set; }
		public IEnumerable<IFile> Files { get { return _files; } }
	}
}