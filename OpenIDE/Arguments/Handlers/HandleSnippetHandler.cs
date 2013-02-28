using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
	class HandleSnippetHandler : ICommandHandler
	{
		private List<ICommandHandler> _handlers = new List<ICommandHandler>();
		private ICodeEngineLocator _codeEngineFactory;


		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Handles queries against the code model cache");
				foreach (var handler in _handlers)
					usage.Add(handler.Usage);
				return usage;
			}
		}

		public HandleSnippetHandler(ICodeEngineLocator locator)
		{
			_codeEngineFactory = locator;
			_handlers.Add(new CreateSnippetHandler(_codeEngineFactory));
			_handlers.Add(new SnippetEditHandler(_codeEngineFactory));
			_handlers.Add(new SnippetDeleteHandler(_codeEngineFactory));
			_handlers.Add(new PrewievSnippetHandler(_codeEngineFactory));
		}

		public string Command { get { return "snippet"; } }
		
		public void Execute (string[] arguments)
		{
			if (arguments.Length == 0)
				return;
			var handler = _handlers.FirstOrDefault(x => x.Command == arguments[0]);
			if (handler == null)
				return;
			handler.Execute(getArguments(arguments));
		}

		private string[] getArguments(string[] args)
		{
			var arguments = new List<string>();
			for (int i = 1; i < args.Length; i++)
				arguments.Add(args[i]);
			return arguments.ToArray();
		}
	}
}