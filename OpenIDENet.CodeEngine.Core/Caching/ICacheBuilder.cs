using System;
using System.Collections.Generic;
namespace OpenIDENet.CodeEngine.Core.Caching
{
	public interface ICacheBuilder
	{
		void AddNamespace(Namespace ns);
		void AddNamespaces(IEnumerable<Namespace> namespaces);
		void AddClass(Class cls);
		void AddClasss(IEnumerable<Class> classes);
	}
}

