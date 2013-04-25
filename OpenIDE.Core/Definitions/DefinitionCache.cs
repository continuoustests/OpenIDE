using System;
using System.Collections.Generic;
using OpenIDE.Core.Language;

namespace OpenIDE.Core.Definitions
{
	public class DefinitionCache
	{ 
		private List<DefinitionCacheItem> _definitions = new List<DefinitionCacheItem>();

		public DefinitionCacheItem[] Definitions { get { return _definitions.ToArray();; } }

		public DefinitionCacheItem Add(DefinitionCacheItemType type, string location, DateTime updated, string name, string description) {
			var item =
				new DefinitionCacheItem() {
						Type = type,
						Location = location,
						Updated = updated,
						Name = name,
						Description = description
					};
			_definitions.Add(item);
			return item;
		}
	}

	public enum DefinitionCacheItemType
	{
		Script,
		Language,
		BuiltIn
	}

	public class DefinitionCacheItem
	{
		public DefinitionCacheItemType Type { get; set; }
		public string Location { get; set; }
		public DateTime Updated { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public List<DefinitionCacheItem> Parameters = new List<DefinitionCacheItem>();

		public DefinitionCacheItem Add(DefinitionCacheItemType type, string location, DateTime updated, string name, string description) {
			var item =
				new DefinitionCacheItem() {
						Type = type,
						Location = location,
						Updated = updated,
						Name = name,
						Description = description
					};
			Parameters.Add(item);
			return item;
		}
	}
}