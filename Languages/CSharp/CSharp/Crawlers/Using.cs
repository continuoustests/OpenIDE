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
        public long ID { get; private set; }

        public bool AllTypesAreResolved { get; set; }

        public string Type { get; private set; }
        public FileRef File { get; private set; }
		public string Signature { get { return Name; } }
		public string Parent { get; private set; }
		public string Name { get; private set; }
        public string Scope { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
        public int EndLine { get; private set; }
        public int EndColumn { get; private set; }
        public string JSON { get; private set; }

        public Using(FileRef file, string name, int line, int column) {
			File = file;
			Parent = "";
			Name = name;
            Scope = "";
			Line = line;
			Column = column;
            JSON = "";
		}

        public void SetID(long id) {
            ID = id;
        }

        public Using SetEndPosition(int line, int column) {
            EndLine = line;
            EndColumn = column;
            return this;
        }

        public string ToFullSignature() {
            return Signature;
        }

        public string ToNamespaceSignature() {
            return Signature;
        }

        public IEnumerable<ResolveStatement> GetResolveStatements() {
            return new ResolveStatement[] {};
        }
    }

    public class UsingAlias : ICodeReference
    {
        public long ID { get; private set; }

        public bool AllTypesAreResolved { get; set; }

        public string Type { get; private set; }
        public FileRef File { get; private set; }
		public string Signature { get { return Name; } }
		public string Parent { get; private set; }
		public string Name { get; private set; }
        public string Scope { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
        public int EndLine { get; private set; }
        public int EndColumn { get; private set; }
        public string JSON { get; private set; }

        public UsingAlias(FileRef file, string alias, string type, int line, int column) {
			File = file;
			Parent = type;
			Name = alias;
            Scope = "";
			Line = line;
			Column = column;
            JSON = "";
		}

        public void SetID(long id) {
            ID = id;
        }

        public UsingAlias SetEndPosition(int line, int column) {
            EndLine = line;
            EndColumn = column;
            return this;
        }

        public string ToFullSignature() {
            return Signature;
        }

        public string ToNamespaceSignature() {
            return Signature;
        }

        public IEnumerable<ResolveStatement> GetResolveStatements() {
            return new ResolveStatement[] {};
        }
    }
}
