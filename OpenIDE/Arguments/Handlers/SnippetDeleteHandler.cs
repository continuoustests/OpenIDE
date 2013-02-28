using System;
using OpenIDE.Core.Language;
using OpenIDE.Core.CodeEngineIntegration;

namespace OpenIDE.Arguments.Handlers
{
	class SnippetDeleteHandler : ICommandHandler
	{
		private ICodeEngineLocator _codeEngineFactory;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Deletes a snippet");
					usage.Add("LANGUAGE", "Language snippet belongs to (name or file extension)")
						.Add("SNIPPET_NAME",
							"Name coresponding to the snippet name in Languages/[LANGUAGE]/snippets");
					return usage;
			}
		}
	
		public string Command { get { return "rm"; } }
		
		public SnippetDeleteHandler(ICodeEngineLocator codeEngineFactory)
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
			instance.SnippetDelete(arguments);
		}
	}	
}
