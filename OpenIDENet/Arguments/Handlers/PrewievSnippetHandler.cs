using System;
using System.IO;
using System.Linq;
using System.Text;
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
					"File containing the snippet text. First line contains placeholders (comma separated).")
					.Add("INSERT_TARGET", "Where to insert the completed snippet FILE|LINE|CLUMN")
						.Add("[INDENTATION]", "Indentation snippet lines t=tab s=space. Eks: ttss");
				return usage;
			}
		}

		public string Command { get { return "snippet-complete"; } }
		
		public void Execute (string[] arguments)
		{
			if (arguments.Length < 2)
			{
				Console.WriteLine("Invalid number of arguments");
				return;
			}
			if (!File.Exists(arguments[0]))
			{
				Console.WriteLine("Invalid snippet file");
				return;
			}
			var lines = File.ReadAllLines(arguments[0]);
			if (lines.Length < 2)
			{
				Console.WriteLine("Snippet files has to contain a minimum of 2 lines. " +
								  "First line are comma separated placeholder. Second line is " +
								  "where the actual snippet starts.");
			}
			var snippet = new StringBuilder();
			for (int i = 1; i < lines.Length; i++)
				snippet.AppendLine(lines[i]);

			var indentation = "";
			if (arguments.Length == 3)
				indentation = arguments[2].Replace("t", "\t").Replace("s", " ");

			var form = new SnippetForm(
				new CommandStringParser(',').Parse(lines[0]).ToArray(),
				snippet.ToString());
			form.ShowDialog();
			
			if (form.Content == null)
				return;

			var contentLines = form.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			var firstLine = true;
			foreach (var contentLine in contentLines)
			{
				if (firstLine)
					Console.WriteLine(contentLine);
				else
					Console.WriteLine(indentation + contentLine);
				firstLine = false;
			}
		}
	}
}
