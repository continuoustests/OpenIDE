using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using OpenIDE.Core.UI;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.CodeEngine.Core.Commands;
using OpenIDE.CodeEngine.Core.EditorEngine;

namespace OpenIDE.CodeEngine.Core.Handlers
{
	public class CompleteSnippetHandler
	{
		private Editor _editor;
		private ICacheBuilder _cache;

		public CompleteSnippetHandler(Editor editor, ICacheBuilder cache)
		{
			_editor = editor;
			_cache = cache;
		}

		public void Handle(string[] arguments)
		{
			if (arguments.Length < 3)
				return;
			var language = getLanguage(arguments[0]);
			if (language == null)
				return;
			var file = Path.Combine(
				Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)),
				Path.Combine(
					"Languages",
					Path.Combine(
						language,
						Path.Combine("snippets", arguments[1] + ".snippet"))));
			if (!File.Exists(file))
				return;
			var lines = File.ReadAllLines(file);
			if (lines.Length < 3)
				return;
			var placeHolders = lines[0];
			var offsetLine = lines[1];
			var snippet = new StringBuilder();
			for (int i = 2; i < lines.Length; i++)
				snippet.AppendLine(lines[i]);
			var indentation = "";
			if (arguments.Length == 4)
				indentation = arguments[3].Replace("t", "\t").Replace("s", " ");
			var form = new SnippetForm(
				new OpenIDE.Core.CommandBuilding.CommandStringParser(',').Parse(placeHolders).ToArray(),
				snippet.ToString());
			int lineCount = 0;
			int lastLineLength = 0;
			form.OnRun((content) =>
				{
					var sb = new StringBuilder();
					var contentLines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
					for (int i = 0; i < contentLines.Length; i++)
					{
						var contentLine  = contentLines[i];
						if (i == 0)
						{
							sb.AppendLine(contentLine);
							lastLineLength = contentLine.Length;
							lineCount++;
						}
						else if (i == contentLines.Length - 1 && contentLine == "")
							break;
						else if (i == contentLines.Length - 1)
						{
							sb.Append(indentation + contentLine);
							lastLineLength = indentation.Length + contentLine.Length;
							lineCount++;
						}
						else
						{
							sb.AppendLine(indentation + contentLine);
							lastLineLength = indentation.Length + contentLine.Length;
							lineCount++;
						}
					}

					form = null;
					var tempFile = Path.GetTempFileName();
					File.WriteAllText(tempFile, sb.ToString());
					var offset = new OpenIDE.Core.CommandBuilding.Position(offsetLine);
					if (offset.Line == -1 && offset.Column == -1)
						offset = new OpenIDE.Core.CommandBuilding.Position(lineCount - 1, lastLineLength);
					else
						offset.AddToColumn(indentation.Length);
					_editor.Send("insert \"" + tempFile + "\" " +  arguments[2] + " \"" + offset.ToCommand() + "\"");
				});
			form.Show();
			form.BringToFront();
		}

		private string getLanguage(string param)
		{
			return new PluginFinder(_cache).FindLanguage(param);
		}
	}
}
