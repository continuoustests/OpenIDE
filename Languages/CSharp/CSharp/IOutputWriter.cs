using System;
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
            Usings.Add(usng);
            writeSignature("using", usng);
        }

        public void WriteProject(Project project)
		{
            Projects.Add(project);
            var json = "";
            if (project.JSON != null)
                json = project.JSON;
			Console.WriteLine("project|" + project.File + "|" + project.JSON + "|filesearch");
		}

        public void WriteFile(FileRef file)
		{
            Files.Add(file);
			Console.WriteLine("file|" + file.File + "|filesearch");
		}

        public void WriteNamespace(Namespce ns)
		{
            Namespaces.Add(ns);
			writeSignature("namespace", ns);
		}

        public void WriteClass(Class cls)
		{
            Classes.Add(cls);
			writeSignature("class", cls, new[] { "typesearch" });
		}

        public void WriteInterface(Interface iface)
		{
            Interfaces.Add(iface);
			writeSignature("interface", iface, new[] { "typesearch" });
		}

        public void WriteStruct(Struct str)
		{
            Structs.Add(str);
			writeSignature("struct", str, new[] { "typesearch" });
		}

        public void WriteEnum(EnumType enu)
		{
            Enums.Add(enu);
			writeSignature("enum", enu, new[] { "typesearch" });
		}

        public void WriteMethod(Method method)
        {
            Methods.Add(method);
            writeSignature("method", method);
        }

        public void WriteField(Field field)
        {
            Fields.Add(field);
            writeSignature("field", field);
        }

        public void WriteError(string description)
		{
			Console.WriteLine("error|" + description);
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
