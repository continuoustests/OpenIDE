using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace OpenIDE.Core.CommandBuilding
{
    public class CommandStringParser
    {
        private List<string> _words;
        private char _separator;
        private char _previousChar;
        private string _word;
        private char _delimiter = ' ';
        private bool _addEmptyWords = false;
        private bool _insideQuotes = false;

        public CommandStringParser()
        {
        }

        public CommandStringParser(char delimiter)
        {
            _delimiter = delimiter;
        }

        public string GetCommand(IEnumerable<string> args)
        {
            return args.ElementAt(0);
        }

        public string GetArgumentString(string[] args)
        {
            return GetArgumentString(args, "\"");
        }

        public string GetArgumentString(string[] arguments, string stringGroupCharacter)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < arguments.Length; i++)
            {
                if (i == 0)
                    sb.Append(stringGroupCharacter + arguments[i] + stringGroupCharacter);
                else
                    sb.Append(" " + stringGroupCharacter + arguments[i] + stringGroupCharacter);
            }
            return sb.ToString();

        }

        public string[] GetArguments(IEnumerable<string> commandArgs)
        {
            var args = commandArgs.ToArray();
            if (args.Length == 1)
                return new string[] {};
            string[] newArgs = new string[args.Length - 1];
            for (int i = 1; i < args.Length; i++)
                newArgs[i - 1] = args[i];
            return newArgs;
        }

        public IEnumerable<string> Parse(string arguments)
        {
            _words = new List<string>();
            _separator = _delimiter;
            _word = "";
            for (int i = 0; i < arguments.Length; i++) {
                processCharacter(arguments[i]);
                _previousChar = arguments[i];
            }
            addWord();
            return _words;
        }

        private void processCharacter(char argument)
        {
            if (isArgumentSeparator(argument)) {
                _separator = argument;
                if (isQuote(argument)) {
                    _insideQuotes = true;
                    return;
                }
            }

            if (itTerminatesArgument(argument))
            {
                addWord();
                _word = "";
                _insideQuotes = false;
                return;
            }
            _word += argument.ToString();
        }

        private void addWord()
        {
            if (_word.Length > 0 || _addEmptyWords) {
                if (!_insideQuotes)
                    _word = _word.Trim();
                _words.Add(_word);
            }
        }

        private bool itTerminatesArgument(char argument)
        {
            return argumentIsTerminatedWithSpace(argument) ||
                   argumentIsTerminatedWithQuote(argument);
        }

        private bool isArgumentSeparator(char argument)
        {
            return
                (!_insideQuotes && _word.Length == 0 && argument == _delimiter) ||
                (_word.Length == 0 && argument == '"') ||
                (_word.Length == 0 && argument == '\'');
        }

        private bool isQuote(char argument)
        {
            return argument == '"' || argument == '\'';
        }

        private bool argumentIsTerminatedWithSpace(char arguments)
        {
            if (_insideQuotes)
                return false;
            return (arguments == _delimiter && _separator == _delimiter);
        }

        private bool argumentIsTerminatedWithQuote(char arguments)
        {
            if (!_insideQuotes)
                return false;
            return
                (arguments == '"' && _separator == '"' && _previousChar != '\\') ||
                (arguments == '\'' && _separator == '\'' && _previousChar != '\\');
        }
    }
}
