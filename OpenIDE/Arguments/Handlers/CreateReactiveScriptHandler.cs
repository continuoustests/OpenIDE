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
	class CreateReactiveScriptHandler : ICommandHandler
	{
		private string _token;
		private Action<string> _dispatch;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Creates a script that is triggered by it's specified events");
					var name = usage.Add("SCRIPT-NAME", "Script name with optional file extension.");
					name.Add("[--global]", "Will create the new script in the main script folder")
						.Add("[-g]", "Short for --global");
					return usage;
			}
		}
	
		public string Command { get { return "new"; } }
		
		public CreateReactiveScriptHandler(string token, Action<string> dispatch)
		{
			_token = token;
			_dispatch = dispatch;
		}

		public void Execute(string[] arguments)
		{
			if (arguments.Length < 1)
				return;
			var filename = getFileName(arguments[0]);
			if (filename == null)
				return;
			var extension = getExtension(arguments[0]);
			var path = getPath(arguments);
			if (path == null)
				return;
			var file = Path.Combine(
				path,
				filename);
			if (extension != null)
				file += extension;
			if (File.Exists(file))
				return;
			Console.WriteLine("Creating " + file);
			PathExtensions.CreateDirectories(file);
			var template = new ReactiveScriptLocator(_token, Environment.CurrentDirectory).GetTemplateFor(extension);
			var content = "";
			if (template != null)
				File.Copy(template, file);
			else 
			{
				var templates = new ReactiveScriptLocator(_token, Environment.CurrentDirectory).GetTemplates().ToArray();
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
			var isGlobal = arguments.Contains("--global") || arguments.Contains("-g");
			var locator = new ReactiveScriptLocator(_token, Environment.CurrentDirectory);
			if (isGlobal)
				return locator.GetGlobalPath();
			else
				return locator.GetLocalPath();
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
