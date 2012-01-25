using System;
using System.IO;
using System.Linq;
using OpenIDENet.UI;
using OpenIDENet.CommandBuilding;
using OpenIDENet.Core.CommandBuilding;
using OpenIDENet.Core.Language;

namespace OpenIDENet.Arguments.Handlers
{
	class PrewievSnippetHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Displays the snippet preview form for completing a code snippet");
				usage.Add(
					"SNIPPET_FILE",
					"File containing the snippet text")
					.Add("PLACEHOLDERS", "Comma separated list of snippet placeholders")
						.Add("INSERT_TARGET", "Where to insert the completed snippet FILE|LINE|CLUMN");
				return usage;
			}
		}

		public string Command { get { return "complete-snippet"; } }
		
		public void Execute (string[] arguments)
		{
			if (arguments.Length != 3)
			{
				Console.WriteLine("Invalid number of arguments");
				return;
			}
			if (!File.Exists(arguments[0]))
			{
				Console.WriteLine("Invalid snippet file");
				return;
			}
			var form = new SnippetForm(
				new CommandStringParser(',').Parse(arguments[1]).ToArray(),
				File.ReadAllText(arguments[0]));
			form.ShowDialog();
		}
	}
}
