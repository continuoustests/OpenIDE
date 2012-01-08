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
		private List<ICommandHandler> _handlers = new List<ICommandHandler>();
		private string _path;

		public DIContainer()
		{
			_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			addCommandHandlers();
			_dispatcher = new CommandDispatcher(_handlers.ToArray());
		}

		private void addCommandHandlers()
		{
			_handlers = new List<ICommandHandler>();
			_handlers.AddRange(
				new ICommandHandler[]
				{
					new EditorHandler(ILocateEditorEngine()),
					new CodeEngineGoToHandler(ICodeEngineLocator()),
					new CodeEngineExploreHandler(ICodeEngineLocator()),
					new ConfigurationHandler(_path)
				});
			
			var plugins = PluginLocator().Locate();
			plugins.ToList()
				.ForEach(x => _handlers.Add(new LanguageHandler(x)));

			_handlers.Add(new RunCommandHandler(_handlers.ToArray()));
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
			var errorTag = "error|";
			var start = command.IndexOf(errorTag) + errorTag.Length;
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
			return _handlers;
		}
	}
}

