using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDENet.FileSystem;
using OpenIDENet.Messaging;
using OpenIDENet.Arguments;
using OpenIDENet.Arguments.Handlers;
using OpenIDENet.EditorEngineIntegration;
using OpenIDENet.CodeEngineIntegration;

namespace OpenIDENet.Bootstrapping
{
	public class DIContainer
	{
		public IFS IFS()
		{
			return new FS();
		}

		public IMessageBus IMessageBus()
		{
			return new MessageBus();
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
			var handlers = new List<ICommandHandler>();
			handlers.AddRange(getHandlers());
			handlers.Add(new RunCommandHandler(getHandlers().ToArray()));
			return handlers;
		}

		private IEnumerable<ICommandHandler> getHandlers()
		{
			return new ICommandHandler[]
				{
					new EditorHandler(ILocateEditorEngine()),
					new CodeEngineGoToHandler(ICodeEngineLocator()),
					new CodeEngineExploreHandler(ICodeEngineLocator())
				};
		}
	}
}

