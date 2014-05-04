using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenIDE.Core.Definitions;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Language;
using OpenIDE.Core.Logging;
using OpenIDE.Core.Profiles;
using OpenIDE.Core.Scripts;

namespace OpenIDE.Core.Definitions
{
	public class BuiltInCommand
	{
		public string Name { get; private set; }
		public CommandHandlerParameter Usage { get; private set; }

		public BuiltInCommand(string name, CommandHandlerParameter usage) {
			Name = name;
			Usage = usage;
		}
	}

	public class DefinitionBuilder
	{
		private string _token;
		private string _workingDirectory;
		private string _defaultLanguage;
		private string[] _enabledLanguages;
		private DefinitionCache _cache = new DefinitionCache();
		private Func<IEnumerable<BuiltInCommand>> _builtIn;
		private Func<string,IEnumerable<LanguagePlugin>> _languages;

		public IEnumerable<DefinitionCacheItem> Definitions { get { return _cache.Definitions; } }

		public DefinitionBuilder(
			string token,
			string workingDirectory,
			string defaultLanguage,
			string[] enabledLanguages,
			Func<IEnumerable<BuiltInCommand>> builtIn,
			Func<string,IEnumerable<LanguagePlugin>> languages)

		{
			_token = token;
			_workingDirectory = workingDirectory;
			_defaultLanguage = defaultLanguage;
			_enabledLanguages = enabledLanguages;
			_builtIn = builtIn;
			_languages = languages;
		}

		public DefinitionCacheItem Get(string[] args) {
			return _cache.Get(args);
		}

		public DefinitionCacheItem GetOriginal(string[] args) {
			return _cache.GetOriginal(args);
		}

		public DefinitionCacheItem GetBuiltIn(string[] args) {
			return _cache.GetBuiltIn(args);
		}
		
		public DefinitionCacheItem GetLanguage(string[] args) {
			return _cache.GetLanguage(args);
		}
		
		public DefinitionCacheItem GetLanguageScript(string[] args) {
			return _cache.GetLanguageScript(args);
		}

		public DefinitionCacheItem GetScript(string[] args) {
			return _cache.GetScript(args);
		}

		public void Build() {
			_cache = new DefinitionCache();
			var profiles = new ProfileLocator(_token);

			mergeBuiltInCommands(profiles);
			// Reverse paths to handle global first then local
			var paths = profiles.GetPathsCurrentProfiles().Reverse().ToArray();
			foreach (var path in paths)
				mergeExternalCommands(path);

			// Add default language
			if (_defaultLanguage != null) {
				var lang = _cache.Get(new[] { _defaultLanguage });
				if (lang != null) {
					var parameters = lang.Parameters;
					foreach (var usage in parameters) {
						// Don't override existing commands with default language
						if (_cache.Get(new[] { usage.Name }) == null) {
							_cache.Add(usage);
						}
					}
				}
			}
		}

		private void mergeBuiltInCommands(ProfileLocator profiles) {
			var builtInFile = Path.Combine(profiles.AppRootPath, "oi-definitions.json");
			var builtInCache = new DefinitionCache();
			if (File.Exists(builtInFile)) {
				builtInCache = appendDefinitions(builtInFile);
				var updated = builtInCache.GetOldestItem();
				if (updated != null && fileTime(System.Reflection.Assembly.GetExecutingAssembly().Location) > updated.Updated)
					builtInCache = writeBuiltInCommands(builtInFile, profiles);
			} else {
				Logger.Write("Could not find definition file: " + builtInFile);
				builtInCache = writeBuiltInCommands(builtInFile, profiles);
			}
			_cache.Merge(_enabledLanguages, builtInCache);
		}

		private void mergeExternalCommands(string path) {
			Logger.Write("Merging path " + path);
			var localCache = new DefinitionCache();
			var file = Path.Combine(path, "oi-definitions.json");
			if (File.Exists(file)) {
				localCache = appendDefinitions(file);
				if (cacheIsOutOfDate(file, localCache)) {
					Logger.Write("Definition file was out of date, updating " + file);
					localCache = buildDefinitions(file);
				}
			} else {
				Logger.Write("Could not find definition file: " + file);
				localCache = buildDefinitions(file);
			}
			
			_cache.Merge(_enabledLanguages, localCache);
		}

		private DefinitionCache writeBuiltInCommands(string file, ProfileLocator profiles) {
			var builtInCache = new DefinitionCache();
			foreach (var cmd in _builtIn()) {
				var item = builtInCache	
					.Add(
						DefinitionCacheItemType.BuiltIn,
						null,
						DateTime.Now,
						false,
						true,
						cmd.Name,
						cmd.Usage.Description);
				add(builtInCache, item, cmd.Usage.Parameters);
			}
			writeCache(profiles.AppRootPath, builtInCache);
			return builtInCache;
		}

		private DateTime fileTime(string file) {
			var time = new FileInfo(file).LastWriteTime;
			var writeTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
			if (writeTime > DateTime.Now) {
				Logger.Write("Updating write time for " + file);
				File.SetLastWriteTime(file, DateTime.Now);
				writeTime = DateTime.Now;
			}
			return writeTime;
		}

		private void writeCache(string path, DefinitionCache cache) {
			new DefinitionCacheWriter(path)
				.Write(cache);
		}

		private DefinitionCache appendDefinitions(string file) {
			return new DefinitionCacheReader().Read(file);
		}

		private DefinitionCache buildDefinitions(string file) {
			var cache = new DefinitionCache();
			var dir = Path.GetDirectoryName(file);
			
			// Add languages
			var languagePath = Path.Combine(dir, "languages");
			LanguagePlugin defaultLanguage = null;
			foreach (var language in _languages(languagePath)) {
				var item = cache.Add(
					DefinitionCacheItemType.Language,
					language.FullPath,
					DateTime.Now,
					false,
					true,
					language.GetLanguage(),
					"Commands for the " + language.GetLanguage() + " plugin");
				add(cache, item, language.GetUsages());
			}

			// Add language scripts
			var currentLanguages = cache
				.Definitions
				.Where(x => x.Type == DefinitionCacheItemType.Language)
				.ToList();
			var otherLocationLanguages = _cache
				.Definitions
				.Where(x => x.Type == DefinitionCacheItemType.Language && !currentLanguages.Any(y => y.Name == x.Name))
				.ToList();
			foreach (var item in currentLanguages) {
				var languageScriptPath = 
					Path.Combine(
						Path.Combine(
							languagePath, item.Name + "-files"),
							"scripts");
				if (!Directory.Exists(languageScriptPath))
					continue;
				Logger.Write("Adding scripts from " + languageScriptPath);
				var languageScripts = new ScriptFilter().GetScripts(languageScriptPath);
				foreach (var scriptFile in languageScripts) {
					Logger.Write("Script " + scriptFile);
					var script  = new Script(_token, _workingDirectory, scriptFile);
					var usages = script.Usages; // Description is built when fetching usages
					var scriptItem = item.Append(
						DefinitionCacheItemType.LanguageScript,
						scriptFile,
						DateTime.Now,
						false,
						true,
						script.Name,
						script.Description);
					add(cache, scriptItem, usages);
				}
			}
			foreach (var language in otherLocationLanguages) {
				var languageScriptPath = 
					Path.Combine(
						Path.Combine(
							languagePath, language.Name + "-files"),
							"scripts");
				if (!Directory.Exists(languageScriptPath))
					continue;
				var item = cache.Add(
					DefinitionCacheItemType.Language,
					"placehoder-for-language-in-different-location",
					DateTime.Now,
					false,
					true,
					language.Name,
					"");
				add(cache, item, new BaseCommandHandlerParameter[] {});
				Logger.Write("Adding scripts from " + languageScriptPath);
				var languageScripts = new ScriptFilter().GetScripts(languageScriptPath);
				foreach (var scriptFile in languageScripts) {
					Logger.Write("Script " + scriptFile);
					var script  = new Script(_token, _workingDirectory, scriptFile);
					var usages = script.Usages; // Description is built when fetching usages
					var scriptItem = item.Append(
						DefinitionCacheItemType.LanguageScript,
						scriptFile,
						DateTime.Now,
						false,
						true,
						script.Name,
						script.Description);
					add(cache, scriptItem, usages);
				}
			}

			// Add scripts
			var scriptPath = Path.Combine(dir, "scripts");
			Logger.Write("Adding scripts from " + scriptPath);
			var scripts = new ScriptFilter().GetScripts(scriptPath);
			foreach (var scriptFile in scripts) {
				Logger.Write("Adding script " + scriptPath);
				var script  = new Script(_token, _workingDirectory, scriptFile);
				var usages = script.Usages; // Description is built when fetching usages
				var item = cache.Add(
					DefinitionCacheItemType.Script,
					scriptFile,
					DateTime.Now,
					false,
					true,
					script.Name,
					script.Description);
				add(cache, item, usages);
			}

			writeCache(dir, cache);
			return cache;
		}

		private void add(DefinitionCache cache, DefinitionCacheItem item, IEnumerable<DefinitionCacheItem> parameters) {
			foreach (var parameter in parameters)
				add(cache, item, parameter);
		}

		private void add(DefinitionCache cache, DefinitionCacheItem item, DefinitionCacheItem parameter) {
			var name = parameter.Name;
			var child =
				item.Append(
						item.Type,
						item.Location,
						item.Updated,
						parameter.Override,
						parameter.Required,
						name,
						parameter.Description);
			foreach (var cmd in parameter.Parameters)
				add(cache, child, cmd);
		}

		private void add(DefinitionCache cache, DefinitionCacheItem item, IEnumerable<BaseCommandHandlerParameter> parameters) {
			foreach (var parameter in parameters) {
				if (parameter.Override)
					overrideCommand(cache, item, parameter);
				else
					add(cache, item, parameter);
			}
		}

		private void add(DefinitionCache cache, DefinitionCacheItem item, BaseCommandHandlerParameter parameter) {
			var name = parameter.Name;
			var child =
				item.Append(
						item.Type,
						item.Location,
						item.Updated,
						parameter.Override,
						parameter.Required,
						name,
						parameter.Description);
			foreach (var cmd in parameter.Parameters)
				add(cache, child, cmd);
		}

		private void overrideCommand(DefinitionCache cache, DefinitionCacheItem item, BaseCommandHandlerParameter parameter) {
			var command = cache.Add(
				item.Type,
				item.Location,
				item.Updated,
				parameter.Override,
				parameter.Required,
				parameter.Name,
				parameter.Description);
			foreach (var cmd in parameter.Parameters)
				add(cache, command, cmd);
		}

		private bool cacheIsOutOfDate(string file, DefinitionCache cache) {
			try {
				var dir = Path.GetDirectoryName(file);
				var locations = cache.GetLocations(DefinitionCacheItemType.Script).Select(x => x.Location);
				var scriptPath = Path.Combine(dir, "scripts");
				var scripts = new ScriptFilter().GetScripts(scriptPath);
				if (scripts.Any(x => !locations.Contains(x))) {
					Logger.Write("New script has been added");
					return true;
				}
				if (locations.Any(x => !scripts.Contains(x))) {
					Logger.Write("New script has been added");
					return true;
				}
				
				foreach (var script in scripts) {
					if (isUpdated(script, cache))
						return true;
				}
				
				var languagePath = Path.Combine(dir, "languages");
				var rawLocations = cache.GetLocations(DefinitionCacheItemType.Language);
				locations = replacePlaceholderLanguages(languagePath, rawLocations);
				var languages = _languages(languagePath).Select(x => x.FullPath).ToList();
				languages = addPlaceholderLanguages(languagePath, languages);
				if (languages.Any(x => !locations.Contains(x))) {
					Logger.Write("New language has been added");
					if (Logger.IsEnabled) {
						foreach (var newLanguage in languages.Where(x => !locations.Contains(x)))
							Logger.Write("\t" + newLanguage);
					}
					return true;
				}
				if (locations.Any(x => !languages.Any(y => y == x))) {
					Logger.Write("Language has been removed");
					return true;
				}

				foreach (var language in languages) {
					if (isUpdated(language, cache))
						return true;
					var languageScriptPath = 
						Path.Combine(
							Path.Combine(languagePath, Path.GetFileNameWithoutExtension(language) + "-files"),
							"scripts");
					if (Directory.Exists(languageScriptPath)) {
						locations = cache
							.GetLocations(DefinitionCacheItemType.LanguageScript)
							.Select(x => x.Location)
							.Where(x => x.StartsWith(languageScriptPath))
							.ToArray();
						var languageScripts = new ScriptFilter().GetScripts(languageScriptPath);
						if (languageScripts.Any(x => !locations.Contains(x))) {
							Logger.Write("Language script has been added");
							return true;
						}
						if (locations.Any(x => !languageScripts.Contains(x))) {
							Logger.Write("Language script has been removed");
							return true;
						}
						foreach (var script in languageScripts) {
							if (isUpdated(script, cache))
								return true;
						}
					}
				}
			} catch (Exception ex) {
				Logger.Write(ex.ToString());
				return true;
			}
			return false;
		}

		private string[] replacePlaceholderLanguages(string path, DefinitionCache.DefinitionLocation[] locations) {
			var result = new List<string>();
			foreach (var location in locations) {
				if (location.Location == "placehoder-for-language-in-different-location")
					result.Add(Path.Combine(path, location.Name));
				else
					result.Add(location.Location);
			}
			return result.ToArray();
		}

		private List<string> addPlaceholderLanguages(string path, List<string> existing) {
			if (!Directory.Exists(path))
				return existing;
			var existingShort = existing.Select(x => 
				Path.Combine(
					Path.GetDirectoryName(x),
					Path.GetFileNameWithoutExtension(x) + "-files"));
			foreach (var dir in Directory.GetDirectories(path)) {
				if (!dir.EndsWith("-files"))
					continue;
				if (existingShort.Contains(dir))
					continue;
				existing.Add(dir.Substring(0, dir.Length - 6));
			}
			return existing;
		}

		private bool isUpdated(string file, DefinitionCache cache) {
			var oldest = cache.GetOldestItem(file);
			// This might be null on language placeholders
			if (oldest == null)
				return false;
			var updated = oldest.Updated;
			// This is a hack to check placeholder languages naturally
			// the language file for a placeholder language will not
			// exist
			if (!File.Exists(file))
				return false;
			var filetime = fileTime(file);
			if (filetime > updated) {
				Logger.Write("Oldest is {0} {1} for {2}", updated.ToShortDateString(), updated.ToLongTimeString(), file);
				Logger.Write("Definition is out of date for {0} with file time {1} {2}", file, filetime.ToShortDateString(), filetime.ToLongTimeString());
				return true;
			}

			var dir = Path.GetDirectoryName(file);
			var name = Path.GetFileNameWithoutExtension(file);
			var filesDir = Path.Combine(dir, name + "-files");
			if (Directory.Exists(filesDir)) {
				return isUpdated(updated, filesDir, Path.Combine(filesDir, "state"));
			}
			return false;
		}

		private bool isUpdated(DateTime updated, string dir, string stateDir) {
			foreach (var file in Directory.GetFiles(dir)) {
				var filetime = fileTime(file);
				if (filetime > updated) {
					Logger.Write("Oldest is {0} {1} for {2}", updated.ToShortDateString(), updated.ToLongTimeString(), file);
					Logger.Write("Definition is out of date for {0} with file time {1} {2}", file, filetime.ToShortDateString(), filetime.ToLongTimeString());
					return true;
				}
			}
			foreach (var subDir in Directory.GetDirectories(dir)) {
				if (subDir == stateDir)
					continue;
				if (isUpdated(updated, subDir, stateDir))
					return true;
			}
			return false;
		}
	}
}