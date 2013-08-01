using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Core.Scripts;
using OpenIDE.Bootstrapping;
using OpenIDE.Core.Profiles;
using OpenIDE.Core.Config;
using CoreExtensions;
namespace OpenIDE.Arguments.Handlers
{
	class EditorHandler : ICommandHandler
	{
		private string _rootPath;
		private OpenIDE.Core.EditorEngineIntegration.ILocateEditorEngine _editorFactory;
		private Func<PluginLocator> _pluginLocator;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"Initializes the environment by staring the editor, code model and editor engine");
				usage.Add("PLUGIN_NAME", "The name of the plugin to launch");
				usage.Add("goto", "Open file on spesific line and column")
					.Add("FILE|LINE|COLUMN", "| separated filepath, line and column");
				usage.Add("setfocus", "Sets focus to the editor");
				usage.Add("insert", "Inserts a chunk of text to a spesific position in a file")
					.Add("CONTENT_FILE", "File containing the text you want to insert")
						.Add("TARGET_LOCATION", "The target file and position to insert the text to. Format: /my/file.cs|LINE|COLUMN");
				usage.Add("remove", "Removes a chunk of text from a file")
					.Add("FILE_AND_START_POSITION", "The file and starting position of text to be removed. Format: /my/file.cs|LINE|COLUMN")
						.Add("END_POSITION", "The position where the text chunk to remove ends. Format: LINE|COLUMN");
				usage.Add("replace", "Replaces a chunk of text within a file")
					.Add("CONTENT_FILE", "File containing the text you want to replace with")
						.Add("FILE_AND_START_POSITION", "The file and starting position for text to be replaced. Format: /my/file.cs|LINE|COLUMN")
							.Add("END_POSITION", "The position where the text chunk to be replaced ends. Format: LINE|COLUMN");
				usage.Add("refactor", "Gives the posibility to batch up several insert, remove and replace commands")
					.Add("CONTENT_FILE", "A file containing insert, remove and replace commands. One command pr line");
				usage.Add("get-dirty-files", "Queries the editor for all modified files and their content")
					.Add("[FILE]", "If passed it will only respond with the file specified");
				usage.Add("command", "Custom editor commands");
				return usage;
			}
		}

		public string Command { get { return "editor"; } }
		
		public EditorHandler(string rootPath, OpenIDE.Core.EditorEngineIntegration.ILocateEditorEngine editorFactory, Func<PluginLocator> locator)
		{
			_rootPath = rootPath;
			_editorFactory = editorFactory;
			_pluginLocator = locator;
		}
		
		public void Execute(string[] arguments)
		{
			var instance = _editorFactory.GetInstance(_rootPath);
			// TODO remove that unbeleavable nasty setfocus solution. Only init if launching editor
			var isSetfocus = arguments.Length > 0 && arguments[0] == "setfocus";
			if (instance == null && arguments.Length >= 0 && !isSetfocus)
			{
				instance = startInstance();
				if (instance == null)
					return;
				var args = new List<string>();
				var configReader = new ConfigReader(_rootPath);
				if (arguments.Length == 0) {
					var name = configReader.Get("default.editor");
					if (name == null) {
						Console.WriteLine("To launch without specifying editor you must specify the default.editor config option");
						return;
					}
					args.Add(name);
				} else {
					args.AddRange(arguments);
				}
				var editorName = args[0];
				args.AddRange( 
					configReader	
						.GetStartingWith("editor." + editorName)
						.Select(x => "--" + x.Key + "=" + x.Value));
				var editor = instance.Start(args.ToArray());
				if (editor != null && editor != "")
					runInitScripts();
				else
					instance = null;
			}
			else if (arguments.Length >= 1 && arguments[0] == "get-dirty-files")
			{
				if (instance == null)
					return;
				string file = null;
				if (arguments.Length > 1)
					file = arguments[1];
				Console.WriteLine(instance.GetDirtyFiles(file));
			}
			else
			{
				if (instance == null)
					return;
				instance.Run(arguments);
			}
		}
		
		private OpenIDE.Core.EditorEngineIntegration.Instance startInstance()
		{
			var exe = Path.Combine(
				Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EditorEngine"),
				"EditorEngine.exe");
			var proc = new Process();
			proc.StartInfo = new ProcessStartInfo(exe, "\"" + _rootPath + "\"");
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.StartInfo.WorkingDirectory = _rootPath;
			proc.Start();
			var timeout = DateTime.Now.AddSeconds(5);
			while (DateTime.Now < timeout)
			{
				if (_editorFactory.GetInstance(_rootPath) != null)
					break;
				Thread.Sleep(50);
			}
			return _editorFactory.GetInstance(_rootPath);
		}

		private void runInitScripts()
		{
			var appdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			initCodeEngine(appdir);
			runInitScript(appdir);
			_pluginLocator().Locate().ToList()
				.ForEach(plugin => {
					var language = plugin.GetLanguage();
					runInitScript(Path.Combine(Path.GetDirectoryName(plugin.FullPath), language + "-files"));
				});
			var locator = new ProfileLocator(_rootPath);
			var profilePath = locator.GetLocalProfilePath(locator.GetActiveLocalProfile());
			runInitScript(profilePath);
		}

		private void initCodeEngine(string folder)
		{
			var defaultLanguage = getDefaultLanguage();
			if (defaultLanguage == null)
				defaultLanguage = "none-default-language";
			var enabledLanguages = getEnabledLanguages();
			if (enabledLanguages == null)
				enabledLanguages = "none-enabled-language";

			var cmd = "mono";
			var arg = "./CodeEngine/OpenIDE.CodeEngine.exe ";
			if (Environment.OSVersion.Platform != PlatformID.Unix &&
				Environment.OSVersion.Platform != PlatformID.MacOSX)
			{
				cmd = Path.Combine("CodeEngine", "OpenIDE.CodeEngine.exe");
				arg = "";
			}
			
			var proc = new Process();
			proc.StartInfo = new ProcessStartInfo(
				cmd,
				arg + "\"" + _rootPath + "\"" + defaultLanguage + enabledLanguages);
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.StartInfo.WorkingDirectory = folder;
			proc.Start();
		}
	
		private void runInitScript(string folder)
		{
			if (!Directory.Exists(folder))
				return;
			var initscript = 
				new ScriptFilter()
					.FilterScripts(Directory.GetFiles(folder, "initialize.*"))
					.FirstOrDefault();
			if (initscript == null)
				return;
			var defaultLanguage = getDefaultLanguage();
			var enabledLanguages = getEnabledLanguages();
			var proc = new Process();
			proc
				.Spawn(
					initscript,
					"\"" + _rootPath + "\"" + defaultLanguage + enabledLanguages,
					false,
					folder);
		}

		private string getDefaultLanguage()
		{
			var defaultLanguage = "";
			if (Bootstrapper.Settings.DefaultLanguage != null)
				defaultLanguage = " " + Bootstrapper.Settings.DefaultLanguage;
			return defaultLanguage;
		}

		private string getEnabledLanguages()
		{
			var enabledLanguages = "";
			if (Bootstrapper.Settings.EnabledLanguages != null && Bootstrapper.Settings.EnabledLanguages.Length > 0)
			{
				enabledLanguages = " \"";
				Bootstrapper.Settings.EnabledLanguages.ToList()
					.ForEach(x => enabledLanguages += x + ",");
				enabledLanguages = 
					enabledLanguages
						.Substring(0, enabledLanguages.Length - 1) + "\"";
			}
			return enabledLanguages;
		}
	}
}

