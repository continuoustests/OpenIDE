using System;
using CSharp.Projects;
using CSharp.Crawlers;

namespace CSharp
{
	public interface IOutputWriter
	{
		void AddProject(Project project);
		void AddFile(string file);
		void AddNamespace(Namespace ns);
		void AddClass(Class cls);
		void AddInterface(Interface iface);
		void AddStruct(Struct str);
		void AddEnum(EnumType enu);
		void Error(string description);
	}

	class OutputWriter : IOutputWriter
	{
		public void AddProject(Project project)
		{
			Console.WriteLine("project|" + project.File);
		}

		public void AddFile(string file)
		{
			Console.WriteLine("file|" + file);
		}
		
		public void AddNamespace(Namespace ns)
		{
			writeSignature("namespace", ns);
		}

		public void AddClass(Class cls)
		{
			writeSignature("class", cls);
		}

		public void AddInterface(Interface iface)
		{
			writeSignature("interface", iface);
		}

		public void AddStruct(Struct str)
		{
			writeSignature("struct", str);
		}

		public void AddEnum(EnumType enu)
		{
			writeSignature("enum", enu);
		}

		public void Error(string description)
		{
			Console.WriteLine("error|" + description);
		}

		private void writeSignature(string type, ICodeReference coderef)
		{
			Console.WriteLine("signature|{0}|{1}|{2}|{3}|{4}|{5}",
				coderef.Signature,
				coderef.Name,
				type,
				coderef.Offset,
				coderef.Line,
				coderef.Column);
		}
	}
}
