using System;
using System.IO;
using System.Text;
using System.Reflection;
using OpenIDE.CodeEngine.Core.EditorEngine;
using OpenIDE.CodeEngine.Core.Caching;

namespace OpenIDE.CodeEngine.Core.Handlers
{
	public class CreateSnippetHandler
	{
		private Editor _editor;
		private ICacheBuilder _cache;
		
		public CreateSnippetHandler(Editor editor, ICacheBuilder cache)
		{
			_editor = editor;
			_cache = cache;
		}

		public void Handle(string[] arguments)
		{
			Logging.Logger.Write("We are in create snippet handler");
			if (arguments.Length < 2)
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
			if (File.Exists(file))
				return;
			var sb = new StringBuilder();
			sb.AppendLine("{param1},{param2}");
			sb.AppendLine("-1|-1");
			sb.Append("Will replace {param1} and {param2}");
			File.WriteAllText(file, sb.ToString());
			_editor.Send(string.Format("goto \"{0}|0|0\"", file));
		}

		private string getLanguage(string param)
		{
			return new PluginFinder(_cache).FindLanguage(param);
		}
	}
}
