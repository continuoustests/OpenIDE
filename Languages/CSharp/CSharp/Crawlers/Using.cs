using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp.Crawlers
{
    public class Using : ICodeReference
    {
        public string Type { get; private set; }
		public string File { get; private set; }
		public string Signature { get { return Name; } }
		public string Namespace { get; private set; }
		public string Name { get; private set; }
        public string Scope { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
        public string JSON { get; private set; }
		
		public Using(string file, string name, int line, int column)
		{
			File = file;
			Namespace = "";
			Name = name;
            Scope = "";
			Line = line;
			Column = column;
            JSON = "";
		}
    }
}
