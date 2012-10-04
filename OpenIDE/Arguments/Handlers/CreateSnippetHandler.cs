using System;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
	class CreateSnippetHandler : ICommandHandler
	{
		private ICodeEngineLocator _codeEngineFactory;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Creates a new snippet for a given language");
				usage.Add("LANGUAGE", "Language to create snippet for (name or file extnsion)")
					.Add("SNIPPET_NAME", "Desired name of snippet")
						.Add("[--global]", "Will create the new snippet in the main language folder")
							.Add("[-g]", "Short for --global");
				return usage;
			}
		}

		public string Command { get { return "snippet-create"; } }

		public CreateSnippetHandler(ICodeEngineLocator locator)
		{
			_codeEngineFactory = locator;
		}

		public void Execute(string[] arguments)
		{
			var instance = _codeEngineFactory.GetInstance(Environment.CurrentDirectory);
			if (instance == null)
				return;
			if (arguments.Length < 2)
				return;
			instance.SnippetCreate(arguments);
		}
	}
}
