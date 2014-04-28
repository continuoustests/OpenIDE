using System;
using System.Collections.Generic;
using System.Linq;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.Language;
using OpenIDE.Core.Snippets;

namespace OpenIDE.Arguments.Handlers
{
	class HandleSnippetHandler : ICommandHandler
	{
		private string _token;
		private Action<string> _dispatch;
		private List<ICommandHandler> _handlers = new List<ICommandHandler>();
		private ICodeEngineLocator _codeEngineFactory;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Manages code snippets");
				foreach (var handler in _handlers)
					usage.Add(handler.Usage);
				return usage;
			}
		}

		public HandleSnippetHandler(string token, Action<string> dispatcher, ICodeEngineLocator locator)
		{
			_token = token;
			_dispatch = dispatcher;
			_codeEngineFactory = locator;
			_handlers.Add(new CreateSnippetHandler(_codeEngineFactory));
			_handlers.Add(new SnippetEditHandler(_codeEngineFactory));
			_handlers.Add(new SnippetDeleteHandler(_codeEngineFactory));
			_handlers.Add(new PrewievSnippetHandler(_codeEngineFactory));
		}

		public string Command { get { return "snippet"; } }
		
		public void Execute (string[] arguments)
		{
			if (arguments.Length == 0) {
				listSnippets();
				return;
			}
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

		private void listSnippets()
		{
			var locator = new SnippetLocator(_token);
			foreach (var snippet in locator.GetSnippets())
				_dispatch(snippet.Name);
		}
	}
}