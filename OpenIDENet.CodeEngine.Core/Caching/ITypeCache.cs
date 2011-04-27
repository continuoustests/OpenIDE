using System;
using System.Collections.Generic;
namespace OpenIDENet.CodeEngine.Core.Caching
{
	public interface ITypeCache
	{
		List<ICodeType> Find(string name);
	}
}

