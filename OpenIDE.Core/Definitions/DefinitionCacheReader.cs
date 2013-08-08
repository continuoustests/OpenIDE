using System;
using System.IO;
using Newtonsoft.Json.Linq;
using OpenIDE.Core.Language;

namespace OpenIDE.Core.Definitions
{
	public class DefinitionCacheReader
	{
		private Func<string,string> _readFile = (file) => File.ReadAllText(file);

		public DefinitionCacheReader ReadUsing(Func<string,string> fileReader) {
			_readFile = fileReader;
			return this;
		}

		public DefinitionCache Read(string file) {
			try {
				var cache = new DefinitionCache();
				var json = JObject.Parse(_readFile(file));
				foreach (var cmd in json["commands"].Children())
					addItem(cache, cmd);
				return cache;
			} catch {
			}
			return null;
		}

		private void addItem(DefinitionCache cache, JToken json) {
			var item = 
				cache
					.Add(
						getType(json["type"].ToString()),
						json["location"].ToString(),
						getTime(json["updated"].ToString()),
						!json["cmd"].ToString().StartsWith("["),
						json["cmd"].ToString().Replace("[", "").Replace("]", ""),
						json["description"].ToString());
			foreach (var child in json["arguments"].Children())
				addItem(item, child);
		}

		private void addItem(DefinitionCacheItem parent, JToken json) {
			var item = 
				parent
					.Append(
						getType(json["type"].ToString()),
						json["location"].ToString(),
						getTime(json["updated"].ToString()),
						!json["cmd"].ToString().StartsWith("["),
						json["cmd"].ToString().Replace("[", "").Replace("]", ""),
						json["description"].ToString());
			foreach (var child in json["arguments"].Children())
				addItem(item, child);
		}

		private DefinitionCacheItemType getType(string type) {
			if (type == "script")
				return DefinitionCacheItemType.Script;
			else if (type == "language")
				return DefinitionCacheItemType.Language;
			else if (type == "languagescript")
				return DefinitionCacheItemType.LanguageScript;
			else
				return DefinitionCacheItemType.BuiltIn;
		}

		private DateTime getTime(string expression) {
			var chunks = expression.Split(new[] { ' ' });
			var date = chunks[0].Split(new[] { '.' });
			var time = chunks[1].Split(new[] { ':' });
			return
				new DateTime(
					int.Parse(date[0]),
					int.Parse(date[1]),
					int.Parse(date[2]),
					int.Parse(time[0]),
					int.Parse(time[1]),
					int.Parse(time[2]));
		}
	}
}