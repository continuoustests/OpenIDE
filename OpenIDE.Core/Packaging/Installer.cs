using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.Profiles;
using OpenIDE.Core.Logging;
using OpenIDE.Core.Language;
using CoreExtensions;

namespace OpenIDE.Core.Packaging
{
	public class Installer
	{
		class ActionParameters
		{
			public Package Package { get; set; }
			public string TempPath { get; set; }
			public string InstallPath { get; set; }
			public string ProfileName { get; set; }
			public string Match { get; set; }
		}

		private string _token;
		private Action<string> _dispatch;
		private bool _useGlobal = false;
		private PluginLocator _locator;
		private PackageFetcher _packageFetcher;

		public Installer(string token, Action<string> dispatch, PluginLocator locator) {
			_token = token;
			_dispatch = dispatch;
			_locator = locator;
			_packageFetcher = new PackageFetcher(_token, _dispatch);
		}

		public void UseGlobalProfiles(bool useGlobal) {
			_useGlobal = useGlobal;
		}

		public void Install(string packageToken) {
			install(packageToken, null);
		}

		private bool install(string packageToken, string[] acceptedVersions) {
			var installType = "package";
			if (acceptedVersions != null)
				installType = "dependency";
			_dispatch("Installing " + installType + " " + packageToken);
			var source = _packageFetcher.Fetch(packageToken);
			if (source == null || !File.Exists(source.Package)) {
				_dispatch("error|could not find package " + packageToken);
				return false;
			}

			var actionSucceeded = prepareForAction(
				source.Package,
				(args) => {
						if (args.Match != null) {
							Logger.Write("Found matching package " + args.Match);
							var package = getPackage(Path.GetFileNameWithoutExtension(args.Match));
							Logger.Write("Loaded package " + package.ID);
							if (acceptedVersions != null) {
								if (acceptedVersions.Length > 0 && !acceptedVersions.Any(x => x == package.Version)) {
									var versions = "";
									foreach (var version in acceptedVersions) {
										versions += version + ",";
									}
									versions = versions.Trim(new[] {','});
									_dispatch(string.Format("error|dependency {0} ({1}) is installed. Accepted versions are {2}", args.Package.ID, package.Version, versions));
									return false;
								}
							}
							_dispatch(string.Format("package {0} ({1}) is already installed", package.ID, package.Version));
						} else if (acceptedVersions != null && !acceptedVersions.Any(x => x == args.Package.Version)) {
							var versions = "";
							foreach (var version in acceptedVersions) {
								versions += version + ",";
							}
							versions = versions.Trim(new[] {','});
							_dispatch(string.Format("error|dependency {0} of version {1} is not a valid. Accepted versions are {2}", args.Package.ID, args.Package.Version, versions));
							return false;
						} else
							installPackage(source.Package, args.Package, args.TempPath, args.InstallPath, args.ProfileName);
						return true;
					});
			if (source.IsTemporaryPackage)
				File.Delete(source.Package);
			return actionSucceeded;
		}

		public void Update(string packageToken) {
			var source = _packageFetcher.Fetch(packageToken);
			if (!File.Exists(source.Package))
				return;
			prepareForAction(
				source.Package,
				(args) => {
						if (args.Match == null)
							printUnexistingUpdate(args.Package.ID, args.Package);
						else
							update(source.Package, args);
						return true;
					});
			if (source.IsTemporaryPackage)
				File.Delete(source.Package);
		}

		public void Remove(string source) {
			var package = getPackage(source);
			if (package == null) {
				_dispatch(string.Format("error|There is no package {0} to remove", source));
				return;
			}
			removePackage(package.Command, Path.GetDirectoryName(Path.GetDirectoryName(package.File)));
			_dispatch(string.Format("Removed package {0}", package.Signature));
		}
		
		private bool prepareForAction(string source, Func<ActionParameters,bool> actionHandler) {
			var profiles = new ProfileLocator(_token);
			string activeProfile;
			

			var actionSucceeded = false;
			var tempPath = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
			Directory.CreateDirectory(tempPath);
			try {
				var package = getInstallPackage(source, tempPath);
				if (package != null) {
					// Force language to global as that is the only thing workin atm
					if (package.Target == "language")
						_useGlobal = true;
					if (_useGlobal)
						activeProfile = profiles.GetActiveGlobalProfile();
					else
						activeProfile = profiles.GetActiveLocalProfile();

					var installPath = getInstallPath(package, profiles, activeProfile);
					if (installPath == null) {
						_dispatch("error|Config point is not initialized");
						return false;
					}
					if (!Directory.Exists(installPath))
						Directory.CreateDirectory(installPath);
					var match =  
						Directory.GetFiles(installPath)
							.FirstOrDefault(x => matchPackage(x, package.Command));
					actionSucceeded = actionHandler(
						new ActionParameters() {
							Package = package,
							TempPath = tempPath,
							InstallPath = installPath,
							ProfileName = activeProfile,
							Match = match
						});
				}
			} catch (Exception ex) {
				_dispatch("error|" + ex.ToString());
			} finally {
				Directory.Delete(tempPath, true);
			}
			return actionSucceeded;
		}

		private Package getPackage(string name) {
			Logger.Write("Looking for package " + name);
			var profiles = new ProfileLocator(_token);
			var packages = new List<Package>();
			profiles.GetFilesCurrentProfiles("package.json").ToList()
				.ForEach(x => {
						try {
							Logger.Write("Reading package " + x);
							var package = Package.Read(x);
							if (package != null) {
								Logger.Write("Adding package {0} ({1})", package.ID, package.Version);
								packages.Add(package);
							}
						} catch (Exception ex) {
							Logger.Write(ex);
						}
					});
			return packages.FirstOrDefault(x => x.ID == name);
		}

		private string getInstallPath(Package package, ProfileLocator profiles, string activeProfile) {
			string installPath;
			if (package.Target.StartsWith("language-"))
				return getLanguageInstallPath(package);
			if (_useGlobal)
				installPath = profiles.GetGlobalProfilePath(activeProfile);
			else
				installPath = profiles.GetLocalProfilePath(activeProfile);
			if (installPath == null)
				return null;
			return Path.Combine(installPath, package.Target + "s");
		}

		private string  getLanguageInstallPath(Package package) {
			var language = _locator
				.Locate()
				.FirstOrDefault(x => x.GetLanguage() == package.Language);
			return
				Path.Combine(
					Path.Combine(
						Path.GetDirectoryName(language.FullPath),
						language.GetLanguage() + "-files"),
					package.Target.Replace("language-", "") + "s");
		}

		private void update(string source, ActionParameters args) {
			if (args.Match == null) {
				_dispatch("error|The requested package is not installed. Try install instead.");
				return;
			}
			var existingPackage = getPackage(Path.GetFileNameWithoutExtension(args.Match));
			if (!runInstallVerify(args.TempPath, args.InstallPath)) {
				printUpdateFailed(args.Package.Signature);
				return;
			}
			if (!runUpgrade(args.TempPath, args.InstallPath, "before-update")) {
				printUpdateFailed(args.Package.Signature);
				return;
			}

			removePackage(args.Package.Command, args.InstallPath);
			new PackageExtractor().Extract(source, args.InstallPath);

			if (!runUpgrade(args.InstallPath, args.InstallPath, "after-update")) {
				printUpdateFailed(args.Package.Signature);
				return;
			}
			
			_dispatch(
				string.Format(
					"Package updated from {0} to {1}",
					existingPackage.Signature,
					args.Package.Signature));
		}

		private void removePackage(string command, string path) {
			Directory.Delete(
				Path.Combine(path, command + "-files"), true);
			Directory
				.GetFiles(path)
				.Where(x => Path.GetFileNameWithoutExtension(x) == command)
				.ToList()
				.ForEach(x => File.Delete(x));
		}

		private void printUpdateFailed(string id) {
			_dispatch("");
			_dispatch(string.Format("error|Failed to update package {0}", id));
		}

		private void printUnexistingUpdate(string name, Package package) {
			_dispatch(string.Format("error|There is no installed {1} package {0} to update", name, package.Target));
		}

		private void installPackage(string source, Package package, string tempPath, string installPath, string activeProfile) {
			if (!runInstallVerify(tempPath, installPath)) {
				_dispatch("");
				_dispatch(string.Format("error|Failed to install package {0}", package.Signature));
				return;
			}

			foreach (var action in package.PreInstallActions)
				_dispatch(action);

			if (!installDependencies(package.Dependencies))
				return;
			new PackageExtractor().Extract(source, installPath);

			foreach (var action in package.PostInstallActions)
				_dispatch(action);

			_dispatch(string.Format("Installed {1} package {0} in profile {2}",
				package.Signature,
				package.Target,
				activeProfile));
		}

		private bool installDependencies(List<Package.Dependency> dependencies) {
			foreach (var dependency in dependencies) {
				if (!install(dependency.ID, dependency.Versions))
					return false;
			}
			return true;
		}

		private bool runInstallVerify(string tempPath, string installPath) {
			var installVerifier = getPackageScript(tempPath, "oi-package-install-verify");
			if (installVerifier == null)
				return true;
			
			return runProcess(installVerifier, "\"" + installPath + "\"");
 		}

 		private bool runUpgrade(string tempPath, string installPath, string argument) {
			var installVerifier = getPackageScript(tempPath, "oi-package-update");
			if (installVerifier == null)
				return true;
			
			return runProcess(installVerifier, argument + " \"" + installPath + "\"");
 		}

 		private string getPackageScript(string path, string script) {
 			var dir = Directory.GetDirectories(path).FirstOrDefault();
			if (dir == null)
				return null;

			return 
				Directory.GetFiles(dir)
				.FirstOrDefault(
					x => Path.GetFileNameWithoutExtension(x) == script);
 		}

 		private bool runProcess(string command, string arguments) {
 			var succeeded = true;
 			try {
				new Process()
					.Query(
						command,
						arguments,
						false,
						_token,
						(err, line) => {
								if (err) {
									_dispatch("error|" + line);
									succeeded = false;
								} else if (line.StartsWith("error|")) {
									_dispatch(line);
									succeeded = false;
								} else {
									_dispatch(line);
								}
							});
			} catch (Exception ex) {
				_dispatch("error|Failed running package verify. Make sure that environment supports script/executable type " + Path.GetExtension(command));
				_dispatch("warning|Exception:");
				_dispatch("warning|" + ex.ToString());
				succeeded = false;
			}
			return succeeded;
 		}

		private bool matchPackage(string path, string name) {
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
				return Path.GetFileNameWithoutExtension(path) == name;
			else
				return Path.GetFileNameWithoutExtension(path).ToLower() == name.ToLower();
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

		private void printConflictingPackage(string name, Package package, IEnumerable<string> matches) {
			var pkgInfo = "";
			var existingPackage = getPackage(matches.First());
			if (existingPackage != null)
				pkgInfo = string.Format(" ({0})", existingPackage.Signature);
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(
				"There is already an installed {2} package called {0}{1}",
				name,
				pkgInfo,
				package.Target);
			Console.ResetColor();
			if (existingPackage != null) {
				_dispatch("");
				_dispatch(existingPackage.ToVerboseString());
			}
			_dispatch("");
			_dispatch(
				"warning|To replace/update the installed package use the update command");
		}
	}
}