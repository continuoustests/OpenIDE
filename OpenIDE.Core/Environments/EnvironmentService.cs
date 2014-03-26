using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using OpenIDE.Core.Config;
using OpenIDE.Core.Scripts;
using OpenIDE.Core.Logging;
using OpenIDE.Core.Language;
using OpenIDE.Core.Profiles;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.EditorEngineIntegration;
using CoreExtensions;

namespace OpenIDE.Core.Environments
{
	public class EnvironmentService
	{
		private string _defaultLanguage;
		private string[] _enabledLanguages;
		private ICodeEngineLocator _codeEnginelocator;
		private ILocateEditorEngine _editorLocator;
		private Func<PluginLocator> _pluginLocator;

		public EnvironmentService(string defaultLanguage, string[] enabledLanguages, Func<PluginLocator> pluginLocator, ICodeEngineLocator codeEnginelocator, ILocateEditorEngine editorLocator)
		{
			_defaultLanguage = defaultLanguage;
			_enabledLanguages = enabledLanguages;
			_pluginLocator = pluginLocator;
			_codeEnginelocator = codeEnginelocator;
			_editorLocator = editorLocator;
		}

		public bool IsRunning(string token)
		{
			return _codeEnginelocator.GetInstance(token) != null;
		}

		public bool HasEditorEngine(string token)
		{
			return _editorLocator.GetInstance(token) != null;
		}

		public void Start(string token)
		{
			var appdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			Logger.Write("Initializing code engine");
			initCodeEngine(token, appdir);
			Logger.Write("Running init script for " + appdir);
			runInitScript(token, appdir);
			Logger.Write("Initalizing language plugins");
			_pluginLocator().Locate().ToList()
				.ForEach(plugin => {
					var language = plugin.GetLanguage();
					Logger.Write("Initializing " + language);
					runInitScript(token, Path.Combine(Path.GetDirectoryName(plugin.FullPath), language + "-files"));
				});
			var locator = new ProfileLocator(token);
			var profilePath = locator.GetLocalProfilePath(locator.GetActiveLocalProfile());
			Logger.Write("Running init script for " + profilePath);
			runInitScript(token, profilePath);
		}

		public bool StartEditorEngine(IEnumerable<string> editorAndArguments, string token)
		{
			Logger.Write ("Starting instance");
			writeStartArguments(editorAndArguments);
			var instance = startInstance(token);
			if (instance == null)
				return false;
			return instance.Start(editorAndArguments.ToArray()) != "";
		}

		public void Shutdown(string token)
		{
			var codeEngine = _codeEnginelocator.GetInstance(token);
			if (codeEngine != null)
				codeEngine.Shutdown();
			var instance = _editorLocator.GetInstance(token);
			if (instance == null)
				return;
			var process = Process.GetProcessById(instance.ProcessID);
			process.Kill();
		}

		private void writeStartArguments (IEnumerable<string> args)
		{
			var arguments = "";
			foreach (var arg in args)
				arguments = arg + " ";
			Logger.Write ("Running editor with " + arguments);
				
		}
		
		private OpenIDE.Core.EditorEngineIntegration.Instance startInstance(string token)
		{
			var assembly = 
				Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					Path.Combine("EditorEngine", "EditorEngine.exe"));
			var exe = assembly;
			var arg = "\"" + token + "\"";
			Logger.Write ("Starting editor " + exe + " " + arg);
			if (Logger.IsEnabled)
				arg += getLogFileArgument(token);
			var proc = new Process();
			proc.Spawn(exe, arg, false, token);
            Logger.Write("Waiting for editor to initialize");
			var timeout = DateTime.Now.AddSeconds(5);
			while (DateTime.Now < timeout)
			{
				if (_editorLocator.GetInstance(token) != null)
					break;
				Thread.Sleep(10);
			}
            if (DateTime.Now > timeout)
                return null;
			return _editorLocator.GetInstance(token);
		}
		
		private void initCodeEngine(string token, string folder)
		{
			var defaultLanguage = getDefaultLanguage();
			if (defaultLanguage == null)
				defaultLanguage = "none-default-language";
			var enabledLanguages = getEnabledLanguages();
			if (enabledLanguages == null)
				enabledLanguages = "none-enabled-language";

			var cmd = Path.Combine("CodeEngine", "OpenIDE.CodeEngine.exe");
			var arg = "";
			arg = arg + "\"" + token + "\"" + defaultLanguage + enabledLanguages;
			Logger.Write("Starting code engine: " + cmd + " " + arg + " at " + folder);
			var proc = new Process();
			proc.Spawn(cmd, arg, false, folder);
		}
	
		private void runInitScript(string token, string folder)
		{
			if (!Directory.Exists(folder))
				return;
			var initscript = 
				new ScriptFilter()
					.FilterScripts(Directory.GetFiles(folder, "initialize.*"))
					.FirstOrDefault();
			if (initscript == null)
				return;
			Logger.Write("Found init script: " + initscript);
			var defaultLanguage = getDefaultLanguage();
			var enabledLanguages = getEnabledLanguages();
			var args = "\"" + token + "\"" + defaultLanguage + enabledLanguages;
			Logger.Write("Running: " + initscript + " " + args + " at " + folder);
			var proc = new Process();
			proc
				.Spawn(
					initscript,
					args,
					false,
					folder);
		}

		private string getDefaultLanguage()
		{
			var defaultLanguage = "";
			if (_defaultLanguage != null)
				defaultLanguage = " " + _defaultLanguage;
			return defaultLanguage;
		}

		private string getEnabledLanguages()
		{
			var enabledLanguages = "";
			if (_enabledLanguages != null && _enabledLanguages.Length > 0)
			{
				enabledLanguages = " \"";
				_enabledLanguages.ToList()
					.ForEach(x => enabledLanguages += x + ",");
				enabledLanguages = 
					enabledLanguages
						.Substring(0, enabledLanguages.Length - 1) + "\"";
			}
			return enabledLanguages;
		}

		private string getLogFileArgument(string token)
		{
			var config = new ConfigReader(token);
			var path = config.Get("oi.logpath");
            if (path == null)
                return "";
            return " \"--logging=" + Path.Combine(path, "EditorEngine.log") + "\"";
		}
	}
}
