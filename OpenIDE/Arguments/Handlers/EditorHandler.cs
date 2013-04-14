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
		
		public EditorHandler(OpenIDE.Core.EditorEngineIntegration.ILocateEditorEngine editorFactory, Func<PluginLocator> locator)
		{
			_editorFactory = editorFactory;
			_pluginLocator = locator;
		}
		
		public void Execute(string[] arguments)
		{
			var instance = _editorFactory.GetInstance(Environment.CurrentDirectory);
			// TODO remove that unbeleavable nasty setfocus solution. Only init if launching editor
			if (instance == null && arguments.Length > 0 && arguments[0] != "setfocus")
			{
				instance = startInstance();
				if (instance == null)
					return;
				var args = new List<string>();
				args.AddRange(arguments);
				args.AddRange( 
					new Configuration(Environment.CurrentDirectory, true)
						.EditorSettings
						.Where(x => x.StartsWith("editor." + arguments[0]))
						.Select(x => "--" + x));
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
			var assembly = 
				Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					Path.Combine("EditorEngine", "EditorEngine.exe"));
			var exe = "mono";
			var arg = assembly + " ";
			if (Environment.OSVersion.Platform != PlatformID.Unix &&
				Environment.OSVersion.Platform != PlatformID.MacOSX)
			{
				exe = assembly;
				arg = "";
			}
			arg += "\"" + Environment.CurrentDirectory + "\"";
			var proc = new Process();
			proc.StartInfo = new ProcessStartInfo(exe, arg);
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
			proc.Start();
			var timeout = DateTime.Now.AddSeconds(5);
			while (DateTime.Now < timeout)
			{
				if (_editorFactory.GetInstance(Environment.CurrentDirectory) != null)
					break;
				Thread.Sleep(50);
			}
			return _editorFactory.GetInstance(Environment.CurrentDirectory);
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
			var locator = new ProfileLocator(Environment.CurrentDirectory);
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
				arg + "\"" + Environment.CurrentDirectory + "\"" + defaultLanguage + enabledLanguages);
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
					"\"" + Environment.CurrentDirectory + "\"" + defaultLanguage + enabledLanguages,
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

