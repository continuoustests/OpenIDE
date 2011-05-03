using System;
using System.Collections.Generic;
using System.Linq;
namespace OpenIDENet.CodeEngine.Core.Caching
{
	public class TypeCache : ICacheBuilder, ITypeCache
	{
		private List<Project> _projects = new List<Project>();
		private List<string> _files = new List<string>();
		private List<Namespace> _namespaces = new List<Namespace>();
		private List<Class> _classes = new List<Class>();
		private List<Interface> _interfaces = new List<Interface>();
		private List<Struct> _structs = new List<Struct>();
		private List<EnumType> _enums = new List<EnumType>();

		public int ProjectCount { get { return _projects.Count; } }
		public int FileCount { get { return _files.Count; } }
		public int NamespaceCount { get { return _namespaces.Count; } }
		public int TypeCount { get { return _classes.Count + _interfaces.Count + _structs.Count + _enums.Count; } }
		
		public List<ICodeType> Find(string name)
		{
			var results = new List<ICodeType>();
			lock (_classes) { results.AddRange(_classes.Where(x => x.Name.ToLower().Contains(name.ToLower())).Cast<ICodeType>()); }
			lock (_interfaces) { results.AddRange(_interfaces.Where(x => x.Name.ToLower().Contains(name.ToLower())).Cast<ICodeType>()); }
			lock (_structs) { results.AddRange(_structs.Where(x => x.Name.ToLower().Contains(name.ToLower())).Cast<ICodeType>()); }
			lock (_enums) { results.AddRange(_enums.Where(x => x.Name.ToLower().Contains(name.ToLower())).Cast<ICodeType>()); }
			return results.OrderBy(x => nameSort(x.Name, name)).ToList();
		}
		
		public bool ProjectExists(Project project)
		{
			return _projects.Exists(x => x.Fullpath.Equals(project.Fullpath));
		}
		
		public void AddProject(Project project)
		{
			lock (_projects)
			{
				_projects.Add(project);
			}
		}
		
		public Project GetProject(string fullpath)
		{
			lock (_projects)
			{
				return _projects.FirstOrDefault(x => x.Fullpath.Equals(fullpath));
			}
		}
		
		public bool FileExists(string file)
		{
			return _files.Contains(file);
		}
		
		public void Invalidate(string file)
		{
			lock (_files) {
				lock (_namespaces) {
					lock (_classes) {
						lock (_interfaces) {
							lock (_structs) {
								lock (_enums) {
									_files.Remove(file);
									_namespaces.RemoveAll(x => x.Fullpath.Equals(file));
									_classes.RemoveAll(x => x.Fullpath.Equals(file));
									_interfaces.RemoveAll(x => x.Fullpath.Equals(file));
									_structs.RemoveAll(x => x.Fullpath.Equals(file));
									_enums.RemoveAll(x => x.Fullpath.Equals(file));
								}
							}
						}
					}
				}
			}
		}
		
		public void AddFile(string file)
		{
			lock (_files)
				_files.Add(file);
		}
		
		public void AddNamespace (Namespace ns)
		{
			lock (_namespaces)
				_namespaces.Add(ns);
		}

		public void AddNamespaces (IEnumerable<Namespace> namespaces)
		{
			lock (_namespaces)
				_namespaces.AddRange(namespaces);
		}

		public void AddClass (Class cls)
		{
			lock (_classes)
				_classes.Add(cls);
		}

		public void AddClasses (IEnumerable<Class> classes)
		{
			lock (_classes)
				_classes.AddRange(classes);
		}
		
		public void AddInterface(Interface iface)
		{
			lock (_interfaces)
				_interfaces.Add(iface);
		}
		
		public void AddInterfaces(IEnumerable<Interface> interfaces)
		{
			lock (_interfaces)
				_interfaces.AddRange(interfaces);
		}
		
		public void AddStruct(Struct str)
		{
			lock (_structs)
				_structs.Add(str);
		}
			
		public void AddStructs(IEnumerable<Struct> structs)
		{
			lock (_structs)
				_structs.AddRange(structs);
		}
		
		public void AddEnum(EnumType enu)
		{
			lock (_enums)
				_enums.Add(enu);
		}
		
		public void AddEnums(IEnumerable<EnumType> enums)
		{
			lock (_enums)
				_enums.AddRange(enums);
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

