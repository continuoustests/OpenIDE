using System;
using System.Collections.Generic;

namespace OpenIDENet.CommandBuilding
{
	public class CommandStringParser
	{
        private List<string> _words;
        private char _separator;
        private string _word;

        public IEnumerable<string> Parse(string arguments)
        {
            _words = new List<string>();
            _separator = ' ';
            _word = "";
            for (int i = 0; i < arguments.Length; i++)
                processCharacter(arguments[i]);
            addWord();
            return _words;
        }

        private void processCharacter(char argument)
        {
            if (isArgumentSeparator(argument))
                _separator = argument;

            if (itTerminatesArgument(argument))
            {
                addWord();
                _word = "";
                return;
            }
            _word += argument.ToString();
        }

        private void addWord()
        {
            if (_word.Length > 0)
                _words.Add(_word);
        }

        private bool itTerminatesArgument(char argument)
        {
            return argumentIsTerminatedWithSpace(argument) ||
                   argumentIsTerminatedWithQuote(argument);
        }

        private bool isArgumentSeparator(char argument)
        {
            return (_word.Length == 0 && argument == ' ') ||
                   (_word.Length == 0 && argument == '"');
        }

        private bool argumentIsTerminatedWithSpace(char arguments)
        {
            return (arguments == ' ' && _separator == ' ');
        }

        private bool argumentIsTerminatedWithQuote(char arguments)
        {
            return (arguments == '"' && _separator == '"');
        }
	}
}