using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Config;
using OpenIDE.Core.Language;
using OpenIDE.FileSystem;

namespace OpenIDE.Arguments.Handlers
{
	class CreateScriptHandler : ICommandHandler
	{
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

		public string Command { get { return "script-create"; } }

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
			var template = new ScriptLocator().GetTemplateFor(extension);
			var content = "";
			if (template != null)
				content = File.ReadAllText(template).Replace("[[scirpt_name]]", filename);
			File.WriteAllText(file, content);
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
				return new ScriptLocator().GetGlobalPath();
			else
				return new ScriptLocator().GetLocalPath();
		}
	}
}
