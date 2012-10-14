using System;
using System.Linq;
using CSharp.Projects;
using CSharp.Crawlers;
using System.Collections.Generic;

namespace CSharp
{
	public interface IOutputWriter
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
        
        void WriteToOutput();
	}

	public class OutputWriter : IOutputWriter
	{
        private bool _visibility = true;

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

        public OutputWriter() {
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
		}

        public void WriteFile(FileRef file)
		{
            Files.Add(file);
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
		}

        public void WriteInterface(Interface iface)
		{
            iface.AllTypesAreResolved = !_visibility;
            Interfaces.Add(iface);
		}

        public void WriteStruct(Struct str)
		{
            str.AllTypesAreResolved = !_visibility;
            Structs.Add(str);
		}

        public void WriteEnum(EnumType enu)
		{
            enu.AllTypesAreResolved = !_visibility;
            Enums.Add(enu);
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
			Console.WriteLine("error|" + description);
		}

        private Dictionary<string,string> _declarations;
        private HashSet<string> _typeIndex;
        private Dictionary<string, string> _nameIndex;
        public void BuildTypeIndex() {
            _typeIndex = new HashSet<string>();
            _nameIndex = new Dictionary<string,string>();
            Classes.ForEach(x => {
                var signature = x.Namespace + "." + x.Name;
                _typeIndex.Add(signature);
                if (!_nameIndex.ContainsKey(x.Name))
                    _nameIndex.Add(x.Name, signature);
            });
            Interfaces.ForEach(x => {
                var signature = x.Namespace + "." + x.Name;
                _typeIndex.Add(signature);
                if (!_nameIndex.ContainsKey(x.Name))
                    _nameIndex.Add(x.Name, signature);
            });
            Structs.ForEach(x => {
                var signature = x.Namespace + "." + x.Name;
                _typeIndex.Add(signature);
                if (!_nameIndex.ContainsKey(x.Name))
                    _nameIndex.Add(x.Name, signature);
            });
            Enums.ForEach(x => {
                var signature = x.Namespace + "." + x.Name;
                _typeIndex.Add(signature);
                if (!_nameIndex.ContainsKey(x.Name))
                    _nameIndex.Add(x.Name, signature);
            });

            _declarations = new Dictionary<string,string>();
            Parameters.ForEach(x => {
                    var signature = x.Namespace + "." + x.Name;
                    _declarations.Add(signature, x.DeclaringType);
                });
            Variables.ForEach(x => {
                    var signature = x.Namespace + "." + x.Name;
                    _declarations.Add(signature, x.DeclaringType);
                });
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

        public void WriteToOutput()
        {
            Projects
                .ForEach(proj => {
                    var json = "";
                    if (proj.JSON != null)
                        json = proj.JSON;
			        Console.WriteLine("project|" + proj.File + "|" + json + "|filesearch");

                    Files.Where(file => file.Project != null && file.Project.File == proj.File).ToList()
                        .ForEach(file => handle(file));
                });

            Files
                .Where(file => file.Project == null).ToList()
                .ForEach(file => handle(file));
        }

        private void handle(FileRef file)
        {
            Console.WriteLine("file|" + file.File + "|filesearch");
            Usings.Where(x => x.File.File == file.File).ToList().ForEach(x => writeSignature("using", x));
            UsingAliases.Where(x => x.File.File == file.File).ToList().ForEach(x => writeSignature("alias", x));
            Namespaces.Where(x => x.File.File == file.File).ToList().ForEach(x => writeSignature("namespace", x));
            Classes.Where(x => x.File.File == file.File).ToList().ForEach(x => writeSignature("class", x, new[] { "typesearch" }));
            Interfaces.Where(x => x.File.File == file.File).ToList().ForEach(x => writeSignature("interface", x, new[] { "typesearch" }));
            Structs.Where(x => x.File.File == file.File).ToList().ForEach(x => writeSignature("struct", x, new[] { "typesearch" }));
            Enums.Where(x => x.File.File == file.File).ToList().ForEach(x => writeSignature("enum", x, new[] { "typesearch" }));
            Fields.Where(x => x.File.File == file.File).ToList().ForEach(x => writeSignature("field", x));
            Methods.Where(x => x.File.File == file.File).ToList().ForEach(x => writeSignature("method", x));
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
			Console.WriteLine("signature|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}{8}",
                coderef.Namespace,
				coderef.Signature,
				coderef.Name,
				type,
                coderef.Scope,
				coderef.Line,
				coderef.Column,
                json,
				additionalArguments);
		}
	}
}
