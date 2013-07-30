using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Projects;

namespace CSharp.Crawlers
{
    public interface IReferenceCache
    {
        long AddClass(Class cls);
        long AddEnum(EnumType enu);
        long AddField(Field field);
        long AddFile(CSharp.Projects.FileRef file);
        long AddInterface(Interface iface);
        long AddMethod(Method method);
        long AddNamespace(Namespce ns);
        long AddProject(CSharp.Projects.Project project);
        long AddStruct(Struct str);
        long AddUsing(Using usng);
        long AddUsingAlias(UsingAlias usng);
        long AddVariable(Variable variable);

        ICodeReference CodeRefFromID(long id);
        ICodeReference CodeRefFromID<T>(long id) where T : ICodeReference;
        ISourceItem ContainerFromID(long id);
        ISourceItem ContainerFromID<T>(long id) where T : ISourceItem;
    }

    public class ReferenceCache : IReferenceCache
    {
        private object _autoIncrementLock = new object();
        private long _autoIncrement = 0;
        private Dictionary<long, Type> _all = new Dictionary<long ,Type>();
        private Dictionary<long, Project> _projects = new Dictionary<long, Project>();
        private Dictionary<long, Using> _usings = new Dictionary<long, Using>();
        private Dictionary<long, UsingAlias> _usingAliases = new Dictionary<long, UsingAlias>();
        private Dictionary<long, FileRef> _files = new Dictionary<long, FileRef>();
        private Dictionary<long, Namespce> _namespaces = new Dictionary<long, Namespce>();
        private Dictionary<long, Class> _classes = new Dictionary<long, Class>();
        private Dictionary<long, Interface> _interfaces = new Dictionary<long, Interface>();
        private Dictionary<long, Struct> _structs = new Dictionary<long, Struct>();
        private Dictionary<long, EnumType> _enums = new Dictionary<long, EnumType>();
        private Dictionary<long, Field> _fields = new Dictionary<long, Field>();
        private Dictionary<long, Method> _methods = new Dictionary<long, Method>();
        private Dictionary<long, Variable> _variables = new Dictionary<long, Variable>();

        private Dictionary<string, long> _signatures = new Dictionary<string,long>();
        private Dictionary<string, long> _containerSignatures = new Dictionary<string, long>();

        public long AddUsing(Using usng) {
            return add(usng, _usings);
        }

        public long AddUsingAlias(UsingAlias usng) {
            return add(usng, _usingAliases);
        }

        public long AddProject(Project project) {
            return addContainer(project, _projects);
        }

        public long AddFile(FileRef file) {
            return addContainer(file, _files);
        }

        public long AddNamespace(Namespce ns) {
            return add(ns, _namespaces);
        }

        public long AddClass(Class cls) {
            return add(cls, _classes);
        }

        public long AddInterface(Interface iface) {
            return add(iface, _interfaces);
        }

        public long AddStruct(Struct str) {
            return add(str, _structs);
        }

        public long AddEnum(EnumType enu) {
            return add(enu, _enums);
        }

        public long AddField(Field field) {
            return add(field, _fields);
        }

        public long AddMethod(Method method) {
            return add(method, _methods);
        }

        public long AddVariable(Variable variable) {
            return add(variable, _variables);
        }

        public ICodeReference CodeRefFromID(long id) {
            Type type;
            if (!_all.TryGetValue(id, out type))
                return null;
            return codeRefFromID(type, id);
        }

        public ICodeReference CodeRefFromID<T>(long id) where T : ICodeReference {
            return codeRefFromID(typeof(T), id);
        }

        public ISourceItem ContainerFromID(long id) {
            Type type;
            if (!_all.TryGetValue(id, out type))
                return null;
            return containerFromID(type, id);
        }

        public ISourceItem ContainerFromID<T>(long id) where T : ISourceItem {
            return containerFromID(typeof(T), id);
        }

        private ISourceItem containerFromID(Type type, long id)
        {
            if (type == typeof(Project))
                return containerFromID(id, _projects);
            else if (type == typeof(FileRef))
                return containerFromID(id, _files);
            return null;
        }

        private ISourceItem containerFromID<T>(long id, Dictionary<long, T> list) where T : ISourceItem {
            T value;
            if (list.TryGetValue(id, out value))
                return value;
            return null;
        }

        private ICodeReference codeRefFromID(Type type, long id) {
            if (type == typeof(Using))
                return codeRefFromID(id, _usings);
            else if (type == typeof(UsingAlias))
                return codeRefFromID(id, _usingAliases);
            else if (type == typeof(Namespce))
                return codeRefFromID(id, _namespaces);
            else if (type == typeof(Class))
                return codeRefFromID(id, _classes);
            else if (type == typeof(Interface))
                return codeRefFromID(id, _interfaces);
            else if (type == typeof(Struct))
                return codeRefFromID(id, _structs);
            else if (type == typeof(EnumType))
                return codeRefFromID(id, _enums);
            else if (type == typeof(Field))
                return codeRefFromID(id, _fields);
            else if (type == typeof(Method))
                return codeRefFromID(id, _methods);
            else if (type == typeof(Variable))
                return codeRefFromID(id, _variables);
            return null;
        }

        private ICodeReference codeRefFromID<T>(long id, Dictionary<long, T> list) where T : ICodeReference {
            T value;
            if (list.TryGetValue(id, out value))
                return value;
            return null;
        }

        private long addContainer<T>(T item, Dictionary<long, T> list) where T : ISourceItem
        {
            var signature = item.Signature;
            if (!_containerSignatures.ContainsKey(item.Signature))
            {
                var id = setID(item);
                list.Add(id, item);
                _all.Add(id, typeof(T));
                _containerSignatures.Add(signature, id);
                return id;
            }
            long value;
            _containerSignatures.TryGetValue(signature, out value);
            return value;
        }

        private long add<T>(T item, Dictionary<long, T> list) where T : ICodeReference {
            var signature = item.Signature;
            if (!_signatures.ContainsKey(item.Signature)) {
                var id = setID(item);
                list.Add(id, item);
                _all.Add(id, typeof(T));
                _signatures.Add(signature, id);
                return id;
            }
            long value;
            _signatures.TryGetValue(signature, out value);
            return value;
        }

        private long setID(ISourceItem instance) {
            lock (_autoIncrementLock) {
                _autoIncrement++;
                instance.SetID(_autoIncrement);
                return _autoIncrement;
            }
        }
    }
}
