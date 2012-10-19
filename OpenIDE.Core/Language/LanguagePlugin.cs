using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.CommandBuilding;

namespace OpenIDE.Core.Language
{
	public class LanguagePlugin
	{
		private string _path;
		private Action<string, string, Action<string>> _execute;
		private Action<string> _dispatch;

		public LanguagePlugin(
			string path,
			Action<string, string, Action<string>> execute,
			Action<string> dispatch)
		{
			_path = path;
			_execute = execute;
			_dispatch = dispatch;
		}

		public string GetPluginDir()
		{
			return 
				Path.Combine(Path.GetDirectoryName(_path),
				Path.GetFileNameWithoutExtension(_path) + "-plugin");
		}

		public string GetLanguage()
		{
			return Path.GetFileNameWithoutExtension(_path);
		}

        public void Initialize(string keyPath)
        {
			run(string.Format("initialize \"{0}\"", keyPath), (line) => _dispatch(line));
        }

        public void Shutdown()
        {
            run("shutdown");
        }

		public IEnumerable<BaseCommandHandlerParameter> GetUsages()
		{
			return getUsages();
		}

		public string GetCrawlFileTypes()
		{
			return ToSingleLine("crawl-file-types");
		}

		public void Crawl(IEnumerable<string> filesAndFolders, Action<string> onLineReceived)
		{
			var file = Path.GetTempFileName();
			File.WriteAllLines(file, filesAndFolders.ToArray());
			run(string.Format("crawl-source \"{0}\"", file), onLineReceived);
			File.Delete(file);
		}

		public SignatureLocation SignatureFromPosition(FilePosition position)
		{
			try {
				var lines = new List<string>();
				run(string.Format(
						"signature-from-position \"{0}\"",
						position.ToCommand()),
				    (m) => lines.Add(m));
				if (lines.Count != 4)
					return null;
				return new SignatureLocation(
					lines[0],
					lines[1],
					new Position(lines[2]),
					new Position(lines[3]));
			} catch {
				return null;
			}
		}

		public string[] RetrieveMembersFromSignature(string signature)
		{
			var lines = new List<string>();
			run("members-from-signature \"" + signature + "\"", (m) => lines.Add(m));
			return lines.ToArray();
		}

        public string[] Query(string[] arguments)
		{
			var sb = new StringBuilder();
			arguments.ToList()
				.ForEach(x => sb.Append(" \"" + x + "\""));
			var lines = new List<string>();
			run(sb.ToString(), (m) => lines.Add(m));
			return lines.ToArray();
		}

		public void Run(string[] arguments)
		{
			foreach (var line in Query(arguments))
				_dispatch(line);
		}
		
		private IEnumerable<BaseCommandHandlerParameter> getUsages()
		{
			var commands = new List<BaseCommandHandlerParameter>();
			var usage = getUsage();
			new UsageParser(usage)
				.Parse().ToList()
					.ForEach(y =>
						{
							var cmd = new BaseCommandHandlerParameter(
								y.Name,
								y.Description,
								CommandType.FileCommand);
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
			var sb = new StringBuilder();
			run(arguments, (x) => sb.Append(x));
			return sb.ToString();
		}

		private void run(string arguments)
		{
			_execute(_path, arguments, null);
		}

        private void run(string arguments, Action<string> onLineReceived)
		{
			_execute(_path, arguments, onLineReceived);
		}
	}

	public class SignatureLocation
	{
		public string File { get; private set; }
		public string Signature { get; private set; }
		public Position Start { get; private set; }
		public Position End { get; private set; }

		public SignatureLocation(string file, string signature, Position start, Position end)
		{
			File = file;
			Signature = signature;
			Start = start;
			End = end;
		}
	}
}
