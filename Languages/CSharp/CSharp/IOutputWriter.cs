using System;
using System.Linq;
using CSharp.Projects;
using CSharp.Crawlers;
using System.Collections.Generic;
using CSharp.Responses;

namespace CSharp
{
	public interface IOutputWriter : IDisposable
	{
        List<Project> Projects { get; }
        List<Using> Usings { get; }
        List<UsingAlias> UsingAliases { get; }
        List<FileRef> Files { get; }
        List<Namespce> Namespaces { get; }
        List<Class> Classes { get; }
        List<Interface> Interfaces { get; }
        List<Struct> Structs { get; }
        List<EnumType> Enums { get; }
        List<Field> Fields { get; }
        List<Method> Methods { get; }
        List<Parameter> Parameters { get; }
        List<Variable> Variables { get; }

        void SetTypeVisibility(bool visibility);
        void WriteUsing(Using usng);
        void WriteUsingAlias(UsingAlias usng);
        void WriteProject(Project project);
        void WriteFile(FileRef file);
        void WriteNamespace(Namespce ns);
        void WriteClass(Class cls);
        void WriteInterface(Interface iface);
        void WriteStruct(Struct str);
        void WriteEnum(EnumType enu);
        void WriteField(Field field);
        void WriteMethod(Method method);
        void WriteVariable(Variable variable);
        void WriteError(string description);

        void BuildTypeIndex();
        bool ContainsType(string fullname);
        string FirstMatchingTypeFromName(string name);
        string VariableTypeFromSignature(string signature);
        string StaticMemberFromSignature(string signature);
        FilePosition PositionFromSignature(string signature);
        
        IEnumerable<string> CollectBases(string type);

        void MergeWith(IOutputWriter cache);
	}

    public class NullOutputWriter : IOutputWriter
	{
        public List<Project> Projects { get { return new List<Project>(); } }
        public List<Using> Usings { get { return new List<Using>(); } }
        public List<UsingAlias> UsingAliases { get { return new List<UsingAlias>(); } }
        public List<FileRef> Files { get { return new List<FileRef>(); } }
        public List<Namespce> Namespaces { get { return new List<Namespce>(); } }
        public List<Class> Classes { get { return new List<Class>(); } }
        public List<Interface> Interfaces { get { return new List<Interface>(); } }
        public List<Struct> Structs { get { return new List<Struct>(); } }
        public List<EnumType> Enums { get { return new List<EnumType>(); } }
        public List<Field> Fields { get { return new List<Field>(); } }
        public List<Method> Methods { get { return new List<Method>(); } }
        public List<Parameter> Parameters { get { return new List<Parameter>(); } }
        public List<Variable> Variables { get { return new List<Variable>(); } }


        public void SetTypeVisibility(bool visibility) {}
        public void WriteUsing(Using usng) { }
        public void WriteUsingAlias(UsingAlias usng) { }
        public void WriteProject(Project project) { }
        public void WriteFile(FileRef file) { }
        public void WriteNamespace(Namespce ns) { }
        public void WriteClass(Class cls) { }
        public void WriteInterface(Interface iface) { }
        public void WriteStruct(Struct str) { }
        public void WriteEnum(EnumType enu) { }
        public void WriteField(Field field) { }
        public void WriteMethod(Method method) { }
        public void WriteVariable(Variable variable) { }
        public void WriteError(string description) { }
        public void BuildTypeIndex() { }
        public bool ContainsType(string fullname) { return false; }
        public string FirstMatchingTypeFromName(string name) { return null; }
        public string VariableTypeFromSignature(string signature) { return null; }
        public string StaticMemberFromSignature(string signature) { return null; }
        public IEnumerable<string> CollectBases(string type) { return new string[] {}; }
        public FilePosition PositionFromSignature(string signature) { return null; }
        public void MergeWith(IOutputWriter cache) { }
        public void Dispose() { }
    }

	public class OutputWriter : IOutputWriter
	{
        private bool _visibility = true;
        private IResponseWriter _writer;

        public List<Project> Projects { get; private set; }
        public List<Using> Usings { get; private set; }
        public List<UsingAlias> UsingAliases { get; private set; }
        public List<FileRef> Files { get; private set; }
        public List<Namespce> Namespaces { get; private set; }
        public List<Class> Classes { get; private set; }
        public List<Interface> Interfaces { get; private set; }
        public List<Struct> Structs { get; private set; }
        public List<EnumType> Enums { get; private set; }
        public List<Field> Fields { get; private set; }
        public List<Method> Methods { get; private set; }
        public List<Parameter> Parameters { get; private set; }
        public List<Variable> Variables { get; private set; }

        public OutputWriter(IResponseWriter writer) {
            _writer = writer;
            Projects = new List<Project>();
            Usings = new List<Using>();
            UsingAliases = new List<UsingAlias>();
            Files = new List<FileRef>();
            Namespaces = new List<Namespce>();
            Classes = new List<Class>();
            Interfaces = new List<Interface>();
            Structs = new List<Struct>();
            Enums = new List<EnumType>();
            Fields = new List<Field>();
            Methods = new List<Method>();
            Parameters = new List<Parameter>();
            Variables = new List<Variable>();
        }

        public void SetTypeVisibility(bool visibility) {
            _visibility = visibility;
        }

        public void WriteProject(Project project)
		{
            Projects.Add(project);
            if (_visibility)
                _writer.Write("project|" + project.File + "||filesearch");
		}

        public void WriteFile(FileRef file)
		{
            Files.Add(file);
            if (_visibility)
                _writer.Write("file|" + file.File + "|filesearch");
		}

        public void WriteUsing(Using usng)
        {
            usng.AllTypesAreResolved = !_visibility;
            Usings.Add(usng);
        }

        public void WriteUsingAlias(UsingAlias alias)
        {
            alias.AllTypesAreResolved = !_visibility;
            UsingAliases.Add(alias);
        }

        public void WriteNamespace(Namespce ns)
		{
            ns.AllTypesAreResolved = !_visibility;
            Namespaces.Add(ns);
		}

        public void WriteClass(Class cls)
		{
            cls.AllTypesAreResolved = !_visibility;
            Classes.Add(cls);
            if (_visibility)
                writeSignature("class", cls, new[] { "typesearch" });
		}

        public void WriteInterface(Interface iface)
		{
            iface.AllTypesAreResolved = !_visibility;
            Interfaces.Add(iface);
            if (_visibility)
                writeSignature("interface", iface, new[] { "typesearch" });
		}

        public void WriteStruct(Struct str)
		{
            str.AllTypesAreResolved = !_visibility;
            Structs.Add(str);
            if (_visibility)
                writeSignature("struct", str, new[] { "typesearch" });
		}

        public void WriteEnum(EnumType enu)
		{
            enu.AllTypesAreResolved = !_visibility;
            Enums.Add(enu);
            if (_visibility)
                writeSignature("enum", enu, new[] { "typesearch" });
		}

        public void WriteMethod(Method method)
        {
            method.AllTypesAreResolved = !_visibility;
            Methods.Add(method);
            method.Parameters.ToList()
                .ForEach(x => {
                    x.AllTypesAreResolved = !_visibility;
                    Parameters.Add(x);
                });
        }

        public void WriteField(Field field)
        {
            field.AllTypesAreResolved = !_visibility;
            Fields.Add(field);
        }

        public void WriteVariable(Variable variable)
        {
            variable.AllTypesAreResolved = !_visibility;
            Variables.Add(variable);
        }

        public void WriteError(string description)
		{
			_writer.Write("error|" + description);
		}

        private Dictionary<string,string> _declarations = new Dictionary<string,string>();
        private Dictionary<string,string> _staticDeclarations = new Dictionary<string,string>();
        private HashSet<string> _typeIndex = new HashSet<string>();
        private Dictionary<string, string> _nameIndex = new Dictionary<string,string>();
        private Dictionary<string, FilePosition> _signaturePositions = new Dictionary<string, FilePosition>();
        public void BuildTypeIndex() {
            _typeIndex = new HashSet<string>();
            _nameIndex = new Dictionary<string,string>();
            _signaturePositions = new Dictionary<string, FilePosition>();
            Classes.ForEach(x => {
                addNameAndTypeIndex(x);
                addSignaturePosition(x);
            });
            Interfaces.ForEach(x => {
                addNameAndTypeIndex(x);
                addSignaturePosition(x);
            });
            Structs.ForEach(x => {
                addNameAndTypeIndex(x);
                addSignaturePosition(x);
            });
            Enums.ForEach(x => {
                addNameAndTypeIndex(x);
                addSignaturePosition(x);
            });

            _declarations = new Dictionary<string,string>();
            _staticDeclarations = new Dictionary<string, string>();
            Parameters.ForEach(x => {
                    var signature = x.Parent + "." + x.Name;
                    addDeclaration(signature, x.DeclaringType, x.IsStatic);
                    addSignaturePosition(x);
                });
            Variables.ForEach(x => {
                    var signature = x.Parent + "." + x.Name;
                    addDeclaration(signature, x.DeclaringType, x.IsStatic);
                    addSignaturePosition(x);
                });
            Methods.ForEach(x => {
                var signature = x.GenerateNameSignature();
                addDeclaration(signature, x.ReturnType, x.IsStatic);
                addSignaturePosition(x);
            });
            Fields.ForEach(x => {
                var signature = x.Parent + "." + x.Name;
                addDeclaration(signature, x.ReturnType, x.IsStatic);
                addSignaturePosition(x);
            });
        }

        private void addNameAndTypeIndex(ICodeReference x) {
            var signature = x.Parent + "." + x.Name;
            _typeIndex.Add(signature);
            if (!_nameIndex.ContainsKey(x.Name))
                _nameIndex.Add(x.Name, signature);
        }

        private void addSignaturePosition(ICodeReference x) {
            var fullSignature = x.Signature;
            if (!_signaturePositions.ContainsKey(fullSignature))
                _signaturePositions.Add(fullSignature, new FilePosition(x.File.File, x.Line, x.Column));
        }

        private void addDeclaration(string key, string value, bool isStatic) {
            if (isStatic) {
                if (!_staticDeclarations.ContainsKey(key))
                    _staticDeclarations.Add(key, value);
            } else {
                if (!_declarations.ContainsKey(key))
                    _declarations.Add(key, value);
            }
        }

        public bool ContainsType(string fullname) {
            return _typeIndex.Contains(fullname);
        }

        public string FirstMatchingTypeFromName(string name) {
            string signature;
            if (_nameIndex.TryGetValue(name, out signature))
                return signature;
            return null;
        }

        public string VariableTypeFromSignature(string signature) {
            string type;
            if (_declarations.TryGetValue(signature, out type))
                return type;
            return null;
        }

        public string StaticMemberFromSignature(string signature) {
            string type;
            if (_staticDeclarations.TryGetValue(signature, out type))
                return type;
            return null;
        }

        public IEnumerable<string> CollectBases(string type) {
            var bases = new List<string>();
            bases.Add("System.Object");
            IType current = Interfaces.FirstOrDefault(x => x.Signature == type);
            if (current == null)
                current = Classes.FirstOrDefault(x => x.Signature == type);
            if (current == null)
                current = Structs.FirstOrDefault(x => x.Signature == type);
            if (current == null)
                current = Enums.FirstOrDefault(x => x.Signature == type);
            if (current == null)
                return new string[]{};
            foreach (var baseSignature in current.BaseTypes) {
                bases.Add(baseSignature);
                bases.AddRange(CollectBases(baseSignature));
            }
            return bases;
        }

        public FilePosition PositionFromSignature(string signature) {
            FilePosition position;
            if (_signaturePositions.TryGetValue(signature, out position))
                return position;
            return null;
        }

		private void writeSignature(string type, ICodeReference coderef)
		{
			writeSignature(type, coderef, new string[] {});
		}

		private void writeSignature(string type, ICodeReference coderef, string[] additional)
		{
            var json = "";
            if (coderef.JSON != null)
                json = coderef.JSON;
			var additionalArguments = "";
            if (_visibility == true) {
			    foreach (var argument in additional)
				    additionalArguments += "|" + argument;
            }
			_writer.Write("signature|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}{8}",
                coderef.Parent,
				coderef.Signature,
				coderef.Name,
				type,
                coderef.Scope,
				coderef.Line,
				coderef.Column,
                json,
				additionalArguments);
		}

        public void MergeWith(IOutputWriter cache)
        {
            // Remove all crawled items
            cache
                .Projects.ForEach(x => {
                    Projects.RemoveAll(y => y.File == x.File);
                    Files.RemoveAll(y => y.Project != null && y.Project.File == x.File);
                });
            cache
                .Files.ForEach(x => {
                    Files.RemoveAll(y => y.File == x.File);
                    Usings.RemoveAll(y => y.File.File == x.File);
                    UsingAliases.RemoveAll(y => y.File.File == x.File);
                    Namespaces.RemoveAll(y => y.File.File == x.File);
                    Classes.RemoveAll(y => y.File.File == x.File);
                    Interfaces.RemoveAll(y => y.File.File == x.File);
                    Structs.RemoveAll(y => y.File.File == x.File);
                    Enums.RemoveAll(y => y.File.File == x.File);
                    Fields.RemoveAll(y => y.File.File == x.File);
                    Methods.RemoveAll(y => y.File.File == x.File);
                    Parameters.RemoveAll(y => y.File.File == x.File);
                    Variables.RemoveAll(y => y.File.File == x.File);
                });

            // Write new items
            Projects.AddRange(cache.Projects);
            Files.AddRange(cache.Files);
            Usings.AddRange(cache.Usings);
            UsingAliases.AddRange(cache.UsingAliases);
            Namespaces.AddRange(cache.Namespaces);
            Classes.AddRange(cache.Classes);
            Interfaces.AddRange(cache.Interfaces);
            Structs.AddRange(cache.Structs);
            Enums.AddRange(cache.Enums);
            Fields.AddRange(cache.Fields);
            Methods.AddRange(cache.Methods);
            Parameters.AddRange(cache.Parameters);
            Variables.AddRange(cache.Variables);

            BuildTypeIndex();
        }

        public void Dispose()
        {
            Usings = null;
            UsingAliases = null;
            Namespaces = null;
            Classes = null;
            Interfaces = null;
            Structs = null;
            Enums = null;
            Fields = null;
            Methods = null;
            Parameters = null;
            Variables = null;
            _declarations = null;
            _staticDeclarations = null;
            _typeIndex = null;
            _nameIndex = null;
        }
    }
}
