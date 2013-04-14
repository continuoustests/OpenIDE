using System;
using System.Collections.Generic;
using CSharp.Crawlers.TypeResolvers;
using CSharp.Projects;
namespace CSharp.Crawlers
{
	public class Interface : TypeBase<Interface>, ICodeReference 
	{
        public bool AllTypesAreResolved { get; set; }

		public string Type { get; private set; }
        public FileRef File { get; private set; }
		public string Signature { get { return string.Format("{0}.{1}", Parent, Name); } }
		public string Parent { get; private set; }
		public string Name { get; private set; }
        public string Scope { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }

        public Interface(FileRef file, string parent, string name, string scope, int line, int column)
		{
            setThis(this);
			File = file;
			Parent = parent;
			Name = name;
            Scope = scope;
			Line = line;
			Column = column;
		}

        public string ToFullSignature() {
            return Signature;
        }

        public string ToNamespaceSignature() {
            return Signature;
        }

        public IEnumerable<ResolveStatement> GetResolveStatements() {
            return getTypeResolveStatements();
        }

        protected override string getNamespace()
        {
            return Parent;
        }
    }
}

