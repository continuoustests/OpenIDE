using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.CodeEngine.Core.EditorEngine;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.Core.Config;

namespace OpenIDE.CodeEngine.Core.Handlers
{
	public class CreateSnippetHandler
	{
		private Editor _editor;
		private ICacheBuilder _cache;
		private string _keyPath;
		
		public CreateSnippetHandler(Editor editor, ICacheBuilder cache, string keyPath)
		{
			_editor = editor;
			_cache = cache;
			_keyPath = keyPath;
		}

		public void Handle(string[] arguments)
		{
			if (arguments.Length < 2)
				return;
			var language = getLanguage(arguments[0]);
			if (language == null)
				return;
			var file = Path.Combine(
				getPath(arguments),
				Path.Combine(
					"Languages",
					Path.Combine(
						language,
						Path.Combine("snippets", arguments[1] + ".snippet"))));
			if (File.Exists(file))
				return;
			createDirectories(file);
			var sb = new StringBuilder();
			sb.AppendLine("{param1},{param2}");
			sb.AppendLine("-1|-1");
			sb.Append("Will replace {param1} and {param2}");
			File.WriteAllText(file, sb.ToString());
			_editor.Send(string.Format("goto \"{0}|0|0\"", file));
		}

		private string getPath(string[] arguments)
		{
			if (arguments.Contains("--global") || arguments.Contains("-g"))
				return Path
					.GetDirectoryName(
						Path.GetDirectoryName(
							Assembly.GetExecutingAssembly().Location));
			else
				return Path.GetDirectoryName(
					Configuration.GetConfigFile(_keyPath));
		}

		private string getLanguage(string param)
		{
			return new PluginFinder(_cache).FindLanguage(param);
		}

		private void createDirectories(string file)
		{
			var unexisting = new List<string>();
			var dir = Path.GetDirectoryName(file);
			while (!Directory.Exists(dir))
			{
				unexisting.Insert(0, Path.GetFileName(dir));
				dir = Path.GetDirectoryName(dir);
				if (dir == null)
					break;
			}
			unexisting.ForEach(x => {
				dir = Path.Combine(dir, x);
				Directory.CreateDirectory(_keyPath);
			});
		}
	}
}
