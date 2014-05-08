using System;
namespace OpenIDE.Core.Caching
{
	public interface ICodeReference
	{
		string Language { get; }
		string Type { get; }
		string File { get; }
        string Parent { get; }
		string Signature { get; }
		string Name { get; }
        string Scope { get; }
		int Line { get; }
		int Column { get; }
        string JSON { get; }

		bool TypeSearch { get; }

		bool Is(ICodeReference reference);
	}
	
	public class CodeReference : ICodeReference
	{
		public string Language { get; private set; }
		public string Type { get; private set; }
		public string File { get; private set; }
		public string Signature { get; private set; }
        public string Parent { get; private set; }
		public string Name { get; private set; }
        public string Scope { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }
        public string JSON { get; private set; }
		public bool TypeSearch { get; private set; }

		public CodeReference(
			string language,
			string type,
			string file,
            string parent,
			string signature,
			string name,
            string scope,
			int line,
			int column,
            string json)
		{
			Language = language;
			Type = type;
			File = file;
            Parent = parent;
			Signature = signature;
			Name = name;
            Scope = scope;
			Line = line;
			Column = column;
            JSON = json;
		}

		public CodeReference SetTypeSearch()
		{
			TypeSearch = true;
			return this;
		}

		public bool Is(ICodeReference compare)
		{
			return
				Language == compare.Language &&
				Type == compare.Type &&
				File == compare.File &&
				Signature == compare.Signature &&
				Parent == compare.Parent &&
				Name == compare.Name &&
				Scope == compare.Scope &&
				Line == compare.Line &&
				Column == compare.Column &&
				JSON == compare.JSON &&
				TypeSearch == compare.TypeSearch;
		}
	}

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

