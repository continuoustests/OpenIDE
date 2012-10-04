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
					name.Add("[--language=LANGUAGE]", "Language to add the reactive script to")
						.Add("[-l=LANGUAGE]", "Short for --language");
					return usage;
			}
		}
	
		public string Command { get { return "rscript-create"; } }
		
		public CreateReactiveScriptHandler(Action<string> dispatch)
		{
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
			var file = Path.Combine(
				getPath(arguments),
				filename);
			if (extension != null)
				file += extension;
			if (File.Exists(file))
				return;
			Console.WriteLine("Creating " + file);
			PathExtensions.CreateDirectories(file);
			var template = new ReactiveScriptLocator().GetTemplateFor(extension);
			var content = "";
			if (template != null)
				File.Copy(template, file);
			else 
			{
				var templates = new ReactiveScriptLocator().GetTemplates().ToArray();
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

			_dispatch("editor goto \"" + file + "|0|0\"");
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
				return new ReactiveScriptLocator().GetGlobalPath();
			else if (arguments.Count(x => x.StartsWith("--language=")) > 0 || 
					 arguments.Count(x => x.StartsWith("-l=")) > 0)
				return new ReactiveScriptLocator().GetLanguagePath(getLanguage(arguments));
			else
				return new ReactiveScriptLocator().GetLocalPath();
		}

		private string getLanguage(string[] arguments)
		{
			var language = getLanguage("--language=", arguments);
			if (language != null)
				return language;
			return getLanguage("--l", arguments);
		}

		private string getLanguage(string pattern, string[] arguments)
		{
			var parameter = arguments.FirstOrDefault(x => x.StartsWith(pattern));
			if (parameter != null)
				return parameter.Substring(pattern.Length, parameter.Length - pattern.Length);
			return null;
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
