using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Core.Profiles;
using CoreExtensions;

namespace OpenIDE.Core.FileSystem
{
	public class Script
	{
		private string _file;
		private string _token;
		private string _workingDirectory;
		private string _localProfileName;
		private string _globalProfileName;

		public IEnumerable<BaseCommandHandlerParameter> Usages { get { return getUsages(); } }

		public string File { get { return _file; } }
		public string Name { get; private set; }
		public string Description { get; private set; }

		public Script(string token, string workingDirectory, string file)
		{
			_file = file;
			_token = token;
			Name = Path.GetFileNameWithoutExtension(file);
			Description = "";
			_workingDirectory = workingDirectory;
			var profiles = new ProfileLocator(_token);
			_globalProfileName = profiles.GetActiveGlobalProfile();
			_localProfileName = profiles.GetActiveLocalProfile();
		}

		public void Run(string arguments, Action<string> onLine)
		{
			arguments = "{global-profile} {local-profile} " + arguments;
			run(
				arguments,
				onLine,
				new[] {
						new KeyValuePair<string,string>("{global-profile}", "\"" + _globalProfileName + "\""),
						new KeyValuePair<string,string>("{local-profile}", "\"" + _localProfileName + "\"")
					});
		}

		private IEnumerable<BaseCommandHandlerParameter> getUsages()
		{
			var commands = new List<BaseCommandHandlerParameter>();
			var usage = getUsage();
			usage = stripDescription(usage);
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

		private string stripDescription(string usage)
		{
			var end = usage.IndexOf("|");
			if (end == -1)
			{
				Description = usage.Trim(new[] { '\"' });
				return "";
			}
			Description = usage.Substring(0, end).Trim(new[] { '\"' });
			return usage.Substring(
				end + 1,
				usage.Length - (end + 1));
		}
		
		private string getUsage()
		{
			return ToSingleLine("get-command-definitions");
		}

		private string ToSingleLine(string arguments)
		{
			var sb = new StringBuilder();
			run(arguments, (line) => sb.Append(line), new KeyValuePair<string,string>[] {});
			return sb.ToString();
		}

		private void run(string arguments, Action<string> onLine,
						 IEnumerable<KeyValuePair<string,string>> replacements)
		{
			var cmd = _file;
			var finalReplacements = new List<KeyValuePair<string,string>>();
			finalReplacements.Add(new KeyValuePair<string,string>("{run-location}", "\"" + _workingDirectory + "\""));
			finalReplacements.AddRange(replacements);
            arguments = "{run-location} " + arguments;
			var proc = new Process();
			proc
				.Query(
					cmd,
					arguments,
					false,
					_token,
					(error, line) => {
							if (error && !line.StartsWith("error|"))
								onLine("error|" + line);
							else
								onLine(line);
						},
					finalReplacements);
		}
	}
}
