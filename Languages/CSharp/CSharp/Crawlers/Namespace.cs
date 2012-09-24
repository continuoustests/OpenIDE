using System;
namespace CSharp.Crawlers
{
	public class Namespace : ICodeReference
	{
		public string Type { get; private set; }
		public string File { get; private set; }
		public string Signature { get { return Name; } }
		public string Name { get; private set; }
        public string Scope { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
        public string JSON { get; private set; }
		
		public Namespace(string file, string name, int line, int column)
		{
			File = file;
			Name = name;
            Scope = "";
			Line = line;
			Column = column;
            JSON = "";
		}
	}
}

