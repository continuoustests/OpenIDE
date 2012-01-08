using System;
namespace CSharp.Crawlers
{
	public class FilePosition
	{
		public string Fullpath { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
		
		public FilePosition(string file, int line, int column)
		{
			Fullpath = file;
			Line = line;
			Column = column;
		}
	}
}

