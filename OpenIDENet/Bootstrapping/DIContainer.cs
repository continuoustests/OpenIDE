using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDENet.FileSystem;
using OpenIDENet.Messaging;
using OpenIDENet.Arguments;
using OpenIDENet.Arguments.Handlers;
using OpenIDENet.EditorEngineIntegration;
using OpenIDENet.CodeEngineIntegration;
using OpenIDENet.Core.Language;
using OpenIDENet.CommandBuilding;

namespace OpenIDENet.Bootstrapping
{
	public class DIContainer
	{
		private ICommandDispatcher _dispatcher;
		private List<ICommandHandler> _pluginHandlers = new List<ICommandHandler>();

		public DIContainer()
		{
			_dispatcher = new CommandDispatcher(getDefaultHandlers().ToArray(), getPluginHandlers);
		}

		private IEnumerable<ICommandHandler> getDefaultHandlers()
		{
			var handlers = new List<ICommandHandler>();
			handlers.AddRange(getDefaultHandlersWithoutRunHandler());
			handlers.Add(new RunCommandHandler(() =>
				{
					var runHandlers = new List<ICommandHandler>();
					runHandlers.AddRange(getDefaultHandlersWithoutRunHandler());
					runHandlers.AddRange(getPluginHandlers());
					return runHandlers;
				}));
			return handlers;
		}

		private IEnumerable<ICommandHandler> getDefaultHandlersWithoutRunHandler()
		{
			var handlers = new List<ICommandHandler>();
			handlers.AddRange(
				new ICommandHandler[]
				{
					new EditorHandler(ILocateEditorEngine()),
					new CodeEngineGoToHandler(ICodeEngineLocator()),
					new CodeEngineExploreHandler(ICodeEngineLocator()),
					new CodeEngineGetProjectsHandler(ICodeEngineLocator()),
					new CodeEngineGetFilesHandler(ICodeEngineLocator()),
					new CodeEngineGetCodeRefsHandler(ICodeEngineLocator()),
					new CodeEngineGetSignatureRefsHandler(ICodeEngineLocator()),
					new ConfigurationHandler(),
					new BringToForegroundHandler()
				});
			return handlers;
		}

		private IEnumerable<ICommandHandler> getPluginHandlers()
		{
			if (_pluginHandlers.Count == 0)
			{
				var plugins = PluginLocator().Locate();
				plugins.ToList()
					.ForEach(x => _pluginHandlers.Add(new LanguageHandler(x)));
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
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				(command) => dispatchMessage(command));
		}

		private void dispatchMessage(string command)
		{
			if (command.Length == 0)
				return;
			var parser = new CommandStringParser();
			var args = parser.Parse(command);
			if (isError(command))
			{
				printError(command);
				return;
			}
			if (isComment(command))
			{
				printComment(command);
				return;
			}
			_dispatcher.For(
				parser.GetCommand(args),
				parser.GetArguments(args));
		}

		private bool isError(string command)
		{
			return command.Trim().StartsWith("error|");
		}

		private void printError(string command)
		{
			var commentTag = "comment|";
			var start = command.IndexOf(commentTag) + commentTag.Length;
			Console.WriteLine(command.Substring(start, command.Length - start));
		}
		
		private bool isComment(string command)
		{
			return command.Trim().StartsWith("comment|");
		}

		private void printComment(string command)
		{
			var commentTag = "comment|";
			var start = command.IndexOf(commentTag) + commentTag.Length;
			Console.WriteLine(command.Substring(start, command.Length - start));
		}
		
		public ILocateEditorEngine ILocateEditorEngine()
		{
			return new EngineLocator(IFS());
		}

		public ICodeEngineLocator ICodeEngineLocator()
		{
			return new CodeEngineDispatcher(IFS());
		}

		public IEnumerable<ICommandHandler> ICommandHandlers()
		{
			var list = new List<ICommandHandler>();
			list.AddRange(getDefaultHandlers());
			list.AddRange(getPluginHandlers());
			return list;
		}
	}
}

