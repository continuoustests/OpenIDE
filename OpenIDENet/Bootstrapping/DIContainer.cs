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
		private string _path;

		public DIContainer()
		{
			_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_dispatcher = new CommandDispatcher(getDefaultHandlers().ToArray(), getPluginHandlers);
		}

		private IEnumerable<ICommandHandler> getCommandHandlers()
		{
			var handlers = new List<ICommandHandler>();
			handlers.AddRange(getDefaultHandlers());
			handlers.AddRange(getPluginHandlers());
			return handlers;
		}

		private IEnumerable<ICommandHandler> getDefaultHandlers()
		{
			var handlers = new List<ICommandHandler>();
			handlers.AddRange(
				new ICommandHandler[]
				{
					new EditorHandler(ILocateEditorEngine()),
					new CodeEngineGoToHandler(ICodeEngineLocator()),
					new CodeEngineExploreHandler(ICodeEngineLocator()),
					new ConfigurationHandler(_path),
					new BringToForegroundHandler()
				});
			handlers.Add(new RunCommandHandler(getPluginHandlers));
			return handlers;
		}

		private IEnumerable<ICommandHandler> getPluginHandlers()
		{
			var handlers = new List<ICommandHandler>();
			var plugins = PluginLocator().Locate();
			plugins.ToList()
				.ForEach(x => handlers.Add(new LanguageHandler(x)));
			return handlers;
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
			return getCommandHandlers();
		}
	}
}

