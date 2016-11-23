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
using OpenIDE.Core.Environments;

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
			CoreExtensions.ProcessExtensions.SetDispatcher(dispatchMessage);
		}

		public DefinitionBuilder GetDefinitionBuilder() {
			if (_definitionBuilder == null) {
				var enabledLanguages = _settings.EnabledLanguages;
				if (enabledLanguages == null)
					enabledLanguages = new string[] {};
				_definitionBuilder = 
					new DefinitionBuilder(
						_settings.RootPath,
						Environment.CurrentDirectory,
						_settings.DefaultLanguage,
						enabledLanguages,
						() => {
							return GetDefaultHandlers()
								.Select(x => 
									new BuiltInCommand(x.Command, x.Usage));
						},
						(path) => PluginLocator().LocateAllFor(path));
			}
			return _definitionBuilder;
		}

		public void EventDispatcher(string msg) {
			dispatchMessage("event|" + msg);
		}

		public IEnumerable<ICommandHandler> GetDefaultHandlers()
		{
			var handlers = new List<ICommandHandler>();
			var configHandler = new ConfigurationHandler(_settings.RootPath, PluginLocator(), EventDispatcher);
			var environment
				= new EnvironmentService(
					_settings.DefaultLanguage,
					_settings.EnabledLanguages,
					PluginLocator,
					ICodeEngineLocator(),
					ILocateEditorEngine());
			handlers.AddRange(
				new ICommandHandler[]
				{
					new InitHandler(configHandler),
					new ProfileHandler(configHandler, EventDispatcher),

					configHandler,
					
					new EditorHandler(_settings.RootPath, ILocateEditorEngine(), environment, dispatchMessage),
					new TouchHandler(dispatchMessage),
					new HandleScriptHandler(_settings.RootPath, dispatchMessage, PluginLocator),
					new HandleReactiveScriptHandler(_settings.RootPath, dispatchMessage, PluginLocator, ICodeEngineLocator()),
					new HandleSnippetHandler(_settings.RootPath, dispatchMessage, ICodeEngineLocator()),
					new HandleLanguageHandler(_settings.RootPath, dispatchMessage, PluginLocator),

					new CodeModelQueryHandler(ICodeEngineLocator(), ILocateEditorEngine()),

					new ProcessStartHandler(),

					new EventListener(_settings.RootPath),
					new OutputListener(_settings.RootPath, dispatchMessage, ICodeEngineLocator()),

					new PackageHandler(_settings.RootPath, _settings.SourcePrioritization, dispatchMessage, PluginLocator),

					new EnvironmentHandler(_settings.RootPath, dispatchMessage, ICodeEngineLocator(), ILocateEditorEngine(), environment),
					new ShutdownHandler(_settings.RootPath, dispatchMessage, environment),

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
						new ProfileLocator(_settings.RootPath),
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
			} else if (isColorized(command)) {
				printColorized(command);
			} else if (isCommand(command) || isEvent(command)) {
				lock (_commandProcessLock) {
					_commandsInProcessing++;
				}
				ThreadPool.QueueUserWorkItem((m) => {
					Logger.Write("Handling command in background thread");
					if (isCommand(command)) {
						Logger.Write("Handling as command");
						var prefix = getCommandPrefix(command);
						var parser = new CommandStringParser();
						var args = 
							parser.Parse(
								command.Substring(prefix.Length, command.Length - prefix.Length))
								.ToArray();
						if (args.Length == 0) {
							Logger.Write("No commands specified for " + command);
						}
						DefinitionCacheItem cmd = null;
						if (prefix == "command|")
							cmd = GetDefinitionBuilder().Get(args);
						else if (prefix == "command-builtin|")
							cmd = GetDefinitionBuilder().GetBuiltIn(args);
						else if (prefix == "command-language|")
							cmd = GetDefinitionBuilder().GetLanguage(args);
						else if (prefix == "command-languagescript|")
							cmd = GetDefinitionBuilder().GetLanguageScript(args);
						else if (prefix == "command-script|")
							cmd = GetDefinitionBuilder().GetScript(args);
						else if (prefix == "command-original|")
							cmd = GetDefinitionBuilder().GetOriginal(args);

						if (cmd != null) {
							new CommandRunner(EventDispatcher)
								.Run(cmd, args);
						} else {
							Logger.Write("Could not find handler for " + command);
						}
						onCommandCompleted();
					} else if (isEvent(command)) {
						Logger.Write("Handling as event");
						var prefix = "event|";
						EventDispatcher()
							.Forward(command.Substring(prefix.Length, command.Length - prefix.Length));
					}
					lock (_commandProcessLock) {
						_commandsInProcessing--;
					}
				}, null);
			} else if (command.StartsWith("raw|")) {
				var msg = command.Substring(4, command.Length - 4);
				Console.Write(msg);
			} else {
				Console.WriteLine(command);
			}
		}

		private bool isCommand(string command)
		{
			return command.StartsWith("command|") ||
				   command.StartsWith("command-builtin|") ||
				   command.StartsWith("command-language|") ||
				   command.StartsWith("command-script|") ||
				   command.StartsWith("command-original|");
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
			if (_settings.RawOutput) {
				Console.WriteLine(command);
				return;
			}
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
			if (_settings.RawOutput) {
				Console.WriteLine(command);
				return;
			}
			var commentTag = "warning|";
			var start = command.IndexOf(commentTag) + commentTag.Length;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(command.Substring(start, command.Length - start));
			Console.ResetColor();
		}

		private bool isColorized(string command)
		{
			var trimmed = command.Trim(); 
			return trimmed.Length > 6 && trimmed.StartsWith("color|") && trimmed.IndexOf("|", 6) > 0;
		}

		private void printColorized(string command)
		{
			if (_settings.RawOutput) {
				Console.WriteLine(command);
				return;
			}
			var commentTag = "color|";
			var start = command.IndexOf(commentTag) + commentTag.Length;
			var end = command.IndexOf("|", start);
			if (end > start) {
				var color = command.Substring(start, end - start);
				ConsoleColor clr;
				if (Enum.TryParse<ConsoleColor>(color, out clr)) {
					Console.ForegroundColor = clr;
				}
				Console.WriteLine(command.Substring(end + 1, command.Length - (end + 1)));
				Console.ResetColor();
			} else {
				Console.WriteLine(command.Substring(start, command.Length - start));
			}
		}
		
		public ILocateEditorEngine ILocateEditorEngine()
		{
			return new EngineLocator(IFS());
		}

		public EventIntegration.IEventDispatcher EventDispatcher()
		{
			return new EventIntegration.EventDispatcher(_settings.RootPath);
		}

		public ICodeEngineLocator ICodeEngineLocator()
		{
			return new CodeEngineDispatcher(IFS());
		}
	}
}

