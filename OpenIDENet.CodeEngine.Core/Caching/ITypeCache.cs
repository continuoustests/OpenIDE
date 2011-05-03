using System;
using System.Collections.Generic;
namespace OpenIDENet.CodeEngine.Core.Caching
{
	public interface ITypeCache
	{
		int ProjectCount { get; }
		int FileCount { get; }
		int NamespaceCount { get; }
		int TypeCount { get; }
		
		List<ICodeType> Find(string name);
	}
}

