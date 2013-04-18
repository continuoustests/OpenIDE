using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.Config;
using OpenIDE.Core.Language;
using OpenIDE.Core.FileSystem;
using CoreExtensions;

namespace OpenIDE.Arguments.Handlers
{
	class CreateScriptHandler : ICommandHandler
	{
		private string _token;
		private Action<string> _dispatch;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Creates a script. Run scripts with 'oi x [script-name]'.");
					usage.Add("SCRIPT-NAME", "Script name with optional file extension.")
						.Add("[--global]", "Will create the new script in the main script folder")
							.Add("[-g]", "Short for --global");
				return usage;
			}
		}

		public string Command { get { return "new"; } }

		public CreateScriptHandler(string token, Action<string> dispatch)
		{
			_dispatch = dispatch;
			_token = token;
		}

		public void Execute(string[] arguments)
		{
			if (arguments.Length < 1)
				return;
			var filename = getFileName(arguments[0]);
			if (filename == null)
				return;
			var extension = getExtension(arguments[0]);
			var file = Path.Combine(
				getPath(arguments),
				filename);
			if (extension != null)
				file += extension;
			if (File.Exists(file))
				return;
			PathExtensions.CreateDirectories(file);
			var template = new ScriptLocator(_token, Environment.CurrentDirectory).GetTemplateFor(extension);
			var content = "";
			if (template != null)
				File.Copy(template, file);
			else
			{
				var templates = new ScriptLocator(_token, Environment.CurrentDirectory).GetTemplates().ToArray();
				if (templates.Length == 0)
				{
					File.WriteAllText(file, content);
					if (Environment.OSVersion.Platform == PlatformID.Unix ||
						Environment.OSVersion.Platform == PlatformID.MacOSX)
						run("chmod", "+x \"" + file + "\"");
				}
				else
				{
					File.WriteAllText(file, "");
					File.Copy(templates[0], file);
				}
			}
			_dispatch("command|editor goto \"" + file + "|0|0\"");
		}

		private string getFileName(string name)
		{
			var extension = getExtension(name);
			if (extension == null)
				return name;
			else
				return name.Substring(0, name.Length - extension.Length);
		}

		private string getExtension(string name)
		{
			return Path.GetExtension(name);
		}
		
		private string getPath(string[] arguments)
		{
			if (arguments.Contains("--global") || arguments.Contains("-g"))
				return new ScriptLocator(_token, Environment.CurrentDirectory).GetGlobalPath();
			else
				return new ScriptLocator(_token, Environment.CurrentDirectory).GetLocalPath();
		}
		
		private void run(string cmd, string arguments)
		{
			try {
				var proc = new Process();
                proc.Run(cmd, arguments, false, Environment.NewLine);
			} catch {
			}
		}
	}
}
