using System;
using System.Collections.Generic;
using OpenIDENet.CodeEngine.Core.Caching.Search;
namespace OpenIDENet.CodeEngine.Core.Caching
{
	public interface ITypeCache
	{
		int ProjectCount { get; }
		int FileCount { get; }
		int CodeReferences { get; }
		
		List<ICodeReference> Find(string name);
        List<FileFindResult> FindFiles(string searchString);
        List<FileFindResult> GetFilesInDirectory(string directory);
        List<FileFindResult> GetFilesInProject(string project);
        List<FileFindResult> GetFilesInProject(string project, string path);
    }
}

