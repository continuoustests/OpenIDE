using System;
using System.Linq;
using OpenIDENet.Core.CommandBuilding;

namespace OpenIDENet.UI
{
	public class SnippetReplaceController
	{
		private string[] _originalPlaceHolders;
		private string[] _placeHolders;
		private string _content;

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
		}

		public void SetContent(string replacementString, int cursorPosition)
		{
			var replacements = new CommandStringParser()
				.Parse(replacementString).ToArray();
			ModifiedSnippet = _content;
			for (int i = 0; i < _placeHolders.Length; i++)
			{
				if (i >= replacements.Length)
					break;
				ModifiedSnippet = ModifiedSnippet.Replace(_placeHolders[i], replacements[i]);
			}
			setCurrentFromPosition(replacementString, cursorPosition, replacements);
		}

		private void setCurrentFromPosition(string text, int position, string[] chunks)
		{
			if (chunks.Length > _placeHolders.Length)
			{
				CurrentPlaceholder = "There are no more placeholders?! Stop writing!";
				return;
			}
			if (position == text.Length && text.EndsWith(" "))
			{
				CurrentPlaceholder = _placeHolders[chunks.Length];
				return;
			}
			var end = 0;
			var index = 0;
			foreach (var chunk in chunks)
			{
				end = getEndOf(chunk, text, end);
				if (end == -1)
					break;
				if (end >= position)
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
