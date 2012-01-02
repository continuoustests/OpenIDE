using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDENet.Arguments;
using OpenIDENet.Arguments.Handlers;

namespace OpenIDENet.Language
{
	public class LanguagePlugin
	{
		private string _path;
		private Func<string, string, IEnumerable<string>> _execute;

		public LanguagePlugin(string path, Func<string, string, IEnumerable<string>> execute)
		{
			_path = path;
			_execute = execute;
		}

		public string GetLanguage()
		{
			return ToSingleLine("get-language");
		}

		public IEnumerable<CommandHandlerParameter> GetUsages()
		{
			return getUsages();
		}

		public string GetCrawlFileTypes()
		{
			return ToSingleLine("crawl-file-types");
		}

		public IEnumerable<string> Crawl(IEnumerable<string> filesAndFolders)
		{
			var file = Path.GetTempFileName();
			File.WriteAllLines(filesAndFolders.ToArray());
			foreach (var line in run(string.Format("crawl-source \"{0}\"", file)))
				yield return line;
		}
		
		private IEnumerable<CommandHandlerParameter> getUsages()
		{
			var commands = new List<CommandHandlerParameter>();
			new UsageParser(getUsage())
				.Parse().ToList()
					.ForEach(y =>
						{
							var cmd = new CommandHandlerParameter(
								GetLanguage(),
								CommandType.FileCommand,
								y.Name,
								y.Description);
							y.Parameters.ToList()
								.ForEach(p => cmd.Add(p));
							if (!y.Required)
								cmd.IsOptional();
							commands.Add(cmd);
						});
			return commands;
		}
		
		private string getUsage()
		{
			return ToSingleLine("get-command-definitions");
		}

		private string ToSingleLine(string arguments)
		{
			var text = new StringBuilder();
			run(arguments).ToList()
				.ForEach(x => text.Append(text));
			return text.ToString();
		}

		private IEnumerable<string> run(string arguments)
		{
			return _execute(_path, arguments);
		}
	}
}
