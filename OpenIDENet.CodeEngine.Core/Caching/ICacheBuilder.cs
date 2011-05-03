using System;
using System.Collections.Generic;
namespace OpenIDENet.CodeEngine.Core.Caching
{
	public interface ICacheBuilder
	{
		int ProjectCount { get; }
		int FileCount { get; }
		int NamespaceCount { get; }
		int TypeCount { get; }
		
		bool ProjectExists(Project project);
		void AddProject(Project project);
		Project GetProject(string fullpath);
		
		bool FileExists(string file);
		void Invalidate(string file);
		void AddFile(string file);
		
		void AddNamespace(Namespace ns);
		void AddNamespaces(IEnumerable<Namespace> namespaces);
		
		void AddClass(Class cls);
		void AddClasses(IEnumerable<Class> classes);
		
		void AddInterface(Interface iface);
		void AddInterfaces(IEnumerable<Interface> interfaces);
		
		void AddStruct(Struct str);
		void AddStructs(IEnumerable<Struct> structs);
		
		void AddEnum(EnumType enu);
		void AddEnums(IEnumerable<EnumType> enums);
	}
}

