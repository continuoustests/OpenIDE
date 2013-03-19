using System;
using System.IO;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Core.FileSystem;

namespace OpenIDE.Arguments.Handlers
{
	class PackageHandler : ICommandHandler
	{
		private Action<string> _dispatch;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Package management");
				usage.Add("init", "Initialize package for script, rscript or language")
					.Add("SOURCE", "Ex. .OpenIDE/scripts/myscript.");
				return usage;
			}
		}

		public string Command { get { return "package"; } }

		public PackageHandler(Action<string> dispatch) {
			_dispatch = dispatch;
		}

		public void Execute(string[] arguments) {
			if (arguments.Length == 2 && arguments[0] == "init")
				init(arguments[1]);
		}

		private void init(string source) {
			source = Path.GetFullPath(source);
			var name = Path.GetFileNameWithoutExtension(source);
			var dir = Path.GetDirectoryName(source);
			if (!Directory.Exists(dir))
				return;
			var files = Path.Combine(dir, name + "-files");
			if (!Directory.Exists(files))
				Directory.CreateDirectory(files);
			var packageFile = Path.Combine(files, "package.json");
			if (!File.Exists(packageFile))
				File.WriteAllText(packageFile, getPackageDescription(dir, name));
			_dispatch("editor goto \"" + packageFile + "|0|0\"");
		}

		private string getPackageDescription(string dir, string name) {
			var type = Path.GetFileName(dir);
			type = type.Substring(0, type.Length - 1);
			var NL = Environment.NewLine;
			var package = 
					"{" + NL +
					"\t\"#Comment\": \"# is used here to comment out optional fields\"," + NL +
					"\t\"target\": \"{1}\"," + NL +
					"\t\"id\": \"{0} v1.0\"," + NL +
					"\t\"description\": \"{0} {1} package\"," + NL +
					"\t\"#config-prefix\": \"{0}.\"," + NL +
					"\t\"#pre-install-actions\": []," + NL +
					"\t\"#post-install-actions\": []," + NL +
					"\t\"#dependencies\": []" + NL +
					"}";
			return package.Replace("{0}", name).Replace("{1}", type);
		}
	}
}