using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.Language;

namespace OpenIDE.FileSystem
{
	class Script
	{
		private string _file;
		private string _workingDirectory;

		public IEnumerable<BaseCommandHandlerParameter> Usages { get { return getUsages(); } }

		public string File { get { return _file; } }
		public string Name { get; private set; }
		public string Description { get; private set; }

		public Script(string workingDirectory, string file)
		{
			_file = file;
			Name = Path.GetFileNameWithoutExtension(file);
			Description = "";
			_workingDirectory = workingDirectory;
		}

		public IEnumerable<string> Run(string arguments)
		{
			return run(arguments);
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
			Description = usage.Substring(0, end + 1).Trim(new[] { '\"' });
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
			run(arguments).ToList()
				.ForEach(x => 
					{
						sb.Append(x);
					});
			return sb.ToString();
		}

		private IEnumerable<string> run(string arguments)
		{
			var cmd = _file;
			var proc = new Process();
			var startedSuccessfully = true;
			try {
				arguments = "\"" + Environment.CurrentDirectory + "\" " + arguments;
				if (Environment.OSVersion.Platform != PlatformID.Unix &&
					Environment.OSVersion.Platform != PlatformID.MacOSX)
				{
					arguments = "/c \"" + ("\"" + cmd + "\" " + arguments).Replace ("\"", "^\"") + "\"";
					cmd = "cmd.exe";
				}
				proc.StartInfo = new ProcessStartInfo(cmd, arguments);
				proc.StartInfo.CreateNoWindow = true;
				proc.StartInfo.UseShellExecute = false;
				proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				proc.StartInfo.RedirectStandardOutput = true;
				proc.StartInfo.WorkingDirectory = _workingDirectory;
				proc.Start();
			} catch {
				startedSuccessfully = false;
			}
			if (startedSuccessfully)
			{
				while (true)
				{
					var line = proc.StandardOutput.ReadLine();
					if (line == null)
						break;
					yield return line;
				}
			}
		}
	}
}
