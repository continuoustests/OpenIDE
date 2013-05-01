using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Core.Profiles;
using OpenIDE.Core.Definitions;
using OpenIDE.Core.FileSystem;
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
		private DefinitionCache _cache = new DefinitionCache();
		private Func<IEnumerable<BuiltInCommand>> _builtIn;
		private Func<string,IEnumerable<LanguagePlugin>> _languages;

		public IEnumerable<DefinitionCacheItem> Definitions { get { return _cache.Definitions; } }

		public DefinitionBuilder(
			string token,
			string workingDirectory,
			string defaultLanguage,
			Func<IEnumerable<BuiltInCommand>> builtIn,
			Func<string,IEnumerable<LanguagePlugin>> languages)

		{
			_token = token;
			_workingDirectory = workingDirectory;
			_defaultLanguage = defaultLanguage;
			_builtIn = builtIn;
			_languages = languages;
		}

		public DefinitionCacheItem Get(string[] args) {
			return _cache.Get(args);
		}

		private DateTime _now;
		public void Build() {
			_cache = new DefinitionCache();
			var profiles = new ProfileLocator(_token);

			// Loop reversed to handle local profile first
			// because of overriding definitions
			var paths = profiles.GetPathsCurrentProfiles().ToArray();
			for (int i = paths.Length - 1; i >= 0; i--)
				mergeExternalCommands(paths[i]);
			mergeBuiltInCommands(profiles);
		}

		private void mergeExternalCommands(string path) {
			var localCache = new DefinitionCache();
			var file = Path.Combine(path, "oi-definitions.json");
			if (File.Exists(file)) {
				localCache = appendDefinitions(file);
				if (cacheIsOutOfDate(file, localCache))
					localCache = buildDefinitions(file);
			} else {
				localCache = buildDefinitions(file);
			}
			
			_cache.Merge(localCache);
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
				builtInCache = writeBuiltInCommands(builtInFile, profiles);
			}
			_cache.Merge(builtInCache);
		}

		private DefinitionCache writeBuiltInCommands(string file, ProfileLocator profiles) {
			var builtInCache = new DefinitionCache();
			foreach (var cmd in _builtIn()) {
				builtInCache	
					.Add(
						DefinitionCacheItemType.BuiltIn,
						null,
						DateTime.Now,
						cmd.Name,
						cmd.Usage.Description,
						cmd.Usage.Parameters);
			}
			writeCache(profiles.AppRootPath, builtInCache);
			return builtInCache;
		}

		private DateTime fileTime(string file) {
			var time = new FileInfo(file).LastWriteTime;
			return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
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

			// Add scripts
			var scriptPath = Path.Combine(dir, "scripts");
			var scripts = new ScriptFilter().GetScripts(scriptPath);
			foreach (var scriptFile in scripts) {
				var script  = new Script(_token, _workingDirectory, scriptFile);
				var usages = script.Usages; // Description is built when fetching usages
				cache.Add(
					DefinitionCacheItemType.Script,
					scriptFile,
					DateTime.Now,
					script.Name,
					script.Description,
					usages);
			}

			// Add languages
			var languagePath = Path.Combine(dir, "languages");
			LanguagePlugin defaultLanguage = null;
			foreach (var language in _languages(languagePath)) {
				cache.Add(
					DefinitionCacheItemType.Language,
					language.FullPath,
					DateTime.Now,
					language.GetLanguage(),
					"Commands for the " + language.GetLanguage() + " plugin",
					language.GetUsages());
				if (language.GetLanguage() == _defaultLanguage)
					defaultLanguage = language;
			}

			// Add default language
			if (defaultLanguage != null) {
				foreach (var usage in defaultLanguage.GetUsages()) {
					cache.Add(
						DefinitionCacheItemType.Language,
						defaultLanguage.FullPath,
						DateTime.Now,
						usage.Name,
						usage.Description,
						usage.Parameters);
				}
			}
			writeCache(dir, cache);
			return cache;
		}

		private bool cacheIsOutOfDate(string file, DefinitionCache cache) {
			var isOutOfDate = false;
			var dir = Path.GetDirectoryName(file);
			var locations = cache.GetLocations(DefinitionCacheItemType.Script);
			var scriptPath = Path.Combine(dir, "scripts");
			var scripts = new ScriptFilter().GetScripts(scriptPath);
			isOutOfDate = scripts.Any(x => !locations.Contains(x));
			if (!isOutOfDate)
				isOutOfDate = locations.Any(x => !scripts.Contains(x));
			if (!isOutOfDate) {
				foreach (var script in scripts) {
					if (isUpdated(script, cache)) {
						isOutOfDate = true;
						break;
					}
				}
			}
			
			if (!isOutOfDate) {
				locations = cache.GetLocations(DefinitionCacheItemType.Language);
				var languagePath = Path.Combine(dir, "languages");
				var languages = _languages(languagePath);
				isOutOfDate = languages.Any(x => !locations.Contains(x.FullPath));
				if (!isOutOfDate)
					isOutOfDate = locations.Any(x => !languages.Any(y => y.FullPath == x));
				if (!isOutOfDate) {
					foreach (var language in languages) {
						if (isUpdated(language.FullPath, cache)) {
							isOutOfDate = true;
							break;
						}
					}
				}
			}
			return isOutOfDate;
		}

		private bool isUpdated(string file, DefinitionCache cache) {
			var updated = cache.GetOldestItem(file).Updated;
			if (fileTime(file) > updated)
				return true;

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
				if (fileTime(file) > updated)
					return true;
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