using System;
namespace CSharp.Crawlers
{
	public class Class : ICodeReference 
	{
		public string Type { get; private set; }
		public string File { get; private set; }
		public string Signature { get { return string.Format("{0}.{1}", Namespace, Name); } }
		public string Namespace { get; private set; }
		public string Name { get; private set; }
        public string Scope { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
        public string JSON { get; private set; }
		
		public Class(string file, string ns, string name, string scope, int line, int column, string json)
		{
			File = file;
			Namespace = ns;
			Name = name;
            Scope = scope;
			Line = line;
			Column = column;
            JSON = json;
		}
    }
}

