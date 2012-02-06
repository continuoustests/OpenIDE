using System;

namespace OpenIDE.Core.CommandBuilding
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

		public FilePosition(string command)
		{
			var chunks = command.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
			if (chunks.Length > 0)
				Fullpath = chunks[0];
			int line;
			if (chunks.Length > 1)
				if (int.TryParse(chunks[1], out line))
					Line = line;
			int column;
			if (chunks.Length > 2)
				if (int.TryParse(chunks[2], out column))
					Column = column;
		}

		public void Add(Position position)
		{
			Line += position.Line;
			if (position.Line > 0)
				Column = position.Column;
			else
				Column += position.Column;
		}

		public string ToCommand()
		{
			return string.Format("{0}|{1}|{2}", Fullpath, Line, Column);
		}
	}

	public class Position
	{
		public int Line { get; private set; }
		public int Column { get; private set; }
		
		public Position(int line, int column)
		{
			Line = line;
			Column = column;
		}

		public Position(string command)
		{
			var chunks = command.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
			int line;
			if (chunks.Length > 0)
				if (int.TryParse(chunks[0], out line))
					Line = line;
			int column;
			if (chunks.Length > 1)
				if (int.TryParse(chunks[1], out column))
					Column = column;
		}

		public void AddToColumn(int offset)
		{
			Column += offset;
		}

		public string ToCommand()
		{
			return string.Format("{0}|{1}", Line, Column);
		}
	}
}
