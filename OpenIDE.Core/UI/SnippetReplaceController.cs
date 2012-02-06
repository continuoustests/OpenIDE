using System;
using System.Linq;
using OpenIDE.Core.CommandBuilding;

namespace OpenIDE.Core.UI
{
	public class SnippetReplaceController
	{
		private string[] _originalPlaceHolders;
		private string[] _placeHolders;
		private string _content;
		private int _activePosition = 0;
		private string _activeText = "";
		private string[] _activeReplacements = new string[] {};

		public string CurrentPlaceholder { get; private set; }
		public string ModifiedSnippet { get; private set; }

		public SnippetReplaceController(string[] placeHolders, string content)
		{
			_originalPlaceHolders = placeHolders;
			_placeHolders = placeHolders;
			_content = content;
			if ( _placeHolders.Length > 0)
				CurrentPlaceholder = placeHolders[0];
			ModifiedSnippet = _content;
		}

		public void SetModifiedContent(string content)
		{
			_content = content;
			_placeHolders = _originalPlaceHolders.Where(x => _content.IndexOf(x) != -1).ToArray();
			setCurrentFromPosition();
		}

		public void SetContent(string replacementString, int cursorPosition)
		{
			_activeText = replacementString;
			_activeReplacements = new CommandStringParser()
				.Parse(_activeText).ToArray();
			_activePosition = cursorPosition;

			ModifiedSnippet = _content;
			for (int i = 0; i < _placeHolders.Length; i++)
			{
				if (i >= _activeReplacements.Length)
					break;
				ModifiedSnippet = ModifiedSnippet.Replace(_placeHolders[i], _activeReplacements[i]);
			}
			setCurrentFromPosition();
		}

		private void setCurrentFromPosition()
		{
			if (_activeReplacements.Length > _placeHolders.Length)
			{
				Console.WriteLine("chunks: " + _activeReplacements.Length.ToString());
				CurrentPlaceholder = "There are no more placeholders?! Stop writing!";
				return;
			}
			if (_activePosition == _activeText.Length &&
				_activeText.EndsWith(" ") &&
				_activeText.Count(x => x.Equals('\"')) % 2 == 0)
			{
				if (_placeHolders.Length - 1 < _activeReplacements.Length)
					CurrentPlaceholder = _placeHolders[_placeHolders.Length - 1];
				else
					CurrentPlaceholder = _placeHolders[_activeReplacements.Length];
				return;
			}
			var end = 0;
			var index = 0;
			foreach (var chunk in _activeReplacements)
			{
				end = getEndOf(chunk, _activeText, end);
				if (end == -1)
					break;
				if (end >= _activePosition)
				{
					CurrentPlaceholder = _placeHolders[index];
					return;
				}
				index++;
			}
			if ( _placeHolders.Length > 0)
				CurrentPlaceholder = _placeHolders[0];
		}

		private int getEndOf(string chunk, string text, int offset)
		{
			var start = text.IndexOf(chunk, offset);
			if (start == -1)
				return -1;
			return start + chunk.Length;
		}
	}
}
