using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using OpenIDE.Core.UI;
using OpenIDE.CommandBuilding;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.CommandBuilding;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
	class PrewievSnippetHandler : ICommandHandler
	{
		private ICodeEngineLocator _codeEngineFactory;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Displays the snippet preview form for completing a code snippet");
				usage.Add("LANGUAGE", "Language snippet belongs to (name or file extension)")
					.Add("SNIPPET_NAME",
						"Name coresponding to the snippet name in Languages/[LANGUAGE]/snippets")
						.Add("INSERT_TARGET", "Where to insert the completed snippet FILE|LINE|COLUMN")
							.Add("[INDENTATION]", "Indentation snippet lines t=tab s=space. Eks: ttss");
				return usage;
			}
		}

		public string Command { get { return "complete"; } }
		
		public PrewievSnippetHandler(ICodeEngineLocator codeEngineFactory)
		{
			_codeEngineFactory = codeEngineFactory;
		}

		public void Execute (string[] arguments)
		{
			var instance = _codeEngineFactory.GetInstance(Environment.CurrentDirectory);
			if (instance == null)
				return;
			if (arguments.Length < 3)
				return;
			instance.SnippetComplete(arguments);
		}
	}
}
