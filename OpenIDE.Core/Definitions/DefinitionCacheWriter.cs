using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenIDE.Core.Language;
using OpenIDE.Core.Logging;

namespace OpenIDE.Core.Definitions
{
	public class DefinitionCacheWriter
	{
		private string _path;
		private Action<string,string> _writer = (file,content) => File.WriteAllText(file, content);

		public DefinitionCacheWriter WriteUsing(Action<string,string> writer) {
			_writer = writer;
			return this;
		}

		public DefinitionCacheWriter(string path) {
			_path = path;
		}

		public void Write(DefinitionCache cache) {
			var file = Path.Combine(_path, "oi-definitions.json");
			var json = new List<DefinitionJson>();
			cache.Definitions.OrderBy(x => x.Override).ToList()
				.ForEach(x => json.Add(get(x)));
			_writer(
				file,
				LowercaseJsonSerializer
					.SerializeObject(new DefinedCommands() { Commands = json }));
		}

		private DefinitionJson get(DefinitionCacheItem parameter) {
			var json = getDefinition(parameter);
			addChildren(json, parameter.Parameters);
			return json;
		}

		private void addChildren(DefinitionJson json, IEnumerable<DefinitionCacheItem> parameters) {
			foreach (var parameter in parameters) {
				var child = getDefinition(parameter);
				addChildren(child, parameter.Parameters);
				json.Arguments.Add(child);
			}
		}

		private DefinitionJson getDefinition(DefinitionCacheItem parameter) {
			var json = new DefinitionJson();
			if (parameter.Override) {
				json.Cmd = "[[" + parameter.Name + "]]";
			} else {
				if (parameter.Required)
					json.Cmd = parameter.Name;
				else
					json.Cmd = "[" + parameter.Name + "]";
			}
			json.Type = parameter.Type.ToString().ToLower();
			json.Location = parameter.Location;
			json.Updated = 
				string.Format("{0}.{1}.{2} {3}:{4}:{5}",
					parameter.Updated.Year,
					parameter.Updated.Month.ToString().PadLeft(2, '0'),
					parameter.Updated.Day.ToString().PadLeft(2, '0'),
					parameter.Updated.Hour.ToString().PadLeft(2, '0'),
					parameter.Updated.Minute.ToString().PadLeft(2, '0'),
					parameter.Updated.Second.ToString().PadLeft(2, '0'));
			json.Description = parameter.Description;
			return json;
		}
	}

	class DefinedCommands
	{
		public List<DefinitionJson> Commands;
	}

	class DefinitionJson
	{
		public string Cmd { get; set; }
		public string Description { get; set; }
		public string Type { get; set; }
		public string Location { get; set; }
		public string Updated { get; set; }
		public List<DefinitionJson> Arguments = new List<DefinitionJson>();
	}

	class LowercaseJsonSerializer : DefaultContractResolver
	{
	    private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	    {
	        ContractResolver = new LowercaseContractResolver()
	    };

	    public static string SerializeObject(object o)
	    {
	        return JsonConvert.SerializeObject(o, Formatting.Indented, Settings);
	    }

	    public class LowercaseContractResolver : DefaultContractResolver
	    {
	        protected override string ResolvePropertyName(string propertyName)
	        {
	            return propertyName.ToLower();
	        }
	    }
	}
}