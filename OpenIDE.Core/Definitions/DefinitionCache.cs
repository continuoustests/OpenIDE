using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Logging;

namespace OpenIDE.Core.Definitions
{
	public class DefinitionCache
	{ 
		private List<DefinitionCacheItem> _builtIn = new List<DefinitionCacheItem>();
		private List<DefinitionCacheItem> _languages = new List<DefinitionCacheItem>();
		private List<DefinitionCacheItem> _languageScripts = new List<DefinitionCacheItem>();
		private List<DefinitionCacheItem> _scripts = new List<DefinitionCacheItem>();
		private List<DefinitionCacheItem> _definitions = new List<DefinitionCacheItem>();

		public DefinitionCacheItem[] Definitions { get { return _definitions.ToArray();; } }

		public DefinitionCacheItem Add(DefinitionCacheItemType type, string location, DateTime updated, bool ovrride, bool required, string name, string description) {
			return add(type, location, updated, ovrride, required, name, description);
		}
		public void Add(DefinitionCacheItem item) {
			_definitions.Add(item);
		}

		public DefinitionCacheItem Get(string[] args) {
			return get(args, 0, _definitions, null);
		}

		public DefinitionCacheItem GetOriginal(string[] args) {
			var item = get(args, 0, _definitions, null);
			if (item.Original != null)
				return item.Original;
			return item;
		}

		public DefinitionCacheItem GetBuiltIn(string[] args) {
			return get(args, 0, _builtIn, null);
		}
		
		public DefinitionCacheItem GetLanguage(string[] args) {
			return get(args, 0, _languages, null);
		}

		public DefinitionCacheItem GetLanguageScript(string[] args) {
			return get(args, 0, _languageScripts, null);
		}
		
		public DefinitionCacheItem GetScript(string[] args) {
			return get(args, 0, _scripts, null);
		}

		public void Merge(DefinitionCache cache) {
			foreach (var definition in cache.Definitions) {
				if (definition.Override)
					overrideItem(definition);
				else
					add(definition);
			}
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

		private DefinitionCacheItem add(DefinitionCacheItemType type, string location, DateTime updated, bool ovrride, bool required, string name, string description) {
			var item =
				new DefinitionCacheItem(parameterAppender) {
						Type = type,
						Location = location,
						Updated = updated,
						Override = ovrride,
						Required = required,
						Name = name,
						Description = description
					};
			if (item.Override) {
				_definitions.Add(item);
				return item;
			}
			return add(item);
		}

		private DefinitionCacheItem add(DefinitionCacheItem item) {
			addRaw(item);
			if (_definitions.Any(x => x.Name == item.Name)) {
				if (item.Location == "placehoder-for-language-in-different-location") {
					var original = _definitions.FirstOrDefault(x => x.Name == item.Name && x.Type == DefinitionCacheItemType.Language);
					overrideItem(item);
					return original;
				}
				_definitions.RemoveAll(x => x.Name == item.Name);
			}
			Logger.Write("Adding command " + item.Name);
			_definitions.Add(item);
			return item;
		}

		private void overrideItem(DefinitionCacheItem item) {
			if (item.Parameters.Length == 0)
				return;
			var existing = _definitions.FirstOrDefault(x => x.Name == item.Name);
			if (existing == null)
				return;
			overrideItem(existing, item);
		}

		private void overrideItem(DefinitionCacheItem existing, DefinitionCacheItem item) {
			if (item.Parameters.Length == 0)
				return;
			var childItem = item.Parameters[0];
			if (!childItem.Override) {
				existing.OverrideItem(childItem);
				return;
			}
			var child = existing.Parameters.FirstOrDefault(x => x.Name == childItem.Name);
			if (child == null)
				return;
			overrideItem(child, childItem);
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
			else if (item.Type == DefinitionCacheItemType.LanguageScript)
				_languageScripts.Add(item);
			else if (item.Type == DefinitionCacheItemType.Script)
				_scripts.Add(item);
		}
	}

	public enum DefinitionCacheItemType
	{
		Script,
		LanguageScript,
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
		public bool Override { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public DefinitionCacheItem Original { get; private set; }
		public DefinitionCacheItem[] Parameters { get { return _parameters.ToArray(); } }

		public DefinitionCacheItem(Func<List<DefinitionCacheItem>,DefinitionCacheItem,DefinitionCacheItem> parameterAppender) {
			Original = null;
			_parameterAppender = parameterAppender;
		}

		public DefinitionCacheItem Append(DefinitionCacheItemType type, string location, DateTime updated, bool ovrride, bool required, string name, string description) {
			return 
				_parameterAppender(
					_parameters,
					new DefinitionCacheItem(_parameterAppender) {
							Type = type,
							Location = location,
							Updated = updated, 
							Required = required,
							Override = ovrride,
							Name = name,
							Description = description
						});
		}

		public DefinitionCacheItem Append(DefinitionCacheItem parameter) {
			return _parameterAppender(_parameters, parameter);
		}

		public void OverrideItem(DefinitionCacheItem item) {
			var wasOverridden = false;
			for (int i = 0; i < _parameters.Count; i++) {
				if (_parameters[i].Name == item.Name) {
					item.Original = _parameters[i];
					_parameters[i] = item;
					wasOverridden = true;
					break;
				}
			}
			if (!wasOverridden)
				Append(item);
		}
	}
}
