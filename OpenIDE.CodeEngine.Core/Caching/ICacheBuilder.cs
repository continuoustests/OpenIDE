using System;
using System.Collections.Generic;
using OpenIDE.Core.Caching;
namespace OpenIDE.CodeEngine.Core.Caching
{
	public interface ICacheBuilder
	{
		List<CachedPlugin> Plugins { get; }
		int ProjectCount { get; }
		int FileCount { get; }
		int CodeReferences { get; }
		
		bool ProjectExists(Project project);
		Project GetProject(string fullpath);
		
		bool FileExists(string file);
		void Invalidate(string file);
	}

	public class CachedPlugin
	{
		public string Name { get; private set; }
		public List<string> Extensions { get; private set; }

		public CachedPlugin(string name, IEnumerable<string> extensions)
		{
			Name = name;
			Extensions = new List<string>(extensions);
		}
	}
}

