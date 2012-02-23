using System;
using OpenIDE.Core.Language;
using OpenIDE.CodeEngineIntegration;

namespace OpenIDE.Arguments.Handlers
{
	class SnippetEditHandler : ICommandHandler
	{
		private ICodeEngineLocator _codeEngineFactory;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Opens a snippet for edit");
					usage.Add("LANGUAGE", "Language snippet belongs to (name or file extension)")
						.Add("SNIPPET_NAME",
							"Name coresponding to the snippet name in Languages/[LANGUAGE]/snippets");
					return usage;
			}
		}
	
		public string Command { get { return "snippet-edit"; } }
		
		public SnippetEditHandler(ICodeEngineLocator codeEngineFactory)
		{
			_codeEngineFactory = codeEngineFactory;
		}

		public void Execute(string[] arguments)
		{
			var instance = _codeEngineFactory.GetInstance(Environment.CurrentDirectory);
			if (instance == null)
				return;
			if (arguments.Length < 2)
				return;
			instance.SnippetEdit(arguments);
		}
	}
}
