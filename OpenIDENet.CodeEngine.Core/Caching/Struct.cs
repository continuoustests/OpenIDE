using System;
namespace OpenIDENet.CodeEngine.Core.Caching
{
	public class Struct : ICodeType
	{
		public string Fullpath { get; private set; }
		public string Signature { get { return string.Format("{0}.{1}", Namespace, Name); } }
		public string Namespace { get; private set; }
		public string Name { get; private set; }
		public int Offset { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
		
		public Struct(string file, string ns, string name, int offset, int line, int column)
		{
			Fullpath = file;
			Namespace = ns;
			Name = name;
			Offset = offset;
			Line = line;
			Column = column;
		}
	}
}

