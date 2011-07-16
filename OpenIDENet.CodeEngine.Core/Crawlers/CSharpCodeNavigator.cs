using System;
using System.Collections.Generic;
using System.Linq;
namespace OpenIDENet.CodeEngine.Core.Crawlers
{
	public class CSharpCodeNavigator
	{
		private int _offset = 0;
		private char[] _chars;
		private int _line;
		private int _column;
		private char _last;
		private char _beforeLast;
		private List<Word> _words;
		private string _lineContent;
		private List<string> _lines;
		private Word _word;
		private Word _lastWord;
		private char[] _endOfWordChars = new char[] 
			{'[',']','(',')','.',':','+','-','*','/','%','&','|','^','!','~','=','<','>','?','+','-', ' '};
		private char[] _separators = new char[]
			{'{','}',';'};
		
		public int Offset { get { return _offset - 1; } }
		public int Line { get { return _line; } }
		public int Column { get { return _column; } }
		
		public CSharpCodeNavigator(char[] chars)
		{
			_chars = chars;
			_offset = 0;
			_line = 0;
			_column = 0;
			_last = ' ';
			_beforeLast = ' ';
			_words = new List<Word>();
			_word = new Word();
			_lastWord = null;
			_lines = new List<string>();
		}
		
		public Word GetWord()
		{
			while (_offset < _chars.Length)
			{
				var c = _chars[_offset];
				if (isComment(c))
					fastForwardToEndOfComment();
				var word = prepare(c);
				if (word != null)
					return word;
			}
			return null;
		}
		
		public Word CollectSignature()
		{
			var signature = new Word();
			while (_offset < _chars.Length)
			{
				var c = _chars[_offset];
				if (isComment(c))
					fastForwardToEndOfComment();
				var word = prepare(c);
				if (word != null && word.Text.Length > 0)
				{
					if (!signature.HasPosition())
						signature.SetPosition(word);
					signature.Text += word.Text + ".";
				}
				if (c == '{')
				{
					if (signature.Text.Length > 0)
					{
						signature.Text = signature.Text.Substring(0, signature.Text.Length - 2);
						return signature;
					}
					return null;
				}
			}
			return null;
		}
		
		private Word prepare(char c)
		{
			Word word = null;
			if (isNewLine())
			{
				_line++;
				_column = 0;
			}
			if (!_word.HasPosition() && !c.Equals(' ') && !c.Equals('\t'))
			{
				_word.Offset = _offset + 1;
				_word.Line = _line + 1;
				_word.Column = _column + 1;
			}
			if (c.Equals('\r') ||
				c.Equals('\n') ||
				c.Equals(' ') || 
				c.Equals('\t') || 
				_separators.Contains(c) ||
				_endOfWordChars.Contains(c))
			{
				if (_word.Text.Length > 0 &&
					_word.Text != " " && 
					_word.Text != "\t" && 
					!c.Equals('\r') && 
					!c.Equals('\n'))
				{
					_lastWord = _word;
					_words.Add(_word);
				}
				word = _word;
				_word = new Word();
			}
			else
				_word.Text += c;
			_beforeLast = _last;
			_last = c;
			_column++;
			_offset++;
			return word;
		}
		
		private bool isEndOfLine(char c)
		{
			return (Environment.NewLine.Length == 1 && Environment.NewLine.Equals(c.ToString())) || 
				   (Environment.NewLine.Length == 2 && Environment.NewLine.Equals((_last + c).ToString()));
		}
		
		private bool isNewLine()
		{
			return (Environment.NewLine.Length == 1 && Environment.NewLine.Equals(_last.ToString())) || 
				   (Environment.NewLine.Length == 2 && 
				   	Environment.NewLine.Equals((_beforeLast + _last).ToString()));
		}
		
		private bool isComment(char c)
		{
			var current = (_last + c).ToString();
			return current == "//" || current == "/*";
		}
		
		private void fastForwardToEndOfComment()
		{
			var comment = (_last + _chars[_offset]).ToString();
			char c = _chars[_offset];
			while (true)
			{
				prepare(c);
				if (_offset > _chars.Length)
					break;
				c = _chars[_offset];
				if ((comment == "//" && isEndOfLine(c)) ||
					(comment == "/*" && (_last + c).ToString() == "*/"))
					break;
			}
		}
	}
	
	public class Word
	{
		public int Offset { get; set; }
		public int Line { get; set; }
		public int Column { get; set; }
		public string Text { get; set; }
		
		public Word()
		{
			Offset = -1;
			Line = -1;
			Column = -1;
			Text = "";
		}
		
		public bool HasPosition()
		{
			return Offset != -1;
		}
		
		public void SetPosition(Word word)
		{
			Offset = word.Offset;
			Line = word.Line;
			Column = word.Column;
		}
	}
}

