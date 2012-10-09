using System;
using System.Linq;
using CSharp.Crawlers;
using CSharp.Projects;

namespace CSharp.Crawlers.TypeResolvers
{
	public class TypeUnderPositionResolver
	{
		class Location
		{
			public int Line;
			public int Column;

			public override string ToString() {
				return Line.ToString() + ":" + Column.ToString();
			}
		}

		private IOutputWriter _cache;
		private Func<string,ICodeReference> _referenceFetcher;
		private char[] _operators = new char[] 
			{
				'(',')',':','+','-','*','/','%','&','|',
				'^','!','~','=','?','+','-','{','}',';'
			};
		private char[] _whitespace = new char[]
            {'\r','\n',' ','\t'};
        private char[] _validBeforeWhitespace = new char[]
            {'.','<','>'};

		public TypeUnderPositionResolver(IOutputWriter cache, Func<string,ICodeReference> referenceFetcher) {
			_cache = cache;
			_referenceFetcher = referenceFetcher;
		}

		public ICodeReference GetTypeName(string filePath, string content, int line, int column) {
			new NRefactoryParser() 
				.SetOutputWriter(_cache)
				.ParseFile(new FileRef(filePath, null), () => content);

			var lines = content
				.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			var location = rewindToBeginningOfWord(lines, line, column);
			if (location == null)
				return null;
			return _referenceFetcher(readForward(lines, location.Line, location.Column));
		}

		private TypeUnderPositionResolver.Location rewindToBeginningOfWord(string[] lines, int line, int column) {
			var startLine = line;
			var startColumn = column;
			var lastchar = '.';
			var exit = false;
			for (int j = line - 1; j >= 0; j--) {
				var content = lines[j];
				var chars = content.ToArray();
				startLine = j + 1;
				if (j + 1 == line)
					startColumn = column;
				else
					startColumn = chars.Length;
				for (int i = startColumn - 2; i >= 0; i--) {
					var c = chars[i];
					if (_whitespace.Contains(c) && !_validBeforeWhitespace.Contains(lastchar)) {
						exit = true;
						break;
					}
					if (_operators.Contains(c)) {
						exit = true;
						break;
					}
					if (!_whitespace.Contains(c))
						lastchar = c;
					startColumn--;
				}
				if (exit)
					break;
			}
			return new TypeUnderPositionResolver.Location()  { 
					Line = startLine,
					Column = startColumn
				};
		}

		private string readForward(string[] lines, int line, int column) {
			var signature = "";
			var exit = false;
			for (int j = line - 1; j >= 0; j++) {
				var content = lines[j];
				var chars = content.ToArray();
				if (j + 1 != line)
					column = 1;
				for (int i = column - 1; i < content.Length; i++) {
					var c = chars[i];
					if (_operators.Contains(c)) {
						exit = true;
						break;
					}
					signature += c.ToString();
				}
				if (exit)
					break;
			}
			foreach (var c in _whitespace)
				signature = signature.Replace(c.ToString(), "");
			return signature;
		}
	}
}