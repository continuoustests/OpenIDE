using System;
using OpenIDENet.CodeEngine.Core.Caching;
using System.Collections.Generic;
using System.Linq;
namespace OpenIDENet.CodeEngine.Core.Crawlers
{
	enum Location
	{
		Root,
		Namespace,
		Class,
		Struct,
		Enum,
		Method
	}
	
	public class CSharpFileParser
	{
		private object _padLock = new object();
		private ICacheBuilder _builder;
		private string _file;
		private string _content;
		private Location _location;
		private CSharpCodeNavigator _navigator;
		
		private Namespace _currentNamespace = null;
		
		public CSharpFileParser(ICacheBuilder builder)
		{
			_builder = builder;
		}
		
		public void ParseFile(string file, Func<string> getContent)
		{
			lock (_padLock)
			{
				_builder.AddFile(file);
				_file = file;
				_content = getContent();
				_location = Location.Root;
				_currentNamespace = null;
				_navigator = new CSharpCodeNavigator(_content.ToCharArray());
				parse();
			}
		}
		
		private void parse()
		{
			Word word;
			while ((word = _navigator.GetWord()) != null)
			{
				if (_location == Location.Root)
					whenInRoot(word);
				else if (_location == Location.Namespace)
					whenInNamespace(word);
				//else if (_location == Location.Class)
				//	handleClass(word);
				//else if (_location == Location.Struct)
				//else if (_location == Location.Enum)
				//else if (_location == Location.Method)
			}
		}
		
		private void whenInRoot(Word word)
		{
			if (word.Text == "namespace")
				handleNamespace(word);
		}

		private void whenInNamespace(Word word)
		{
			if (word.Text == "class")
				handleClass(word);
				
		}

		private void handleNamespace(Word word)
		{
			var signature = _navigator.CollectSignature();
			var ns = new Namespace(
				_file,
				signature.Text,
				signature.Offset,
				signature.Line,
				signature.Column);
			_builder.AddNamespace(ns);
			_currentNamespace = ns;
			_location = Location.Namespace;
		}
		
		private void handleClass(Word word)
		{
			var signature = _navigator.CollectSignature();
			var ns = "";
			if (_currentNamespace != null)
				ns = _currentNamespace.Name;
			_builder.AddClass(
				new Class(
					_file,
					ns,
					signature.Text,
					signature.Offset,
					signature.Line,
					signature.Column));
			_location = Location.Class;
		}
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		/*private void handleComments()
		{
			if (_commentStart == -1)
			{
				//_commentStart = _line.IndexOf("/*");
				_commentEnd = _line.IndexOf("* /");
			}
			else
			{
				if (_commentEnd == -1)
				{
					_commentEnd = _line.IndexOf("* /");
					if (_commentEnd != -1)
						_commentStart = _line.IndexOf("/*", _commentEnd);
				}
				else
				{
					_commentStart = -1;
					_commentEnd = -1;
				}
			}
		}
		
		private void getClosures()
		{
			int index = 0;
			while (true)
			{
				var start = _line.IndexOf('{', index);
				if (start != -1)
					addClosure(start, '{');
				else if ((start = _line.IndexOf('}', index)) != -1)
					addClosure(start, '}');
				else if ((start = _line.IndexOf(';', index)) != -1)
					addClosure(start, ';');
				if (start == -1)
					break;
				index = start + 1;
			}
		}
		
		private void addClosure(int start, char type)
		{
			if (_commentStart != -1 && start > _commentStart && (_commentEnd == -1 || _commentEnd > start))
				return;
			_closures.Add(new KeyValuePair<int, char>(_offset + start, type));
		}
		
		private void handle()
		{
			var curly = _line.IndexOf("{");
			while (curly != -1)
			{
				if (!(_commentStart != -1 && curly > _commentStart && (_commentEnd == -1 || _commentEnd > curly)))
				{
					_currentCurly = curly;
					extractFromCurly(curly);
				}
				curly = _line.IndexOf("{", curly + 1);
			}
		}
		
		private void extractFromCurly(int index)
		{
			var chunk = getClosure(_offset + index);
			if (chunk == null)
				return;
			var keyword = findKeyword(chunk, "class");
			if (keyword != null)
				extractClass(keyword, chunk);
			else if ((keyword = findKeyword(chunk, "namespace")) != null)
				extractNamespace(keyword, chunk);
			else if ((keyword = findKeyword(chunk, "interface")) != null)
				extractInterface(keyword, chunk);
			else if ((keyword = findKeyword(chunk, "struct")) != null)
				extractStruct(keyword, chunk);
			else if ((keyword = findKeyword(chunk, "enum")) != null)
				extractEnum(keyword, chunk);
		}
		
		private void extractClass(Keyword keyword, string chunk)
		{
			extractType(
				keyword,
			    chunk,
			    (name, offset, lineNumber, column) =>
			            { 
							_builder.AddClass(new Class(_file, _currentNamespace, name, offset, lineNumber, column));
						});
		}
		
		private void extractInterface(Keyword keyword, string chunk)
		{
			extractType(
				keyword,
			    chunk,
			    (name, offset, lineNumber, column) =>
			            { 
							_builder.AddInterface(new Interface(_file, _currentNamespace, name, offset, lineNumber, column));
						});
		}
		
		private void extractStruct(Keyword keyword, string chunk)
		{
			extractType(
				keyword,
			    chunk,
			    (name, offset, lineNumber, column) =>
			            { 
							_builder.AddStruct(new Struct(_file, _currentNamespace, name, offset, lineNumber, column));
						});
		}
		
		private void extractEnum(Keyword keyword, string chunk)
		{
			extractType(
				keyword,
			    chunk,
			    (name, offset, lineNumber, column) =>
			            { 
							_builder.AddEnum(new EnumType(_file, _currentNamespace, name, offset, lineNumber, column));
						});
		}
		
		private void extractNamespace(Keyword keyword, string chunk)
		{
			extractType(
				keyword,
			    chunk,
			    (name, offset, lineNumber, column) =>
			            { 
							_builder.AddNamespace(new Namespace(_file, name, offset, lineNumber, column));
							_currentNamespace = name;
						});
		}
		
		private void extractType(Keyword keyword, string chunk, Action<string, int, int, int> action)
		{
			/*try
			{* /
				var typeName = trim(chunk.Substring(keyword.Index + keyword.Pattern.Length, chunk.Length - (keyword.Index + keyword.Pattern.Length)), keyword.Pattern);	
				var start = chunk.IndexOf(typeName, keyword.Index);
				var column = start;
				if (_currentCurly > chunk.Length)
					column = (_currentCurly - chunk.Length) + start;
				else if (chunk.LastIndexOf(Environment.NewLine, start) != -1)
					column = start - (chunk.LastIndexOf(Environment.NewLine, start) + Environment.NewLine.Length);
				var lineNumber = _lineIndex - countLines(chunk.Substring(start, chunk.Length - start));
				var offset = _offset + (chunk.Length - start);
				if (chunk.LastIndexOf(Environment.NewLine) != -1)
				{
					var offsetIndex = chunk.LastIndexOf(Environment.NewLine) + Environment.NewLine.Length;
					if (offsetIndex < start)
						offset = _offset + (start - offsetIndex);
					else
						offset = _offset - (offsetIndex - start);
				}
				else
					offset = _offset + (_currentCurly - chunk.Length) + start;
				action.Invoke(typeName, offset + 1, lineNumber + 1, column + 1);
			/*}
			catch (Exception ex)
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendLine("Failed to handle: " + chunk + " on line " + _lineIndex.ToString() + " in file " + _file);
				sb.AppendLine(ex.ToString());
				sb.AppendLine("----------------------------------------------------");
				using (var writer = new System.IO.StreamWriter("/home/ack/tmp/run_output_code_engine.txt", true))
					writer.WriteLine(sb.ToString());
			}* /
		}
		
		private string getClosure(int offset)
		{
			var previousOffsets = _closures.Where(x => x.Key < offset);
			if (previousOffsets.Count() == 0)
				return null;
			var start = previousOffsets.Max(x => x.Key);
			var text = _content.Substring(start, offset - start);
			var lineComment = text.LastIndexOf("//");
			var multiLineCommentStart = text.LastIndexOf("/*");
			var multiLineCommentEnd = text.LastIndexOf("* /");
			var maxInt = new int[] {lineComment, multiLineCommentStart, multiLineCommentEnd}.Max();
			if (maxInt != -1)
				return text.Substring(maxInt, text.Length - maxInt);
			return text;
		}
		
		private Keyword findKeyword(string text, string keyword)
		{
			var contains = new string[] {
											string.Format("\t{0} ", keyword),
											string.Format("\t{0}\t", keyword),
											string.Format("\t{0}{1}", keyword, Environment.NewLine),
											string.Format(" {0} ", keyword),
											string.Format(" {0}\t", keyword),
											string.Format(" {0}{1}", keyword, Environment.NewLine),
											string.Format("{1}{0} ", keyword, Environment.NewLine),
											string.Format("{1}{0}\t", keyword, Environment.NewLine),
											string.Format("{1}{0}{1}", keyword, Environment.NewLine)
										};
			var startswith = new string[] {
											string.Format("{0} ", keyword),
								            string.Format("{0}\t", keyword),
											string.Format("{0}{1}", keyword, Environment.NewLine)
										  };
			var results = new List<Keyword>();
			results.AddRange(contains.Select(x => new Keyword() { Index = text.LastIndexOf(x), Pattern = x}).Where(x => x.Index != -1 && 
			                                                                                                       !isBetweenCommentAndEndOfLine(text, x.Index) && 
			                                                                                                       !isBetweenMultilineComments(text, x.Index) && 
			                                                                                                       !isInsideString(text, x.Index) &&
			                 																					   !isBetweenHashAndEndOfLine(text, x.Index)));
			results.AddRange(startswith.Where(x => text.StartsWith(x)).Select(x => new Keyword() { Index = 0, Pattern = x}));
			return results.OrderBy(x => x.Index).FirstOrDefault();
		}
		
		private bool isBetweenCommentAndEndOfLine(string text, int index)
		{
			var comment = text.LastIndexOf("//", index);
			if (comment == -1)
				return false;
			return text.LastIndexOf(Environment.NewLine, index) < comment;
		}
		
		private bool isBetweenMultilineComments(string text, int index)
		{
			var start = text.LastIndexOf("/ *", index);
			if (start == -1)
				return false;
			return text.LastIndexOf("* /", index) < start;
		}
		
		private bool isInsideString(string text, int index)
		{
			var start = text.LastIndexOf("\"", index);
			if (start == -1)
				return false;
			return text.IndexOf("\"", index) != -1;
		}
		
		private bool isBetweenHashAndEndOfLine(string text, int index)
		{
			var hash = text.LastIndexOf("#", index);
			if (hash == -1)
				return false;
			return text.LastIndexOf(Environment.NewLine, index) < hash;
		}
		
		private string trim(string text, string additionalTrimPattern)
		{
			if (text == null)
				return "";
			if (text.IndexOf(":") != -1)
				text = text.Substring(0, text.IndexOf(":"));
			if (text.IndexOf(" where") != -1)
				text = text.Substring(0, text.IndexOf(" where"));
			if (text.IndexOf("\twhere") != -1)
				text = text.Substring(0, text.IndexOf("\twhere"));
			if (text.IndexOf("#") != -1)
				text = text.Substring(0, text.IndexOf("#"));
			if (text.IndexOf("//") != -1)
				text = text.Substring(0, text.IndexOf("//"));
			if (text.IndexOf("/*") != -1)
				text = text.Substring(0, text.IndexOf("/*"));
			return text
				.Replace(additionalTrimPattern, "")
				.Replace(Environment.NewLine, "")
				.Replace("\t", "")
				.Replace("\r", "")
				.Replace("\n", "")
				.TrimStart()
				.TrimEnd();
		}
		
		private int countLines(string text)
		{
			int lineCount = 0;
			int index = 0;
			while (true)
			{
				var start = text.IndexOf(Environment.NewLine, index);
				if (start != -1)
					lineCount++;
				if (start == -1)
					break;
				index = start + 1;
			}
			return lineCount;
		}
		
		private class Keyword { public int Index; public string Pattern; };*/
	}
}

