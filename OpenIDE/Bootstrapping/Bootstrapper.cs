using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using OpenIDE.Arguments;
using OpenIDE.Arguments.Handlers;
using OpenIDE.Core.Config;
using OpenIDE.Core.Configs;
using OpenIDE.Core.Profiles;
using OpenIDE.Core.CommandBuilding;
using CoreExtensions;
namespace OpenIDE.Bootstrapping
{
	public static class Bootstrapper
	{
		private static DIContainer _container;
		private static Interpreters _interpreters;

		public static AppSettings Settings = null; 
		public static Action<string> DispatchMessage { get { return _container.DispatchMessage ; } }
		public static Action<string> DispatchEvent { get { return _container.EventDispatcher ; } }
		public static Action<string, Action> DispatchAndCompleteMessage { get { return _container.DispatchAndCompleteMessage; } }
		public static bool IsProcessing { get { return _container.IsProcessing; } }
		
		public static void Initialize()
		{
			Settings = new AppSettings(
				Path.GetDirectoryName(
					Assembly.GetExecutingAssembly().Location),
					getDefaultHandlers,
					getLanguageHandlers);
			_interpreters = new Interpreters(Settings.RootPath);
			ProcessExtensions.GetInterpreter = 
				(file) => {
						return _interpreters
							.GetInterpreterFor(Path.GetExtension(file));
					};
			_container = new DIContainer(Settings);
		}
		
		public static OpenIDE.Core.Definitions.DefinitionBuilder GetDefinitionBuilder() {
			return _container.GetDefinitionBuilder();
		}

		public static IEnumerable<ICommandHandler> GetDefaultHandlers()
		{
			return _container.GetDefaultHandlers();
		}

		private static IEnumerable<ICommandHandler> getDefaultHandlers()
		{
			return _container.GetDefaultHandlers();
		}

		private static IEnumerable<ICommandHandler> getLanguageHandlers()
		{
			return _container.GetPluginHandlers();
		}
	}

	public class AppSettings
	{
		private const string DEFAULT_LANGUAGE = "--default.language=";
		private const string ENABLED_LANGUAGES = "--enabled.languages";
		private const string ENABLE_LOGGING = "--logging";
		private const string RAW_OUTPUT = "--raw";

		private string _path;

		public string TokenPath { get; private set; }
		public string RootPath { get; private set; }
		public string Path { get { return _path; } }
		public string DefaultLanguage { get; private set; }
		public string[] EnabledLanguages { get; private set; }
		public string[] SourcePrioritization { get; private set; }
		public string Plugin = "";
		public bool LoggingEnabled = false;
		public bool RawOutput = false;

		public AppSettings(string path, Func<IEnumerable<ICommandHandler>> handlers, Func<IEnumerable<ICommandHandler>> pluginHandlers)
		{
			_path = path;
			SourcePrioritization = new string[] {};
			var locator = new ProfileLocator(fixPath(Environment.CurrentDirectory));
			RootPath = locator.GetLocalProfilePath("default");
			if (RootPath == null)
				RootPath = fixPath(Directory.GetCurrentDirectory());
			else
				RootPath = System.IO.Path.GetDirectoryName(RootPath);
			var reader = new ConfigReader(RootPath);

			var defaultLanguage = reader.Get("default.language");
			if (defaultLanguage != null)
				DefaultLanguage = defaultLanguage;

			var enabledLanguages = reader.Get("enabled.languages");
			if (enabledLanguages != null)
				EnabledLanguages = splitValue(enabledLanguages);

			var prioritizedSources = reader.Get("oi.source.prioritization");
			if (prioritizedSources != null)
				SourcePrioritization = splitValue(prioritizedSources);
		}

		private string[] splitValue(string value)
		{
			return value
				.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim())
				.ToArray();
		}

		private string fixPath(string path)
		{
			if (path.Contains(":"))
				return path.ToLower();
			return path;
		}

		public string[] Parse(string[] args)
		{
			var newArgs = new List<string>();
			foreach (var arg in args)
			{
				if (arg.StartsWith(DEFAULT_LANGUAGE))
				{
					DefaultLanguage = arg
						.Substring(DEFAULT_LANGUAGE.Length, arg.Length - DEFAULT_LANGUAGE.Length);
					continue;
				}
				else if (arg.StartsWith(ENABLED_LANGUAGES))
				{
					EnabledLanguages = 
						new CommandStringParser(',')
							.Parse(arg
								.Substring(
									ENABLED_LANGUAGES.Length,
									arg.Length - ENABLED_LANGUAGES.Length)).ToArray();
					continue;
				} else if (arg == ENABLE_LOGGING) {
					LoggingEnabled = true;
					continue;
				} else if (arg == RAW_OUTPUT) {
					RawOutput = true;
					continue;
				}

				newArgs.Add(arg);
			}
			return newArgs.ToArray();
		}		
	}
}
