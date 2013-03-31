using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Packaging;
using OpenIDE.Core.Profiles;
using CoreExtensions;

namespace OpenIDE.Arguments.Handlers
{
	class PackageHandler : ICommandHandler
	{
		private string _token;
		private Action<string> _dispatch;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Package management - no arguments lists installed packages");
				usage.Add("init", "Initialize package for script, rscript or language")
					.Add("SOURCE", "Ex. .OpenIDE/scripts/myscript");
				usage.Add("read", "reads package contents")
					.Add("SOURCE", "Ex. .OpenIDE/scripts/myscript or package.json");
				usage.Add("build", "Builds package")
					.Add("SOURCE", "Ex. .OpenIDE/scripts/myscript")
						.Add("DESTINATION", "Directory to write package to");
				usage.Add("install", "Installs package")
					.Add("SOURCE", "Path to local file or URL");
				return usage;
			}
		}

		public string Command { get { return "package"; } }

		public PackageHandler(string token, Action<string> dispatch) {
			_token = token;
			_dispatch = dispatch;
		}

		public void Execute(string[] arguments) {
			if (arguments.Length == 0)
				list();
			if (arguments.Length == 2 && arguments[0] == "init")
				init(arguments[1]);
			if (arguments.Length == 2 && arguments[0] == "read")
				read(arguments[1]);
			if (arguments.Length == 3 && arguments[0] == "build")
				build(arguments[1], arguments[2]);
			if (arguments.Length > 1 && arguments[0] == "install")
				install(arguments);
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

		private void read(string source) {
			if (!File.Exists(source)) {
				var dir = 
					Path.Combine(
						Path.GetDirectoryName(source),
						Path.GetFileNameWithoutExtension(source) + "-files");
				source = Path.Combine(dir, "package.json");
			}

			if (!File.Exists(source))
				return;

			var package = Package.Read(File.ReadAllText(source));
			if (package != null) {
				Console.WriteLine(package.ToVerboseString());
				return;
			}

			var tempPath = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
			Directory.CreateDirectory(tempPath);
			try {
				package = getInstallPackage(source, tempPath);
				if (package != null)
					Console.WriteLine(package.ToVerboseString());
			} catch {
			} finally {
				Directory.Delete(tempPath, true);
			}
		}

		private void list() {
			var profiles = new ProfileLocator(_token);
			printPackages(profiles.GetLocalProfilePath("default"));
			printPackages(profiles.GetGlobalProfilePath("default"));
		}

		private void printPackages(string dir) {
			getPackages(dir)
				.ForEach(x => {
						try {
							var package = Package.Read(File.ReadAllText(x));
							if (package != null)
								Console.WriteLine(package.ToString() + " - " + x);
						} catch {
						}
					});
		}

		private List<string> getPackages(string directory) {
			return Directory
				.GetFiles(directory, "package.json", SearchOption.AllDirectories)
				.ToList();
		}

		private string getPackageDescription(string dir, string name) {
			var type = Path.GetFileName(dir);
			type = type.Substring(0, type.Length - 1);
			var NL = Environment.NewLine;
			var package = 
					"{" + NL +
					"\t\"#Comment\": \"# is used here to comment out optional fields\"," + NL +
					"\t\"target\": \"{1}\"," + NL +
					"\t\"id\": \"{0}-v1.0\"," + NL +
					"\t\"description\": \"{0} {1} package\"," + NL +
					"\t\"#config-prefix\": \"{0}.\"," + NL +
					"\t\"#pre-install-actions\": []," + NL +
					"\t\"#post-install-actions\": []," + NL +
					"\t\"#dependencies\": []" + NL +
					"}";
			return package.Replace("{0}", name).Replace("{1}", type);
		}

		private void build(string source, string destination) {
			source = Path.GetFullPath(source);
			destination = Path.GetFullPath(destination);
			var name = Path.GetFileNameWithoutExtension(source);
			var dir = Path.GetDirectoryName(source);
			var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			new Process()
				.Query(
					Path.Combine(Path.Combine(appDir, "Packaging"), "oipckmngr.exe"),
					string.Format("build \"{0}\" \"{1}\" \"{2}\"", name, dir, destination),
					false,
					Environment.CurrentDirectory,
					(err, line) => { Console.WriteLine(line); });
		}

		private void install(string[] args) {
			var source = Path.GetFullPath(args[1]);
			if (!File.Exists(source))
				return;
			var profiles = new ProfileLocator(_token);
			var activeProfile = profiles.GetActiveLocalProfile();
			var installPath = profiles.GetLocalProfilePath(activeProfile);
			var tempPath = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
			Directory.CreateDirectory(tempPath);
			try {
				var package = getInstallPackage(source, tempPath);
				if (package != null) {
					var name = 
						Path.GetFileNameWithoutExtension(
							Directory.GetFiles(tempPath)[0]);
					installPath = Path.Combine(installPath, package.Target + "s");
					var matches = 
						Directory.GetFiles(installPath)
							.Where(x => matchPackage(x, name));
					if (matches.Count() > 0)
						printConflictingPackage(name, package, matches);
					else
						installPackage(source, package, installPath, activeProfile);
				}
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			} finally {
				Directory.Delete(tempPath, true);
			}
		}

		private void installPackage(string source, Package package, string installPath, string activeProfile) {
			extractPackage(source, installPath);
			Console.WriteLine("Installed {1} package {0} in profile {2}",
				package.ID,
				package.Target,
				activeProfile);
		}

		private Package getPackage(string file) {
			var path = Path.GetDirectoryName(file);
			var name = Path.GetFileNameWithoutExtension(file);
			var pkgFile =
				Path.Combine(
					Path.Combine(path, name + "-files"),
					"package.json");
			if (File.Exists(pkgFile))
				return Package.Read(File.ReadAllText(pkgFile));
			return null;
		}

		private void printConflictingPackage(string name, Package package, IEnumerable<string> matches) {
			var pkgInfo = "";
			var existingPackage = getPackage(matches.First());
			if (existingPackage != null)
				pkgInfo = string.Format(" ({0})", existingPackage.ID);
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(
				"There is already an installed {2} package called {0}{1}",
				name,
				pkgInfo,
				package.Target);
			Console.ResetColor();
			if (existingPackage != null) {
				Console.WriteLine();
				Console.WriteLine(existingPackage.ToVerboseString());
			}
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("To replace/update the installed package use the update command");
			Console.ResetColor();
		}

		private bool matchPackage(string path, string name) {
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
				return Path.GetFileNameWithoutExtension(path) == name;
			else
				return Path.GetFileNameWithoutExtension(path).ToLower() == name.ToLower();
		}

		private Package getInstallPackage(string source, string tempPath) {
			extractPackage(source, tempPath);
			var pkgFile =
				Path.Combine(
					Path.Combine(
						tempPath,
						Path.GetFileName(Directory.GetDirectories(tempPath)[0])),
					"package.json");
			return Package.Read(File.ReadAllText(pkgFile));
		}

		private void extractPackage(string source, string path) {
			var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			new Process()
				.Query(
					Path.Combine(Path.Combine(appDir, "Packaging"), "oipckmngr.exe"),
					string.Format("extract \"{0}\" \"{1}\"", source, path),
					false,
					Environment.CurrentDirectory,
					(err, line) => { });
		}
	}
}