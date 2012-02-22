using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.Config;
using OpenIDE.Core.Language;

namespace OpenIDE.FileSystem
{
	class ScriptLocator
	{
		public string GetTemplateFor(string extension)
		{
			if (extension == null)
				return null;
			var dir =
				Path.Combine(
					GetGlobalPath(),
					"templates");
			if (!Directory.Exists(dir))
				return null;
			return Directory.GetFiles(dir)
				.Where(x => x.EndsWith(extension))
				.FirstOrDefault();
		}

		public string[] GetGlobalScripts()
		{
			return null;
		}

		public string[] GetLocalScripts()
		{
			return null;
		}

		public string GetGlobalPath()
		{
			return getPath(Path
					.GetDirectoryName(
						Assembly.GetExecutingAssembly().Location));
		}

		public string GetLocalPath()
		{
			var configFile = Configuration.GetConfigFile(Environment.CurrentDirectory);
			if (configFile == null)
				return null;
			return getPath(Path.GetDirectoryName(configFile));
		}

		private string getPath(string location)
		{
			return Path.Combine(location, "scripts");
		}
	}

	class Script
	{
		private string _file;

		public IEnumerable<BaseCommandHandlerParameter> Usages { get { return getUsages(); } }

		public string Name { get; private set; }

		public Script(string file)
		{
			_file = file;
			Name = Path.GetFileNameWithoutExtension(file);
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
            if (Environment.OSVersion.Platform != PlatformID.Unix && Environment.OSVersion.Platform != PlatformID.MacOSX)
			{
                arguments = "/c \"" + ("\"" + cmd + "\" " + arguments).Replace ("\"", "^\"") + "\"";
				cmd = "cmd.exe";
			}
			proc.StartInfo = new ProcessStartInfo(cmd, arguments);
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.Start();
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
