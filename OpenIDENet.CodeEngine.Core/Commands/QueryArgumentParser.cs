using System;
using System.Collections.Generic;

namespace OpenIDENet.CodeEngine.Core.Commands
{
	public class QueryArgumentParser
	{
		public List<KeyValuePair<string,string>> Parse(string argument)
		{
			var list = new List<KeyValuePair<string,string>>();
			var insideQuote = false;
			var word = "";
			var name = "";
			var previous = ' ';
			foreach (var c in argument)
			{
				if (!insideQuote && c == '"')
				{
					insideQuote = true;
					continue;
				}

				if (insideQuote)
				{
					if (c == '"' && previous != '\\')
					{
						insideQuote = false;
						continue;
					}
					word += c.ToString();
					previous = c;
					continue;
				}

				if (c == '=')
				{
					name = word.Trim();
					word = "";
					continue;
				}

				if (c == ',')
				{
					list.Add(new KeyValuePair<string,string>(name, word.Trim().Replace("\\\"", "\"")));
					word = "";
					continue;
				}
				word += c.ToString();
				previous = c;
			}
			if (name.Length > 0 && word.Length > 0)
				list.Add(new KeyValuePair<string,string>(name, word.Trim().Replace("\\\"", "\"")));
			if (list.Count == 0)
				return null;
			return list;
		}
	}
}
