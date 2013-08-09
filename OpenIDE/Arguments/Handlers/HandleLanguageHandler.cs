using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Bootstrapping;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.RScripts;
using OpenIDE.Core.Commands;
using OpenIDE.Core.Language;
using OpenIDE.Core.Definitions;
using OpenIDE.Core.Config;
using CoreExtensions;

namespace OpenIDE.Arguments.Handlers
{
	class HandleLanguageHandler : ICommandHandler
	{
		private string _token;
		private Action<string> _dispatch;
		private PluginLocator _locator;

		public CommandHandlerParameter Usage { 
			get {
				var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Administer laungage plugins");
				var scripts = usage.Add("script", "List all language scripts");
				scripts.Add("LANGUAGE_NAME", "Language to list scripts for");
				scripts
					.Add("new", "Create new script")
						.Add("LANGUAGE_NAME", "Language containing script")
							.Add("SCRIPT_NAME", "Script name with file extension.");
				scripts
					.Add("edit", "Open script in the editor")
						.Add("LANGUAGE_NAME", "Language containing script")
							.Add("SCRIPT_NAME", "Script name");
				scripts
					.Add("cat", "Print script to the terminal")
						.Add("LANGUAGE_NAME", "Language containing script")
							.Add("SCRIPT_NAME", "Script name");
				scripts
					.Add("rm", "Delete script")
						.Add("LANGUAGE_NAME", "Language containing script")
							.Add("SCRIPT_NAME", "Script name");

				var rscripts = usage.Add("rscript", "List all language reactive scripts");
				rscripts.Add("LANGUAGE_NAME", "Language to list reactive scripts for");
				rscripts
					.Add("new", "Create new reactive script")
						.Add("LANGUAGE_NAME", "Language containing reactive script")
							.Add("SCRIPT_NAME", "Reactive script name with file extension");
				rscripts
					.Add("edit", "Open reactive script in the editor")
						.Add("LANGUAGE_NAME", "Language containing reactive script")
							.Add("SCRIPT_NAME", "Reactive script name");
				rscripts
					.Add("cat", "Print reactive script to the terminal")
						.Add("LANGUAGE_NAME", "Language containing reactive script")
							.Add("SCRIPT_NAME", "Reactive script name");
				rscripts
					.Add("rm", "Delete reactive script")
						.Add("LANGUAGE_NAME", "Language containing reactive script")
							.Add("SCRIPT_NAME", "Reactive script name");
				return usage;
			}
		}

		public string Command { get { return "language"; } }

		public HandleLanguageHandler(string token, Action<string> dispatch, PluginLocator locator) {
			_token = token;
			_dispatch = dispatch;
			_locator = locator;
		}

		public void Execute(string[] args)
		{
			if (args.Length == 0)
				listLanguages();
			else if (args.Length > 0 && args[0] == "script")
				handleScript(args);
			else if (args.Length > 0 && args[0] == "rscript")
				handleRScript(args);
		}

		private void listLanguages() {
			Console.WriteLine("Available language plugins:");
			foreach (var language in _locator.Locate()) {
				Console.WriteLine("\t" + language.GetLanguage());
			}
		}

		private void handleScript(string[] args) {
			if (args.Length == 1)
				listScript();
			else if (args.Length == 4 && args[1] == "new")
				newScript(args);
			else if (args.Length == 4 && args[1] == "edit")
				editScript(args);
			else if (args.Length == 4 && args[1] == "cat")
				catScript(args);
			else if (args.Length == 4 && args[1] == "rm")
				rmScript(args);
			else if (args.Length == 2)
				listScript(args[1]);
		}

		private void newScript(string[] args) {
			var language = getLanguage(args[2]);
			if (language == null)
				return;
			var filename = getFileName(args[3]);
			if (filename == null)
				return;
			var extension = getExtension(args[3]);
			var path = getLanguagePath(language, "scripts");
			if (path == null)
				return;
			var file = Path.Combine(
				path,
				filename);
			if (extension != null)
				file += extension;
			if (File.Exists(file))
				return;
			PathExtensions.CreateDirectories(file);
			var template = new ScriptLocator(_token, Environment.CurrentDirectory).GetTemplateFor(extension);
			var content = "";
			if (template != null)
				File.Copy(template, file);
			else
			{
				var templates = new ScriptLocator(_token, Environment.CurrentDirectory).GetTemplates().ToArray();
				if (templates.Length == 0)
				{
					File.WriteAllText(file, content);
					if (Environment.OSVersion.Platform == PlatformID.Unix ||
						Environment.OSVersion.Platform == PlatformID.MacOSX)
						run("chmod", "+x \"" + file + "\"");
				}
				else
				{
					File.WriteAllText(file, "");
					File.Copy(templates[0], file);
				}
			}
			_dispatch("command|editor goto \"" + file + "|0|0\"");
		}

		private void editScript(string[] args) {
			var script = scriptFromLanguageAndScriptName(args[2], args[3]);
			if (script == null)
				return;
			_dispatch(string.Format("command|editor goto \"{0}|0|0\"", script.Location));
		}

		private void catScript(string[] args) {
			var script = scriptFromLanguageAndScriptName(args[2], args[3]);
			if (script == null)
				return;
			Console.WriteLine(File.ReadAllText(script.Location));
		}

		private void rmScript(string[] args) {
			var script = scriptFromLanguageAndScriptName(args[2], args[3]);
			if (script == null)
				return;
			deleteScript(script.Name, script.Location);
		}

		private DefinitionCacheItem scriptFromLanguageAndScriptName(string languageName, string scriptName) {
			var language = getLanguage(languageName);
			if (language == null)
				return null;
			var scripts = new List<DefinitionCacheItem>();
			listScript(
				languageName,
				(l) => {},
				(scr) => scripts.Add(scr));
			return scripts.FirstOrDefault(x => x.Name == scriptName);
		}

		private void listScript() {
			listScript(null);
		}

		private void listScript(string languageName) {
			listScript(
				languageName,
				(l) => Console.WriteLine("Available scripts for {0}:", l.GetLanguage()),
				(d) => UsagePrinter.PrintDefinition(d));
		}

		private void listScript(string languageName, Action<LanguagePlugin> onLanguage, Action<DefinitionCacheItem> onItem) {
			var definitions = 
				Bootstrapper.GetDefinitionBuilder()
					.Definitions
					.Where(x => x.Type == DefinitionCacheItemType.LanguageScript)
					.ToList();
			itemsPrLanguage(
				onLanguage,
				onItem,
				(d) => d.Location,
				definitions,
				languageName,
				"scripts");
		}
		
		private void handleRScript(string[] args) {
			if (args.Length == 1)
				listRScript();
			else if (args.Length == 4 && args[1] == "new")
				newRScript(args);
			else if (args.Length == 4 && args[1] == "edit")
				editRScript(args);
			else if (args.Length == 4 && args[1] == "cat")
				catRScript(args);
			else if (args.Length == 4 && args[1] == "rm")
				rmRScript(args);
			else if (args.Length == 2)
				listRScript(args[1]);
		}

		private void newRScript(string[] args) {
			var language = getLanguage(args[2]);
			if (language == null)
				return;
			var filename = getFileName(args[3]);
			if (filename == null)
				return;
			var extension = getExtension(args[3]);
			var path = getLanguagePath(language, "rscripts");
			if (path == null)
				return;
			var file = Path.Combine(
				path,
				filename);
			if (extension != null)
				file += extension;
			if (File.Exists(file))
				return;
			PathExtensions.CreateDirectories(file);
			var template = new ReactiveScriptLocator(_token, Environment.CurrentDirectory).GetTemplateFor(extension);
			var content = "";
			if (template != null)
				File.Copy(template, file);
			else 
			{
				var templates = new ReactiveScriptLocator(_token, Environment.CurrentDirectory).GetTemplates().ToArray();
				if (templates.Length == 0)
				{
					File.WriteAllText(file, content);
					if (Environment.OSVersion.Platform == PlatformID.Unix ||
						Environment.OSVersion.Platform == PlatformID.MacOSX)
						run("chmod", "+x \"" + file + "\"");
				}
				else
				{
					File.WriteAllText(file, "");
					File.Copy(templates[0], file);
				}
			}

			_dispatch("command|editor goto \"" + file + "|0|0\"");
		}

		private void editRScript(string[] args) {
			var script = reactiveScriptFromLanguageAndScriptName(args[2], args[3]);
			if (script == null)
				return;
			_dispatch(string.Format("command|editor goto \"{0}|0|0\"", script.File));
		}

		private void catRScript(string[] args) {
			var script = reactiveScriptFromLanguageAndScriptName(args[2], args[3]);
			if (script == null)
				return;
			Console.WriteLine(File.ReadAllText(script.File));
		}

		private void rmRScript(string[] args) {
			var script = reactiveScriptFromLanguageAndScriptName(args[2], args[3]);
			if (script == null)
				return;
			deleteScript(script.Name, script.File);
		}

		private ReactiveScript reactiveScriptFromLanguageAndScriptName(string languageName, string scriptName) {
			var language = getLanguage(languageName);
			if (language == null)
				return null;
			var scripts = new List<ReactiveScript>();
			listRScript(
				languageName,
				(l) => {},
				(scr) => scripts.Add(scr));
			return scripts.FirstOrDefault(x => x.Name == scriptName);
		}

		private void listRScript() {
			listRScript(null);
		}

		private void listRScript(string languageName) {
				listRScript(
				languageName,
				(l) => Console.WriteLine("Available reactive scripts for {0}:", l.GetLanguage()),
				(d) => Console.WriteLine("\t" + d.Name));
		}

		private void listRScript(string languageName, Action<LanguagePlugin> onLanguage, Action<ReactiveScript> onItem) {
			var scripts = 
				new ReactiveScriptReader(
					_token,
					() => { return _locator; },
					(m) => {})
					.ReadLanguageScripts();
			itemsPrLanguage(
				onLanguage,
				onItem,
				(d) => d.File,
				scripts,
				languageName,
				"rscripts");
		}

		private void deleteScript(string name, string path) {
			if (File.Exists(path))
				File.Delete(path);
			var dir = 
				Path.Combine(
					Path.GetDirectoryName(path),
					name + "-files");
			if (Directory.Exists(dir))
				Directory.Delete(dir, true);
		}

		private LanguagePlugin getLanguage(string languageName) {
			return 
				_locator
					.Locate()
					.FirstOrDefault(x => x.GetLanguage() == languageName);
		}

		private void itemsPrLanguage<T>(Action<LanguagePlugin> onLanguage, Action<T> onItem, Func<T,string> pathExtractor, List<T> items, string languageName, string type) {
			foreach (var language in _locator.Locate()) {
				if (languageName != null && language.GetLanguage() != languageName)
					continue;
				var path = getLanguagePath(language, type);
				var languageItems = 
					items
						.Where(x => pathExtractor(x).StartsWith(path)).ToArray();
				if (languageItems.Length > 0) {
					onLanguage(language);
					foreach (var item in languageItems)
						onItem(item);
				}
			}
		}
		
		private string getLanguagePath(LanguagePlugin language, string dir) {
			return
				Path.Combine(
					Path.GetDirectoryName(language.FullPath),
					Path.Combine(
						language.GetLanguage() + "-files",
						dir));
		}

		private string getFileName(string name) {
			var extension = getExtension(name);
			if (extension == null)
				return name;
			else
				return name.Substring(0, name.Length - extension.Length);
		}

		private string getExtension(string name) {
			return Path.GetExtension(name);
		}

		private void run(string cmd, string arguments) {
			try {
				var proc = new Process();
                proc.Run(cmd, arguments, false, Environment.NewLine);
			} catch {
			}
		}
	}
}