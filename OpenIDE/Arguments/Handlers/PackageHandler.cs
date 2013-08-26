using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Packaging;
using OpenIDE.Core.Profiles;
using OpenIDE.Core.Config;
using OpenIDE.Core.Logging;
using CoreExtensions;

namespace OpenIDE.Arguments.Handlers
{
	class PackageHandler : ICommandHandler
	{
		private string _token;
		private Action<string> _dispatch;
		private PluginLocator _locator;
		private PackageFetcher _packageFetcher;

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
					.Add("SOURCE", "Ex. .OpenIDE/scripts/myscript, name or package.json");
				usage.Add("build", "Builds package")
					.Add("SOURCE", "Ex. .OpenIDE/scripts/myscript")
						.Add("[DESTINATION]", "Destination directory (default destination from config)");
				usage.Add("install", "Installs package")
					.Add("SOURCE", "Path to local file, URL or name")
						.Add("[-g]", "Installs to global profiles");
				usage.Add("update", "Updates package")
					.Add("SOURCE", "Path to local file, URL or name")
						.Add("[-g]", "Updates package in global profiles");
				usage.Add("uninstall", "Uninstalls package")
					.Add("SOURCE", "Ex. .OpenIDE/scripts/myscript or name");
				usage.Add("edit", "Opens package file in text editor")
					.Add("NAME", "Package name");

				var sources = usage.Add("src", "Lists, adds and removes package sources");
				sources
					.Add("add", "Add source")
						.Add("NAME", "Name of new source")
							.Add("LOCATION", "File reference/URL")
								.Add("[-g]", "Adds source to global profile");
				sources
					.Add("rm", "Remove source")
						.Add("NAME", "Name of source to remove");
				sources
					.Add("update", "Updates existing source list.")
						.Add("[NAME]", "Source name. If not specified it updates all");
				sources
					.Add("list", "Lists available packages from sources")
						.Add("[NAME]", "Source to list packages for");
				return usage;
			}
		}

		public string Command { get { return "package"; } }

		public PackageHandler(string token, Action<string> dispatch, PluginLocator locator) {
			_token = token;
			_dispatch = dispatch;
			_locator = locator;
			_packageFetcher = new PackageFetcher(_token, _dispatch);
		}

		public void Execute(string[] arguments) {
			if (arguments.Length == 0) {
				list();
				return;
			}
			if (arguments.Length == 2 && arguments[0] == "init")
				init(arguments[1]);
			if (arguments.Length == 2 && arguments[0] == "read")
				read(arguments[1]);
			if (new[] {2,3}.Contains(arguments.Length) && arguments[0] == "build")
				build(arguments);
			if (arguments.Length > 1 && arguments[0] == "install")
				install(arguments);
			if (arguments.Length > 1 && arguments[0] == "update")
				update(arguments);
			if (arguments.Length > 1 && arguments[0] == "uninstall")
				remove(arguments);
			if (arguments.Length > 1 && arguments[0] == "edit")
				edit(arguments);
			if (arguments[0] == "src")
				sourceCommands(arguments);
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
			_dispatch("command|editor goto \"" + packageFile + "|0|0\"");
		}

		private void read(string source) {
			var pkg = 
				getPackages()
					.FirstOrDefault(x => x.ID == source);
			if (pkg == null) {
				if (!File.Exists(source)) {
					var dir = 
						Path.Combine(
							Path.GetDirectoryName(source),
							Path.GetFileNameWithoutExtension(source) + "-files");
					source = Path.Combine(dir, "package.json");
				}
			} else {
				source = pkg.File;
			}
			
			if (!File.Exists(source)) 
				return;

			var package = Package.Read(source);
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
		
		private Package getInstallPackage(string source, string tempPath) {
			new PackageExtractor().Extract(source, tempPath);
			var pkgFile =
				Path.Combine(
					Path.Combine(
						tempPath,
						Path.GetFileName(Directory.GetDirectories(tempPath)[0])),
					"package.json");
			return Package.Read(pkgFile);
		}

		private void list() {
			printPackages(getPackages());
		}

		private IEnumerable<Package> getPackages() {
			var profiles = new ProfileLocator(_token);
			var packages = new List<Package>();
			profiles.GetFilesCurrentProfiles("package.json").ToList()
				.ForEach(x => {
						try {
							Logger.Write("Reading package " + x);
							var package = Package.Read(x);
							if (package != null)
								packages.Add(package);
						} catch {
						}
					});
			return packages;
		}

		private void printPackages(IEnumerable<Package> packages) {
			packages.ToList()
				.ForEach(x => Console.WriteLine(x.ID + " (" + x.Version + ")"));
		}

		private string getPackageDescription(string dir, string name) {
			var NL = Environment.NewLine;
			var type = getType(dir);
			var language = getLanguage(type, dir);
			var languageText = "";
			if (language != null)
				languageText = "\t\"language\": \"" + language + "\"," + NL;
			var package = 
					"{" + NL +
					"\t\"#Comment\": \"# is used here to comment out optional fields\"," + NL +
					"\t\"#Comment\": \"pre and post install actions accepts only OpenIDE non edior commands\"," + NL +
					"\t\"target\": \"{1}\"," + NL +
					languageText +
					"\t\"id\": \"{0}\"," + NL +
					"\t\"version\": \"v1.0\"," + NL +
					"\t\"description\": \"{0} {1} package\"," + NL +
					"\t\"#config-prefix\": \"{0}.\"," + NL +
					"\t\"#pre-install-actions\": []," + NL +
					"\t\"#post-install-actions\": []," + NL +
					"\t\"#dependencies\": [" + NL +
					"\t\t\t{" + NL +
					"\t\t\t\t\"id\": \"name\"," + NL +
					"\t\t\t\t\"versions\":" + NL +
					"\t\t\t\t[" + NL +
					"\t\t\t\t\t\"v1.0\"" + NL +
					"\t\t\t\t]" + NL +
					"\t\t\t}" + NL +
					"\t\t]" + NL +
					"}";
			return package
						.Replace("{0}", name)
						.Replace("{1}", type);
		}

		private string getType(string dir) {
			var type = Path.GetFileName(dir);
			var subDir = Path.GetDirectoryName(dir);
			if (subDir != null) {
				subDir = Path.GetDirectoryName(subDir);
				if (subDir != null) {
					if (Path.GetFileName(subDir) == "languages")
						type = "language-" + type;
				}
			}
			return type.Substring(0, type.Length - 1);
		}

		private string getLanguage(string type, string dir) {
			if (!type.StartsWith("language-"))
				return null;
			var dirName = Path.GetFileName(Path.GetDirectoryName(dir));
			return dirName.Substring(0, dirName.IndexOf("-files"));
		}

		private void build(string[] args) {
			string source = args[1];
			string destination = null;
			if (args.Length == 3) {
				destination = Path.GetFullPath(args[2]);
			} else {
				var setting = new ConfigReader(_token).Get("default.package.destination");
				if (setting != null)
					destination = setting;
			}
			if (!Directory.Exists(destination))
				return;

			string name;
			string dir;
			var package = 
				getPackages()
					.FirstOrDefault(x => x.ID == source);
			if (package != null) {
				name = package.ID;
				dir = Path.GetDirectoryName(Path.GetDirectoryName(package.File));
			} else {
				source = Path.GetFullPath(source);
				name = Path.GetFileNameWithoutExtension(source);
				dir = Path.GetDirectoryName(source);
			}

			destination = Path.GetFullPath(destination);
			
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
			var useGlobal = globalSpecified(ref args);
			var installer = new Installer(_token, _dispatch, _locator);
			installer.UseGlobalProfiles(useGlobal);
			installer.Install(args[1]);
		}

		private void install(string package, bool isGlobal) {
			
		}
		
		private void update(string[] args) {
			var useGlobal = globalSpecified(ref args);
			var installer = new Installer(_token, _dispatch, _locator);
			installer.UseGlobalProfiles(useGlobal);
			installer.Update(args[1]);
		}

		private bool globalSpecified(ref string[] args) {
			var useGlobal = false;
			var newArgs = new List<string>();
			foreach (var arg in args) {
				if (arg == "-g") {
					useGlobal = true;
					continue;
				}
				newArgs.Add(arg);
			}
			args = newArgs.ToArray();
			return useGlobal;
		}

		private PackageFetcher.FetchedPackage downloadPackage(string source) {
			return _packageFetcher.Fetch(source);
		}
		
		private void remove(string[] args) {
			var source = args[1];
			var installer = new Installer(_token, _dispatch, _locator);
			installer.Remove(source);
		}

		private void edit(string[] args) {
			var source = args[1];
			var package = 
				getPackages()
					.FirstOrDefault(x => x.ID == source);
			if (package != null) {
				_dispatch("command|editor goto \"" + package.File + "|0|0\"");
			}
		}
		
		private void sourceCommands(string[] args) {
			var locator = new SourceLocator(_token);
			if (args.Length == 1) {
				locator
					.GetSources().ToList()
					.ForEach(x => 
						Console.WriteLine(x.Name + " - " + x.Origin));
				return;
			}
			var useGlobal = globalSpecified(ref args);
			var path = locator.GetLocalDir();
			if (useGlobal)
				path = locator.GetGlobalDir();
			if (args.Length == 4 && args[1] == "add") {
				if (path == null) {
					printError("Config point is not initialized");
					return;
				}
				var name = args[2];
				var sources = locator.GetSourcesFrom(path);
				if (sources.Any(x => x.Name == name)) {
					printError("There is already a source named " + name);
					return;
				}
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				var destination = Path.Combine(path, name + ".source");
				download(args[3], destination);
				if (!File.Exists(destination))
					printError("Failed while downloading source file " + args[3]);
				return;
			}
			if (args.Length == 3 && args[1] == "rm") {
				var name = args[2];
				var source = 
					locator
						.GetSources()
						.FirstOrDefault(x => x.Name == name);
				if (source == null) {
					printError("There is no package source named " + name);
					return;
				}
				File.Delete(source.Path);
				return;
			}
			if (args.Length > 1 && args[1] == "update") {
				string name = null;
				if (args.Length > 2)
					name = args[2];
				var sources = 
					locator
						.GetSources()
						.Where(x => name == null || x.Name == name);
				if (sources.Count() == 0) {
					printError("There are no package sources to update");
					return;
				}
				foreach (var source in sources) {
					if (!download(source.Origin, source.Path))
						printError("Failed to download source file " + source.Origin);
				}
				return;
			}
			if (args.Length > 1 && args[1] == "list") {
				string name = null;
				if (args.Length > 2)
					name = args[2];
				var sources = 
					locator
						.GetSources()
						.Where(x => name == null || x.Name == name);
				foreach (var source in sources) {
					_dispatch("Packages in " +  source.Name);
					foreach (var package in source.Packages)
						_dispatch("\t" + package.ToString());
				}
			}
		}

		private bool download(string source, string destination) {
			return new FileFetcher(_dispatch).Download(source, destination);
		}

		private void printError(string msg) {
			_dispatch("error|" + msg);
		}
	}
}