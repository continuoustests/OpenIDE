using System;
using System.Collections.Generic;
using System.Text;
using CSharp.Crawlers.TypeResolvers;
using CSharp.Projects;
namespace CSharp.Crawlers
{
	public class Class : TypeBase<Class>, ICodeReference
	{
        public bool AllTypesAreResolved { get; private set; }

		public string Type { get; private set; }
        public FileRef File { get; private set; }
		public string Signature { get { return string.Format("{0}.{1}", Namespace, Name); } }
		public string Namespace { get; private set; }
		public string Name { get; private set; }
        public string Scope { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }

        public Class(FileRef file, string ns, string name, string scope, int line, int column)
		{
            setThis(this);
			File = file;
			Namespace = ns;
			Name = name;
            Scope = scope;
			Line = line;
			Column = column;
		}

        public string GenerateFullSignature() {
            return null;
        }

        public void ResolveTypes(ICacheReader cache) {
            throw new NotImplementedException();
        }
    }

    public class Field : ICodeReference 
	{
        public bool AllTypesAreResolved { get; private set; }

		public string Type { get; private set; }
        public FileRef File { get; private set; }
		public string Signature { get; private set; }
		public string Namespace { get; private set; }
		public string Name { get; private set; }
        public string Scope { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
        public string JSON { get; private set; }

        public Field(FileRef file, string ns, string name, string scope, int line, int column, string returnType, string json)
		{
			File = file;
			Namespace = ns;
			Name = name;
            Signature = string.Format("{0} {1}.{2}",
                returnType,
                Namespace,
                Name);
            Scope = scope;
			Line = line;
			Column = column;
            JSON = json;
		}

        public string GenerateFullSignature() {
            return null;
        }

        public void ResolveTypes(ICacheReader cache) {
            throw new NotImplementedException();
        }
    }

    public class Method : ICodeReference 
	{
        public bool AllTypesAreResolved { get; private set; }

		public string Type { get; private set; }
        public FileRef File { get; private set; }
		public string Signature { get; private set; }
		public string Namespace { get; private set; }
		public string Name { get; private set; }
        public string Scope { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
        public string JSON { get; private set; }

        public Method(FileRef file, string ns, string name, string scope, int line, int column, string returnType, IEnumerable<Parameter> parameters, JSONWriter writer)
		{
            var paramString = getParamString(parameters, writer);
			File = file;
			Namespace = ns;
			Name = name;
            Signature = string.Format("{0} {1}.{2}({3})",
                returnType,
                Namespace,
                Name,
                paramString);
            Scope = scope;
			Line = line;
			Column = column;
            JSON = writer.ToString();
		}

        public string GenerateFullSignature() {
            return null;
        }

        public void ResolveTypes(ICacheReader cache) {
            throw new NotImplementedException();
        }

        private string getParamString(IEnumerable<Parameter> parameters, JSONWriter writer)
        {
            var json = new JSONWriter();
            var sb = new StringBuilder();
            foreach (var param in parameters) {
                json.Append(param.Name, param.Type);
                if (sb.Length == 0)
                    sb.Append(param.Type);
                else
                    sb.Append("," + param.Type);
            }
            if (json.ToString().Length > 0)
                writer.AppendSection("parameters", json);
            return sb.ToString();
        }
    }

    public class Parameter
    {
        public string Type { get; private set; }
        public string Name { get; private set; }

        public Parameter(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}

