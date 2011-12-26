using System;
using System.Collections.Generic;
namespace OpenIDENet.CodeEngine.Core.Caching
{
	public interface ICacheBuilder
	{
		int ProjectCount { get; }
		int FileCount { get; }
		int CodeReferences { get; }
		
		bool ProjectExists(Project project);
		Project GetProject(string fullpath);
		
		bool FileExists(string file);
		void Invalidate(string file);
		
		void Add(Project project);
		void Add(ProjectFile file);
		void Add(ICodeReference reference);
		void Add(IEnumerable<ICodeReference> references);
	}
}

