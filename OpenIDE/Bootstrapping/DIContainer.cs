using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.FileSystem;
using OpenIDE.Messaging;
using OpenIDE.Arguments;
using OpenIDE.Arguments.Handlers;
using OpenIDE.Core.EditorEngineIntegration;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.Language;
using OpenIDE.Core.Logging;
using OpenIDE.Core.Profiles;
using OpenIDE.CommandBuilding;
using OpenIDE.Core.CommandBuilding;
using OpenIDE.Core.Definitions;

namespace OpenIDE.Bootstrapping
{
	public class DIContainer
	{
		private AppSettings _settings;
		private List<ICommandHandler> _pluginHandlers = new List<ICommandHandler>();
		private DefinitionBuilder _definitionBuilder = null;
		private PluginLocator _pluginLocator = null;
		private object _commandProcessLock = new object();
		private int _commandsInProcessing = 0;

		public Action<string> DispatchMessage { get { return dispatchMessage; } }
		public Action<string, Action> DispatchAndCompleteMessage { get { return dispatchAndCompleteMessage; } }
		public bool IsProcessing { get { return _commandsInProcessing > 0; } }

		public DIContainer(AppSettings settings)
		{
			_settings = settings;
		}

		public DefinitionBuilder GetDefinitionBuilder() {
			if (_definitionBuilder == null) {
				_definitionBuilder = 
					new DefinitionBuilder(
						_settings.RootPath,
						Environment.CurrentDirectory,
						_settings.DefaultLanguage,
						() => {
							return GetDefaultHandlers()
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
			var configHandler = new ConfigurationHandler(PluginLocator());
			handlers.AddRange(
				new ICommandHandler[]
				{
					new InitHandler(configHandler),
					new ProfileHandler(configHandler),

					configHandler,
					
					new EditorHandler(_settings.RootPath, ILocateEditorEngine(), () => { return PluginLocator(); }),
					new TouchHandler(dispatchMessage),
					new HandleScriptHandler(_settings.RootPath, dispatchMessage, PluginLocator()),
					new HandleReactiveScriptHandler(_settings.RootPath, dispatchMessage, PluginLocator(), ICodeEngineLocator()),
					new HandleSnippetHandler(ICodeEngineLocator()),
					new HandleLanguageHandler(_settings.RootPath, dispatchMessage, PluginLocator()),

					new CodeModelQueryHandler(ICodeEngineLocator()),

					new CodeEngineGoToHandler(ICodeEngineLocator(), ILocateEditorEngine()),
					new CodeEngineExploreHandler(ICodeEngineLocator()),
					new MemberLookupHandler(ICodeEngineLocator()),
					new GoToDefinitionHandler(ICodeEngineLocator()),

					new ProcessStartHandler(),
					new BringToForegroundHandler(),

					new EventListener(_settings.RootPath),

					new PackageHandler(_settings.RootPath, dispatchMessage, PluginLocator()),
					new PkgTestHandler(_settings.RootPath),

					new EnvironmentHandler(dispatchMessage, ICodeEngineLocator(), ILocateEditorEngine()),
					new ShutdownHandler(_settings.RootPath, dispatchMessage, ILocateEditorEngine()),

					new GetCommandsHandler(),

					new RunCommandHandler(),

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
			if (_pluginLocator == null) {
				_pluginLocator =
					new PluginLocator(
						_settings.EnabledLanguages,
						new ProfileLocator(_settings.Path),
						(command) => dispatchMessage(command));
			}
			return _pluginLocator;
		}

		private void dispatchMessage(string command) {
			dispatchAndCompleteMessage(command, () => {});
		}

		private void dispatchAndCompleteMessage(string command, Action onCommandCompleted)
		{
			Logger.Write("Dispatching " + command);
			if (command.Length == 0) {
				Console.WriteLine();
			} else if (isError(command)) {
				printError(command);
			} else if (isWarning(command)) {
				printWarning(command);
			} else if (isCommand(command) || isEvent(command)) {
				lock (_commandProcessLock) {
					_commandsInProcessing++;
				}
				ThreadPool.QueueUserWorkItem((m) => {
					if (isCommand(command)) {
						var prefix = getCommandPrefix(command);
						var parser = new CommandStringParser();
						var args = 
							parser.Parse(
								command.Substring(prefix.Length, command.Length - prefix.Length));
						DefinitionCacheItem cmd = null;
						if (prefix == "command|")
							cmd = GetDefinitionBuilder().Get(args.ToArray());
						else if (prefix == "command-builtin|")
							cmd = GetDefinitionBuilder().GetBuiltIn(args.ToArray());
						else if (prefix == "command-language|")
							cmd = GetDefinitionBuilder().GetLanguage(args.ToArray());
						else if (prefix == "command-languagescript|")
							cmd = GetDefinitionBuilder().GetLanguageScript(args.ToArray());
						else if (prefix == "command-script|")
							cmd = GetDefinitionBuilder().GetScript(args.ToArray());

						if (cmd != null) {
							new CommandRunner()
								.Run(cmd, args.ToArray());
						}
						onCommandCompleted();
					} else if (isEvent(command)) {
						var prefix = "event|";
						EventDispatcher()
							.Forward(command.Substring(prefix.Length, command.Length - prefix.Length));
					}
					lock (_commandProcessLock) {
						_commandsInProcessing--;
					}
				}, null);
			} else {
				Console.WriteLine(command);
			}
		}

		private bool isCommand(string command)
		{
			return command.StartsWith("command|") ||
				   command.StartsWith("command-builtin|") ||
				   command.StartsWith("command-language|") ||
				   command.StartsWith("command-script|");
		}

		private string getCommandPrefix(string command) {
			return command.Substring(0, command.IndexOf('|', 0) + 1);
		}

		private bool isEvent(string command)
		{
			return command.StartsWith("event|");
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
	}
}

