using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Crawlers.TypeResolvers;
using CSharp.Projects;

namespace CSharp.Crawlers
{
    public class Using : ICodeReference
    {
        public bool AllTypesAreResolved { get; set; }

        public string Type { get; private set; }
        public FileRef File { get; private set; }
		public string Signature { get { return Name; } }
		public string Namespace { get; private set; }
		public string Name { get; private set; }
        public string Scope { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
        public string JSON { get; private set; }

        public Using(FileRef file, string name, int line, int column) {
			File = file;
			Namespace = "";
			Name = name;
            Scope = "";
			Line = line;
			Column = column;
            JSON = "";
		}

        public string GenerateFullSignature() {
            return null;
        }

        public IEnumerable<ResolveStatement> GetResolveStatements() {
            return new ResolveStatement[] {};
        }
    }
}
