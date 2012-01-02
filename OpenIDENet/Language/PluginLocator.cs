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
		public LanguagePlugin[] Locate()
		{
			return getPlugins()
				.Select(x => new LanguagePlugin(x, run))
				.Where(x => x.GetUsages().Count() > 0).ToArray();
		}
		
		public IEnumerable<CommandHandlerParameter> GetUsages()
		{
			var commands = new List<CommandHandlerParameter>();
			getPlugins().ToList()
				.ForEach(x => 
					{
						var plugin = new LanguagePlugin(x, run);
						plugin.GetUsages().ToList()
							.ForEach(y => commands.Add(y));
					});
			return commands;
		}

		private string[] getPlugins()
		{
			return Directory.GetFiles(
				Path.Combine(
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					"Languages"));
		}

		private IEnumerable<string> run(string cmd, string arguments)
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
				output = output.Substring(0, output.Length - Environment.NewLine.Length);
			return output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		}
	}
}
