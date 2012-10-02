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

    public class Field : CodeItemBase<Field>, ICodeReference 
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

        public Field(FileRef file, string ns, string name, string scope, int line, int column, string returnType)
		{
            setThis(this);
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
		}

        public string GenerateFullSignature() {
            return null;
        }

        public void ResolveTypes(ICacheReader cache) {
            throw new NotImplementedException();
        }
    }

    public class Method : CodeItemBase<Method>, ICodeReference 
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

        public Method(FileRef file, string ns, string name, string scope, int line, int column, string returnType, IEnumerable<Parameter> parameters)
		{
            var paramString = getParamString(parameters);
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
		}

        public string GenerateFullSignature() {
            return null;
        }

        public void ResolveTypes(ICacheReader cache) {
            throw new NotImplementedException();
        }

        private string getParamString(IEnumerable<Parameter> parameters)
        {
            var sb = new StringBuilder();
            foreach (var param in parameters) {
                if (sb.Length == 0)
                    sb.Append(param.Type);
                else
                    sb.Append("," + param.Type);
            }
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

