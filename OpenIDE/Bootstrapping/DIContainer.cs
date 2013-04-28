using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.FileSystem;
using OpenIDE.Messaging;
using OpenIDE.Arguments;
using OpenIDE.Arguments.Handlers;
using OpenIDE.Core.EditorEngineIntegration;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.Language;
using OpenIDE.Core.Profiles;
using OpenIDE.CommandBuilding;
using OpenIDE.Core.CommandBuilding;
using OpenIDE.Core.Definitions;

namespace OpenIDE.Bootstrapping
{
	public class DIContainer
	{
		private AppSettings _settings;
		private ICommandDispatcher _dispatcher;
		private List<ICommandHandler> _pluginHandlers = new List<ICommandHandler>();
		private DefinitionBuilder _definitionBuilder = null;

		public Action<string> DispatchMessage { get { return dispatchMessage; } }

		public DIContainer(AppSettings settings)
		{
			_settings = settings;
			_dispatcher = new CommandDispatcher(
				GetDefaultHandlers().ToArray(),
				GetPluginHandlers,
				EventDispatcher());
		}

		public DefinitionBuilder GetDefinitionBuilder() {
			if (_definitionBuilder == null) {
				_definitionBuilder = 
					new DefinitionBuilder(
						_settings.RootPath,
						Environment.CurrentDirectory,
						_settings.DefaultLanguage,
						() => {
							return GetDefaultHandlersWithoutRunHandler()
								.Select(x => 
									new BuiltInCommand(x.Command, x.Usage));
						},
						(path) => PluginLocator().LocateFor(path));
			}
			return _definitionBuilder;
		}

		public IEnumerable<ICommandHandler> GetDefaultHandlers()
		{
			var handlers = new List<ICommandHandler>();
			handlers.AddRange(GetDefaultHandlersWithoutRunHandler());
			handlers.Add(new RunCommandHandler(() =>
				{
					var runHandlers = new List<ICommandHandler>();
					runHandlers.AddRange(GetDefaultHandlersWithoutRunHandler());
					runHandlers.AddRange(GetPluginHandlers());
					return runHandlers;
				}));
			return handlers;
		}

		public IEnumerable<ICommandHandler> GetDefaultHandlersWithoutRunHandler()
		{
			var handlers = new List<ICommandHandler>();
			var configHandler = new ConfigurationHandler(PluginLocator());
			handlers.AddRange(
				new ICommandHandler[]
				{
					new InitHandler(configHandler),
					new ProfileHandler(configHandler),

					configHandler,
					
					new EditorHandler(ILocateEditorEngine(), () => { return PluginLocator(); }),
					new TouchHandler(dispatchMessage),
					new HandleScriptHandler(_settings.RootPath, dispatchMessage),
					new HandleReactiveScriptHandler(_settings.RootPath, dispatchMessage, PluginLocator()),
					new HandleSnippetHandler(ICodeEngineLocator()),

					new CodeModelQueryHandler(ICodeEngineLocator()),

					new CodeEngineGoToHandler(ICodeEngineLocator(), ILocateEditorEngine()),
					new CodeEngineExploreHandler(ICodeEngineLocator()),
					new MemberLookupHandler(ICodeEngineLocator()),
					new GoToDefinitionHandler(ICodeEngineLocator()),

					new ProcessStartHandler(),
					new BringToForegroundHandler(),

					new EventListener(_settings.RootPath),

					new PackageHandler(_settings.RootPath, dispatchMessage),
					new PkgTestHandler(_settings.RootPath),

					new GetCommandsHandler(),

					new HelpHandler()
				});
			return handlers;
		}

		public IEnumerable<ICommandHandler> GetPluginHandlers()
		{
			if (_pluginHandlers.Count == 0)
			{
				var plugins = PluginLocator().Locate();
				plugins.ToList()
					.ForEach(x =>
						{
							if (_settings.EnabledLanguages == null ||
								_settings.EnabledLanguages.Contains(x.GetLanguage()))
								_pluginHandlers.Add(new LanguageHandler(x));
						});
			}
			return _pluginHandlers;
		}

		public ICommandDispatcher GetDispatcher()
		{
			return _dispatcher;
		}

		public IFS IFS()
		{
			return new FS();
		}

		public IMessageBus IMessageBus()
		{
			return new MessageBus();
		}

		public PluginLocator PluginLocator()
		{
			return new PluginLocator(
				_settings.EnabledLanguages,
				new ProfileLocator(_settings.Path),
				(command) => dispatchMessage(command));
		}

		private void dispatchMessage(string command)
		{
			if (command.Length == 0) {
				Console.WriteLine();
				return;
			}
			if (isError(command))
			{
				printError(command);
				return;
			}
			if (isWarning(command))
			{
				printWarning(command);
				return;
			}
			if (isCommand(command))
			{
				var prefix = "command|";
				var parser = new CommandStringParser();
				var args = 
					parser.Parse(
						command.Substring(prefix.Length, command.Length - prefix.Length));
				_dispatcher.For(
					parser.GetCommand(args),
					parser.GetArguments(args),
					(m) => {});
				return;
			}
			Console.WriteLine(command);			
		}

		private bool isCommand(string command)
		{
			return command.StartsWith("command|");
		}

		private bool isError(string command)
		{
			return command.Trim().StartsWith("error|");
		}

		private void printError(string command)
		{
			var commentTag = "error|";
			var start = command.IndexOf(commentTag) + commentTag.Length;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(command.Substring(start, command.Length - start));
			Console.ResetColor();
		}
		
		private bool isWarning(string command)
		{
			return command.Trim().StartsWith("warning|");
		}

		private void printWarning(string command)
		{
			var commentTag = "warning|";
			var start = command.IndexOf(commentTag) + commentTag.Length;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(command.Substring(start, command.Length - start));
			Console.ResetColor();
		}
		
		public ILocateEditorEngine ILocateEditorEngine()
		{
			return new EngineLocator(IFS());
		}

		public EventIntegration.IEventDispatcher EventDispatcher()
		{
			return new EventIntegration.EventDispatcher(_settings.Path);
		}

		public ICodeEngineLocator ICodeEngineLocator()
		{
			return new CodeEngineDispatcher(IFS());
		}

		public IEnumerable<ICommandHandler> ICommandHandlers()
		{
			var list = new List<ICommandHandler>();
			list.AddRange(GetDefaultHandlers());
			list.AddRange(GetPluginHandlers());
			return list;
		}
	}
}

