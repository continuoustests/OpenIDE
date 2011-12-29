using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDENet.Arguments;
using OpenIDENet.Arguments.Handlers;
namespace OpenIDENet.Language
{
	public class PluginLocator
	{
		public string[] Locate()
		{
			return getPlugins()
				.Where(x => hasUsages(x)).ToArray();
		}

		public string GetLanguage(string plugin)
		{
			return run(plugin, "get-language");
		}
		
		public IEnumerable<CommandHandlerParameter> GetUsages()
		{
			return getUsages();
		}
		
		private bool hasUsages(string plugin)
		{
			var usage = getUsage(plugin);
			return new UsageParser(usage).Parse().Length > 0;
		}

		private IEnumerable<CommandHandlerParameter> getUsages()
		{
			var commands = new List<CommandHandlerParameter>();
			getPlugins().ToList()
				.ForEach(x => 
					{
						new UsageParser(getUsage(x))
							.Parse().ToList()
								.ForEach(y =>
									{
										var cmd = new CommandHandlerParameter(
											GetLanguage(x),
											CommandType.FileCommand,
											y.Name,
											y.Description);
										y.Parameters.ToList()
											.ForEach(p => cmd.Add(p));
										if (!y.Required)
											cmd.IsOptional();
										commands.Add(cmd);
									});
					});
				return commands;
		}

		private string getUsage(string plugin)
		{
			return run(plugin, "get-command-definitions");
		}

		private string[] getPlugins()
		{
			return Directory.GetFiles(
				Path.Combine(
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					"Languages"));
		}

		private string run(string cmd, string arguments)
		{
			var proc = new Process();
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
			    proc.StartInfo = new ProcessStartInfo(cmd, arguments);
            else
                proc.StartInfo = new ProcessStartInfo("cmd.exe", "/c \"" + cmd + "\" " + arguments);
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.Start();
			var output = proc.StandardOutput.ReadToEnd();
			proc.WaitForExit();
			if (output.Length > Environment.NewLine.Length)
				return output.Substring(0, output.Length - Environment.NewLine.Length);
			return output;
		}
	}
}
