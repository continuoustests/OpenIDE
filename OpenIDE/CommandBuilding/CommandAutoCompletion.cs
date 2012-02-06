using System;
using System.Linq;
using OpenIDE.Core.CommandBuilding;

namespace OpenIDE.CommandBuilding
{
	public class CommandAutoCompletion
	{
        private string[] _command;
        private string[] _typedText;

        public string AutoComplete(string command, string typedText)
        {
            _command = splitParams(command);
            _typedText = splitParams(typedText);
            var autocomplete = "";
            for (int i = 0; i < _command.Length; i++)
            {
                if (endOfTypedText(i))
                    return _command[i];
                if (typedComparesToCommand(i))
                    continue;
                if (isPartiallyTyped(i))
                    return remainingText(i);
            }
            return autocomplete;
        }

        private string remainingText(int i)
        {
            return _command[i]
                .Substring(
                    _typedText[i].Length,
                    _command[i].Length - _typedText[i].Length);
        }

        private bool isPartiallyTyped(int i)
        {
            return _command[i].StartsWith(_typedText[i]);
        }

        private bool typedComparesToCommand(int i)
        {
            return _typedText[i] == _command[i];
        }

        private bool endOfTypedText(int i)
        {
            return _typedText.Length < (i + 1);
        }

        private string[] splitParams(string content)
        {
            return new CommandStringParser()
                .Parse(content)
                .ToArray();
        }
	}
}
