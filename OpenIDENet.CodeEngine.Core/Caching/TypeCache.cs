using System;
using System.Collections.Generic;
using System.Linq;
namespace OpenIDENet.CodeEngine.Core.Caching
{
	public class TypeCache : ICacheBuilder, ITypeCache
	{
		private List<Namespace> _namespaces = new List<Namespace>();
		private List<Class> _classes = new List<Class>();

		public List<ICodeType> Find(string name)
		{
			return _classes.Where(x => x.Name.ToLower().Contains(name.ToLower())).OrderBy(x => nameSort(x.Name, name)).Cast<ICodeType>().ToList();
		}
		
		public void AddNamespace (Namespace ns)
		{
			_namespaces.Add(ns);
		}

		public void AddNamespaces (IEnumerable<Namespace> namespaces)
		{
			_namespaces.AddRange(namespaces);
		}

		public void AddClass (Class cls)
		{
			_classes.Add(cls);
		}

		public void AddClasss (IEnumerable<Class> classes)
		{
			_classes.AddRange(classes);
		}
		
		private int nameSort(string name, string compareString)
		{
			if (name.Equals(compareString))
				return 1;
			if (name.ToLower().Equals(compareString.ToLower()))
				return 2;
			if (name.StartsWith(compareString))
				return 3;
			if (name.ToLower().StartsWith(compareString.ToLower()))
				return 4;
			if (name.EndsWith(compareString))
				return 5;
			if (name.ToLower().EndsWith(compareString.ToLower()))
				return 6;
			if (name.Contains(compareString))
				return 7;
			return 8;
		}
	}
}

