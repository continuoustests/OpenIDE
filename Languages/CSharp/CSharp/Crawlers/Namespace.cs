using System;
namespace CSharp.Crawlers
{
	public class Namespace : ICodeReference
	{
		public string Fullpath { get; private set; }
		public string Signature { get { return Name; } }
		public string Name { get; private set; }
		public int Offset { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
		
		public Namespace(string file, string name, int offset, int line, int column)
		{
			Fullpath = file;
			Name = name;
			Offset = offset;
			Line = line;
			Column = column;
		}
	}
}

