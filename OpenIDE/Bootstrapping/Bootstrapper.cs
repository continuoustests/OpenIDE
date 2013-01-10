using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using OpenIDE.Arguments;
using OpenIDE.Arguments.Handlers;
using OpenIDE.Core.Config;
using OpenIDE.Core.CommandBuilding;
namespace OpenIDE.Bootstrapping
{
	public static class Bootstrapper
	{
		private static DIContainer _container;

		public static AppSettings Settings = null; 
		
		public static void Initialize()
		{
			Settings = new AppSettings(
				Path.GetDirectoryName(
					Assembly.GetExecutingAssembly().Location),
					getDefaultHandlers,
					getLanguageHandlers);
			_container = new DIContainer(Settings);
		}
		
		public static ICommandDispatcher GetDispatcher()
		{
			return _container.GetDispatcher();
		}

		public static IEnumerable<ICommandHandler> GetCommandHandlers()
		{
			return _container.ICommandHandlers();
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
		private const string PROFILE = "--config-rofile";

		private string _path;
		private ICommandHandler[] _handlers;
		private ICommandHandler[] _pluginHandlers;
		private Func<IEnumerable<ICommandHandler>> _handlerFactory;
		private Func<IEnumerable<ICommandHandler>> _pluginHandlerFactory;

		public string RootPath { get; private set; }
		public string Path { get { return _path; } }
		public string DefaultLanguage { get; private set; }
		public string[] EnabledLanguages { get; private set; }
		public string Plugin = "";

		public AppSettings(string path, Func<IEnumerable<ICommandHandler>> handlers, Func<IEnumerable<ICommandHandler>> pluginHandlers)
		{
			_path = path;
			RootPath = Configuration.GetConfigPoint(Directory.GetCurrentDirectory());
			if (RootPath == null)
				RootPath = Directory.GetCurrentDirectory();
			else
				RootPath = System.IO.Path.GetDirectoryName(RootPath);
			var local = new Configuration(Directory.GetCurrentDirectory(), false);
			var global = new Configuration(_path, false);

			if (local.DefaultLanguage != null)
				DefaultLanguage = local.DefaultLanguage;
			else if (global.DefaultLanguage != null)
				DefaultLanguage = global.DefaultLanguage;

			if (local.EnabledLanguages != null)
				EnabledLanguages = local.EnabledLanguages;
			else if (global.EnabledLanguages != null)
				EnabledLanguages = global.EnabledLanguages;

			_handlerFactory = handlers;
			_pluginHandlerFactory = pluginHandlers;
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
				if (arg.StartsWith(ENABLED_LANGUAGES))
				{
					EnabledLanguages = 
						new CommandStringParser(',')
							.Parse(arg
								.Substring(
									ENABLED_LANGUAGES.Length,
									arg.Length - ENABLED_LANGUAGES.Length)).ToArray();
					continue;
				}
				newArgs.Add(arg);
			}

			var unhandledArg = true;
			if (newArgs.Count > 0)
			{
				if (_handlers == null)
					_handlers = _handlerFactory().ToArray();
				if (_handlers.FirstOrDefault(x => x.Command.Equals(newArgs[0])) != null)
					unhandledArg = false;
			}

			if (DefaultLanguage != null && unhandledArg && newArgs.Count > 0)
			{
				if (_pluginHandlers == null)
					_pluginHandlers = _pluginHandlerFactory().ToArray();
				var command = 
					_pluginHandlers
						.FirstOrDefault(x => x.Command.Equals(DefaultLanguage));
				if (command != null)
				{
					var usage = command.Usage;
					if (usage != null)
					{
						if (usage.Parameters.Count(x => x.Name.Equals(newArgs[0])) > 0)
							newArgs.Insert(0, DefaultLanguage);
					}
				}
			}
			return newArgs.ToArray();
		}
	}
}
