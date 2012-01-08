using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

namespace OpenIDENet.Core.Language
{
	public class PluginLocator
	{
		private string _languageRoot;
		private Action<string> _dispatchMessage;

		public PluginLocator(string languageRoot, Action<string> dispatchMessage)
		{
			_languageRoot = languageRoot;
			_dispatchMessage = dispatchMessage;
		}

		public LanguagePlugin[] Locate()
		{
			return getPlugins()
				.Select(x => new LanguagePlugin(x, run, _dispatchMessage))
				.ToArray();
		}
		
		public IEnumerable<BaseCommandHandlerParameter> GetUsages()
		{
			var commands = new List<BaseCommandHandlerParameter>();
			getPlugins().ToList()
				.ForEach(x => 
					{
						var plugin = new LanguagePlugin(x, run, _dispatchMessage);
						plugin.GetUsages().ToList()
							.ForEach(y => commands.Add(y));
					});
			return commands;
		}

		private string[] getPlugins()
		{
			var dir = 
				Path.Combine(
					_languageRoot,
					"Languages");
			if (!Directory.Exists(dir))
				return new string[] {};
			return Directory.GetFiles(dir);
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
