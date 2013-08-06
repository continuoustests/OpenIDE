using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Core.FileSystem;

namespace OpenIDE.Core.Definitions
{
	public class DefinitionCache
	{ 
		private List<DefinitionCacheItem> _builtIn = new List<DefinitionCacheItem>();
		private List<DefinitionCacheItem> _languages = new List<DefinitionCacheItem>();
		private List<DefinitionCacheItem> _scripts = new List<DefinitionCacheItem>();
		private List<DefinitionCacheItem> _definitions = new List<DefinitionCacheItem>();

		public DefinitionCacheItem[] Definitions { get { return _definitions.ToArray();; } }

		public DefinitionCacheItem Add(DefinitionCacheItemType type, string location, DateTime updated, bool required, string name, string description) {
			return add(type, location, updated, required, name, description);
		}

		public void Add(DefinitionCacheItemType type, string location, DateTime updated, string name, string description, IEnumerable<BaseCommandHandlerParameter> parameters) {
			var item = add(type, location, updated, true, name, description);
			foreach (var parameter in parameters)
				add(item, parameter);
		}

		public DefinitionCacheItem Get(string[] args) {
			return get(args, 0, _definitions, null);
		}

		public DefinitionCacheItem GetBuiltIn(string[] args) {
			return get(args, 0, _builtIn, null);
		}
		
		public DefinitionCacheItem GetLanguage(string[] args) {
			return get(args, 0, _languages, null);
		}
		
		public DefinitionCacheItem GetScript(string[] args) {
			return get(args, 0, _scripts, null);
		}

		public void Merge(DefinitionCache cache) {
			addRaw(cache.Definitions);
			var merge = cache.Definitions.Where(x => !_definitions.Any(y => x.Name == y.Name));
			_definitions.AddRange(merge);
		}

		public DefinitionCacheItem GetOldestItem() {
			return GetOldestItem(null);
		}

		public DefinitionCacheItem GetOldestItem(string location) {
			DefinitionCacheItem oldest = null;
			visitAll(
				(item) => {
					if (location != null && item.Location != location)
						return;
					if (oldest == null) {
						oldest = item;
					} else {
						if (oldest.Updated > item.Updated)
							oldest = item;
					}
				},
				_definitions);
			return oldest;
		}

		public string[] GetLocations(DefinitionCacheItemType type) {
			var locations = new List<string>();
			visitAll(
				(item) => {
						if (item.Type != type)
							return;
						if (locations.Contains(item.Location))
							return;
						locations.Add(item.Location);
					},
				_definitions);
			return locations.ToArray();
		}

		private void visitAll(Action<DefinitionCacheItem> visitor, IEnumerable<DefinitionCacheItem> items) {
			if (items.Count() == 0)
				return;
			foreach (var item in items) {
				visitor(item);
				visitAll(visitor, item.Parameters);
			}
		}

		private void add(DefinitionCacheItem item, BaseCommandHandlerParameter parameter) {
			var name = parameter.Name;
			var child =
				new DefinitionCacheItem(parameterAppender) {
						Type = item.Type,
						Location = item.Location,
						Updated = item.Updated,
						Required = parameter.Required,
						Name = name,
						Description = parameter.Description 
					};
			foreach (var cmd in parameter.Parameters)
				add(child, cmd);
			item.Append(child);
		}

		private DefinitionCacheItem add(DefinitionCacheItemType type, string location, DateTime updated, bool required, string name, string description) {
			var item =
				new DefinitionCacheItem(parameterAppender) {
						Type = type,
						Location = location,
						Updated = updated,
						Required = required,
						Name = name,
						Description = description
					};
			addRaw(item);
			_definitions.Add(item);
			return item;
		}

		private DefinitionCacheItem parameterAppender(List<DefinitionCacheItem> parameters, DefinitionCacheItem parameterToAdd) {
			parameters.Add(parameterToAdd);
			return parameterToAdd;
		}

		private DefinitionCacheItem get(string[] args, int index, IEnumerable<DefinitionCacheItem> items, DefinitionCacheItem parent) {
			if (index > args.Length - 1)
				return parent;
			if (items.Count() == 0)
				return parent;
			var item = items.FirstOrDefault(x => x.Name == args[index]);
			if (item == null) {
				if (isOptionalArgument(args[index], items)) {
					return parent;
				}
				if (items.Any(x => x.Name == x.Name.ToUpper()))
					return parent;
				return null;
			}
			if (args.Length == index + 1)
				return item;
			return get(args, index + 1, item.Parameters, item);
		}

		private bool isOptionalArgument(string argument, IEnumerable<DefinitionCacheItem> items) {
			if (items.Count() == 0)
				return false;
			if (items.Any(x => x.Name == argument))
				return true;
			var possibleItems = new List<DefinitionCacheItem>();
			items.ToList()
				.ForEach(x => possibleItems.AddRange(x.Parameters));
			return isOptionalArgument(argument, possibleItems);
		}

		private void addRaw(IEnumerable<DefinitionCacheItem> items) {
			foreach (var item in items) {
				addRaw(item);
			}
		}

		private void addRaw(DefinitionCacheItem item) {
			if (item.Type == DefinitionCacheItemType.BuiltIn)
				_builtIn.Add(item);
			else if (item.Type == DefinitionCacheItemType.Language)
				_languages.Add(item);
			else if (item.Type == DefinitionCacheItemType.Script)
				_scripts.Add(item);
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
		private List<DefinitionCacheItem> _parameters = new List<DefinitionCacheItem>();
		private Func<List<DefinitionCacheItem>,DefinitionCacheItem,DefinitionCacheItem> _parameterAppender;

		public DefinitionCacheItemType Type { get; set; }
		public string Location { get; set; }
		public DateTime Updated { get; set; }
		public bool Required { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public DefinitionCacheItem[] Parameters { get { return _parameters.ToArray(); } }

		public DefinitionCacheItem(Func<List<DefinitionCacheItem>,DefinitionCacheItem,DefinitionCacheItem> parameterAppender) {
			_parameterAppender = parameterAppender;
		}

		public DefinitionCacheItem Append(DefinitionCacheItemType type, string location, DateTime updated, bool required, string name, string description) {
			return 
				_parameterAppender(
					_parameters,
					new DefinitionCacheItem(_parameterAppender) {
							Type = type,
							Location = location,
							Updated = updated, 
							Required = required,
							Name = name,
							Description = description
						});
		}

		public DefinitionCacheItem Append(DefinitionCacheItem parameter) {
			return _parameterAppender(_parameters, parameter);
		}
	}
}
