using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharp.Crawlers
{
	public enum IfDef
	{
		If,
		Else,
		EndIf
	}

	public class CSharpCodeNavigator
	{
        private Action _beginningOfBody;
        private Action _endOfBody;
		private Action<IfDef> _ifDef;
		private int _offset = 0;
		private char[] _chars;
		private int _line;
		private int _column;
		private char _last;
		private char _beforeLast;
		private List<Word> _words;
		private Word _word;
		private char[] _operators = new char[] 
			{'[',']','(',')','.',':','+','-','*','/','%','&','|','^','!','~','=','<','>','?','+','-'};
        private char[] _bodySeparators = new char[]
            { '{', '}', ';' };
		private char[] _whitespace = new char[]
            {'\r','\n',' ','\t'};

		public int Offset { get { return _offset - 1; } }
		public int Line { get { return _line; } }
		public int Column { get { return _column; } }

        public CSharpCodeNavigator(char[] chars, Action beginningOfBody, Action endOfBody, Action<IfDef> ifDef)
		{
			_chars = chars;
            _beginningOfBody = beginningOfBody;
            _endOfBody = endOfBody;
			_ifDef = ifDef;
			_offset = 0;
			_line = 0;
			_column = 0;
			_last = ' ';
			_beforeLast = ' ';
			_words = new List<Word>();
			_word = new Word();
		}
		
		public Word GetWord()
		{
			try {
				while (_offset < _chars.Length)
				{
					var c = _chars[_offset];
					if (isComment(c))
						fastForwardToEndOfComment();
					if (isString(c))
						fastForwardToEndOfString();
					var word = prepare(c);
					if (word != null)
						return word;
				}
				return null;
			} catch {
				Console.WriteLine("GetWord failed while on line {0} and column {1}", _line, _column);
				throw;
			}
		}
		
		public Word CollectSignature()
		{
			try {
				return collectSignature();
			} catch {
				Console.WriteLine("GetWord failed while on line {0} and column {1}", _line, _column);
				throw;
			}
		}

		private Word collectSignature()
		{
			var signature = new Word();
			while (_offset < _chars.Length)
			{
				var c = _chars[_offset];
				if (isComment(c))
					fastForwardToEndOfComment();
				var word = prepare(c);			

				if (isValidWord(word))
				{
                    var wordChar = ' ';
                    if (word.Text.Length == 1)
                        wordChar = word.Text.ToCharArray()[0];
					if (!signature.HasPosition())
						signature.SetPosition(word);
                    if (word.Text.Length == 1 && _operators.Contains(wordChar))
                        signature.Text += word.Text;
                    else if (word.SyntaxOperator != ' ' && !_bodySeparators.Contains(word.SyntaxOperator))
                        signature.Text += word.Text + word.SyntaxOperator;
                    else if (!_bodySeparators.Contains(wordChar))
					    signature.Text += word.Text;
				}
                if (c == '{')
                {
                    if (signature.Text.Length > 0)
                        return signature;
                    return null;
                }
			}
			return null;
		}	

		private bool isValidWord(Word word)
		{
			return word != null && word.Text.Length > 0;
		}

		private Word prepare(char c)
		{
			return prepare(c, false);
		}

		private Word prepare(char c, bool isFastForward)
		{
			Word word = null;
			if (isNewLine())
				moveToNextLine();	

			if (!isFastForward)
			{
	            if (c == '}')
    	            _endOfBody();
        	    if (c == '{')
            	    _beginningOfBody();

				if (isBeginningOfWord(c))
					initializeWord();

    	        var isEndOfWord = _bodySeparators.Contains(c) || _operators.Contains(c);
        	    var isWhitespace = _whitespace.Contains(c);
            	if (isEndOfWord || isWhitespace)
	            	word = finalizeWord(isEndOfWord, isWhitespace, c);
    	        else
            	    _word.Text += c;
			}
			stepOneForward(c);
			if (isIfDef(word))
				notifyAboutIfDef(word);
			return word;
		}

		private void stepOneForward(char c)
		{
			_beforeLast = _last;
			_last = c;
			_column++;
			_offset++;	
		}

		private bool isIfDef(Word word)
		{	
			if (word == null)
				return false;
			return word.Text.StartsWith("#if") ||
				   word.Text.StartsWith("#else") ||
				   word.Text.StartsWith("#endif");
		}

		private void notifyAboutIfDef(Word word)
		{
			if (word.Text.StartsWith("#if"))
				_ifDef(IfDef.If);
			else if (word.Text.StartsWith("#else"))
				_ifDef(IfDef.Else);
			else if (word.Text.StartsWith("#endif"))
				_ifDef(IfDef.EndIf);
		}

		private void moveToNextLine()
		{
			_line++;
			_column = -1;
		}

		private bool isBeginningOfWord(char c)
		{
			return !_word.HasPosition() && !c.Equals(' ') && !c.Equals('\t');
		}

		private void initializeWord()
		{
			_word.Offset = _offset + 1;
			_word.Line = _line + 1;
			_word.Column = _column + 1;
		}

		private Word finalizeWord(bool isEndOfWord, bool isWhitespace, char c)
		{
			if (_word.Text.Length == 0 && isEndOfWord)
   	            _word.Text = c.ToString();
       	    if (isEndOfWord)
           	    _word.SyntaxOperator = c;

            if (_word.Text.Length > 0 || isWhitespace)
   	            _words.Add(_word);
       	    var word = _word.Clone();
           	_word = new Word();
			return word;
		}
		
		private bool isEndOfLine(char c)
		{
			return (Environment.NewLine.Length == 1 && Environment.NewLine.Equals(c.ToString())) || 
				   (Environment.NewLine.Length == 2 && 
				   		Environment.NewLine.Equals(_last.ToString() + c.ToString()));
		}
		
		private bool isNewLine()
		{
			return (Environment.NewLine.Length == 1 && Environment.NewLine.Equals(_last.ToString())) || 
				   (Environment.NewLine.Length == 2 && 
				   	Environment.NewLine.Equals(_beforeLast.ToString() + _last.ToString()));
		}
		
		private bool isComment(char c)
		{
			var current = _last.ToString() + c.ToString();
			return current == "//" || current == "/*";
		}
		
		private void fastForwardToEndOfComment()
		{
			var comment = _last.ToString() + _chars[_offset].ToString();
			char c = _chars[_offset];
			while (true)
			{
				prepare(c, true);
				if (_offset > (_chars.Length - 1))
					break;
				c = _chars[_offset];
				if ((comment == "//" && isEndOfLine(c)) ||
					(comment == "/*" && _last.ToString() + c.ToString() == "*/"))
					break;
			}
		}

		private bool isString(char c)
		{
			return c == '"' || c == '\'';
		}
		
		private void fastForwardToEndOfString()
		{
			char c = _chars[_offset];
			while (true)
			{
				prepare(c, true);
				if (_offset > (_chars.Length - 1))
					break;
				c = _chars[_offset];
				if ((c == '"' && _last != '\\') ||
					(c == '\'' && _last != '\\'))
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

        public char SyntaxOperator { get; set; }
		
		public Word()
		{
			Offset = -1;
			Line = -1;
			Column = -1;
			Text = "";
            SyntaxOperator = ' ';
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

		public Word Clone()
		{
			var word = new Word();
			word.SetPosition(this);
			word.Text = Text;
			word.SyntaxOperator = SyntaxOperator;
			return word;
		}
	}
}

