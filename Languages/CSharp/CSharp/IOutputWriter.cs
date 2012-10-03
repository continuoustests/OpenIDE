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
        List<FileRef> Files { get; }
        List<Namespce> Namespaces { get; }
        List<Class> Classes { get; }
        List<Interface> Interfaces { get; }
        List<Struct> Structs { get; }
        List<EnumType> Enums { get; }
        List<Method> Methods { get; }
        List<Field> Fields { get; }

        void SetTypeVisibility(bool visibility);
        void WriteUsing(Using usng);
        void WriteProject(Project project);
        void WriteFile(FileRef file);
        void WriteNamespace(Namespce ns);
        void WriteClass(Class cls);
        void WriteInterface(Interface iface);
        void WriteStruct(Struct str);
        void WriteEnum(EnumType enu);
        void WriteField(Field field);
        void WriteMethod(Method method);
        void WriteError(string description);

        void WriteToOutput();
	}

	public class OutputWriter : IOutputWriter
	{
        private bool _visibility = true;

        public List<Project> Projects { get; private set; }
        public List<Using> Usings { get; private set; }
        public List<FileRef> Files { get; private set; }
        public List<Namespce> Namespaces { get; private set; }
        public List<Class> Classes { get; private set; }
        public List<Interface> Interfaces { get; private set; }
        public List<Struct> Structs { get; private set; }
        public List<EnumType> Enums { get; private set; }
        public List<Method> Methods { get; private set; }
        public List<Field> Fields { get; private set; }

        public OutputWriter() {
            Projects = new List<Project>();
            Usings = new List<Using>();
            Files = new List<FileRef>();
            Namespaces = new List<Namespce>();
            Classes = new List<Class>();
            Interfaces = new List<Interface>();
            Structs = new List<Struct>();
            Enums = new List<EnumType>();
            Methods = new List<Method>();
            Fields = new List<Field>();
        }

        public void SetTypeVisibility(bool visibility) {
            _visibility = visibility;
        }

        public void WriteUsing(Using usng)
        {
            usng.AllTypesAreResolved = !_visibility;
            Usings.Add(usng);
        }

        public void WriteProject(Project project)
		{
            Projects.Add(project);
		}

        public void WriteFile(FileRef file)
		{
            Files.Add(file);
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
        }

        public void WriteField(Field field)
        {
            field.AllTypesAreResolved = !_visibility;
            Fields.Add(field);
        }

        public void WriteError(string description)
		{
			Console.WriteLine("error|" + description);
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
            Usings.ForEach(x => writeSignature("using", x));
            Namespaces.ForEach(x => writeSignature("namespace", x));
            Classes.ForEach(x => writeSignature("class", x, new[] { "typesearch" }));
            Interfaces.ForEach(x => writeSignature("interface", x, new[] { "typesearch" }));
            Structs.ForEach(x => writeSignature("struct", x, new[] { "typesearch" }));
            Enums.ForEach(x => writeSignature("enum", x, new[] { "typesearch" }));
            Fields.ForEach(x => writeSignature("field", x));
            Methods.ForEach(x => writeSignature("method", x));
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
