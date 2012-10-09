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
		}

		private IOutputWriter _cache;
		private char[] _operators = new char[] 
			{
				'(',')',':','+','-','*','/','%','&','|',
				'^','!','~','=','?','+','-','{','}',';'
			};
		private char[] _whitespace = new char[]
            {'\r','\n',' ','\t'};
        private char[] _validBeforeWhitespace = new char[]
            {'.','<','>'};

		public TypeUnderPositionResolver() {
			_cache = new OutputWriter();
		}

		public string GetTypeName(string filePath, string content, int line, int column) {
			new NRefactoryParser() 
				.SetOutputWriter(_cache)
				.ParseFile(new FileRef(filePath, null), () => content);

			var lines = content
				.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			var location = rewindToBeginningOfWord(lines, line, column);
			if (location == null)
				return null;
			return readForward(lines, location.Line, location.Column);
		}

		private TypeUnderPositionResolver.Location rewindToBeginningOfWord(string[] lines, int line, int column) {
			var content = lines[line - 1];
			var chars = content.ToArray();
			var startLine = line;
			var startColumn = column;
			var lastchar = '.';
			for (int i = column - 2; i >= 0; i--) {
				var c = chars[i];
				if (_whitespace.Contains(c) && !_validBeforeWhitespace.Contains(c))
					break;
				if (_operators.Contains(c))
					break;
				lastchar = c;
				startColumn--;
			}
			return new TypeUnderPositionResolver.Location()  { 
					Line = startLine,
					Column = startColumn
				};
		}

		private string readForward(string[] lines, int line, int column) {
			var content = lines[line - 1];
			var chars = content.ToArray();
			var signature = "";
			for (int i = column - 1; i < content.Length; i++) {
				var c = chars[i];
				if (_operators.Contains(c))
					break;
				signature += c.ToString();
			}

			return signature;
		}
	}
}