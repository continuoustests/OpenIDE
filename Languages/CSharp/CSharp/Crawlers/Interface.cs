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
		public string Signature { get { return string.Format("{0}.{1}", Namespace, Name); } }
		public string Namespace { get; private set; }
		public string Name { get; private set; }
        public string Scope { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }

        public Interface(FileRef file, string ns, string name, string scope, int line, int column)
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

        public IEnumerable<ResolveStatement> GetResolveStatements() {
            return getTypeResolveStatements();
        }
	}
}

