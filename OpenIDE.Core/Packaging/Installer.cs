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
			public string Match { get; set; }
		}

		private string _token;
		private Action<string> _dispatch;
		private bool _useGlobal = false;
		private PluginLocator _locator;
		private PackageFetcher _packageFetcher;

		public Installer(string token, string[] sourcePrioritization, Action<string> dispatch, PluginLocator locator) {
			_token = token;
			_dispatch = dispatch;
			_locator = locator;
			_packageFetcher = new PackageFetcher(_token, sourcePrioritization, _dispatch);
		}

		public void UseGlobalProfiles(bool useGlobal) {
			_useGlobal = useGlobal;
		}

		public void Install(string packageToken) {
			install(packageToken, null);
		}

		public bool Install(string packageToken, string[] acceptedVersions) {
			return install(packageToken, acceptedVersions);
		}

		private bool install(string packageToken, string[] acceptedVersions) {
			var installType = "package";
			if (acceptedVersions == null) // if not it's a dependency
				_dispatch("installing " + installType + " " + packageToken);
			var source = _packageFetcher.Fetch(packageToken);
			if (source == null || !File.Exists(source.Package)) {
				_dispatch("error|could not find package " + packageToken);
				return false;
			}

			if (isMetaPackage(source.Package)) {
				installMetaPackage(source);
				return true;
			}

			string activeProfile = null;
			var actionSucceeded = prepareForAction(
				source.Package,
				(package) => {
					var profiles = new ProfileLocator(_token);
					if (_useGlobal)
						activeProfile = profiles.GetActiveGlobalProfile();
					else
						activeProfile = profiles.GetActiveLocalProfile();
					var installPath = getInstallPath(package, profiles, activeProfile);
					if (installPath == null)
						return null;
					Logger.Write("Installing the package in " + installPath);
					return installPath;
				},
				(args) => {
						if (args.Match != null) {
							Logger.Write("found matching package " + args.Match);
							var command = Path.GetFileNameWithoutExtension(args.Match);
							var package = getPackage(command);
							if (package != null) {
								Logger.Write("loaded package " + package.ID);
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
								_dispatch(string.Format("skipping {0} ({1}) already installed", package.ID, package.Version));
							} else {
								_dispatch(string.Format("error|the package with the command {0} conflicts with the package you are trying to install", command));
							}
						} else if (acceptedVersions != null && !acceptedVersions.Any(x => x == args.Package.Version)) {
							var versions = "";
							foreach (var version in acceptedVersions) {
								versions += version + ",";
							}
							versions = versions.Trim(new[] {','});
							_dispatch(string.Format("error|dependency {0} of version {1} is not a valid. Accepted versions are {2}", args.Package.ID, args.Package.Version, versions));
							return false;
						} else
							installPackage(source.Package, args.Package, args.TempPath, args.InstallPath, activeProfile);
						return true;
					});
			if (source.IsTemporaryPackage)
				File.Delete(source.Package);
			return actionSucceeded;
		}

		public void Update(string packageToken) {
			var source = _packageFetcher.Fetch(packageToken);
			if (source == null) {
				_dispatch("error|cannot find the package you are trying to update to");
				return;
			}
			if (!File.Exists(source.Package)) {
				_dispatch("error|cannot find the package you are trying to update to");
				return;
			}
			Package packageToUpdate = null;
			prepareForAction(
				source.Package,
				(package) => {
					packageToUpdate = getPackages(true)
						.FirstOrDefault(x => x.ID == package.ID);
					if (packageToUpdate == null) {
						_dispatch("error|the package you are trying to update is not installed");
						return null;
					}
					var profiles = new ProfileLocator(_token);
					_useGlobal = profiles.IsGlobal(packageToUpdate.File);
					return Path.GetDirectoryName(Path.GetDirectoryName(packageToUpdate.File));
				},
				(args) => {
						if (args.Match == null)
							printUnexistingUpdate(args.Package.ID, args.Package);
						else
							update(source.Package, packageToUpdate, args);
						return true;
					});
			if (source.IsTemporaryPackage)
				File.Delete(source.Package);
		}

		public void Remove(string source) {
			var package = getPackage(source);
			if (package == null) {
				_dispatch(string.Format("error|there is no package {0} to remove", source));
				return;
			}
			var profiles = new ProfileLocator(_token);
			var isGlobal = profiles.IsGlobal(package.File);
			removePackage(package, Path.GetDirectoryName(Path.GetDirectoryName(package.File)), isGlobal);
			_dispatch(string.Format("Removed package {0}", package.Signature));
		}

		private bool isMetaPackage(string source) {
			return source.EndsWith(".meta");
		}

		private void installMetaPackage(PackageFetcher.FetchedPackage source) {
			var package = MetaPackage.Read(source.Package);
			if (package == null) {
				_dispatch("error|Invalid meta package");
				return;
			}
			foreach (var pckg in package.Packages) {
				install(pckg.Id, new[] {pckg.Version});
			}
			if (source.IsTemporaryPackage)
				File.Delete(source.Package);
		}
		
		private bool prepareForAction(string source, Func<Package,string> destinatinoPathLocator, Func<ActionParameters,bool> actionHandler) {
			var actionSucceeded = false;
			var tempPath = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
			Directory.CreateDirectory(tempPath);
			try {
				var package = getInstallPackage(source, tempPath);
				if (package != null) {
					var installPath = destinatinoPathLocator(package);
					if (installPath == null)
						return false;
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

		private IEnumerable<Package> getPackages(bool all) {
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
								Logger.Write("adding package {0} ({1})", package.ID, package.Version);
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
			if (package.Target.StartsWith("language-")) {
				var path = getLanguageInstallPath(package, !_useGlobal);
				if (path == null) {
					_dispatch("error|could not find language to install language dependent package in");
					return null;
				}
				if (_useGlobal && !profiles.IsGlobal(path)) {
					_dispatch("error|cannot install language dependent package globally as language is installed locally.");
					return null;
				}
				return path;
			}
			if (_useGlobal)
				installPath = profiles.GetGlobalProfilePath(activeProfile);
			else
				installPath = profiles.GetLocalProfilePath(activeProfile);
			if (installPath == null) {
				_dispatch("error|the current location does not have an initialized config point");
				return null;
			}
			return Path.Combine(installPath, package.Target + "s");
		}

		private string  getLanguageInstallPath(Package package) {
			return getLanguageInstallPath(package, false);
		}

		private string  getLanguageInstallPath(Package package, bool forcelocal) {
			var language = _locator
				.Locate()
				.FirstOrDefault(x => x.GetLanguage() == package.Language);
			if (language == null) {
				Logger.Write("Failed to locate language " + package.Language);
				return null;
			}
			var basepath = Path.GetDirectoryName(language.FullPath);
			if (forcelocal) {
				var profiles = new ProfileLocator(_token);
				basepath = Path.Combine(profiles.GetLocalProfilePath(profiles.GetActiveLocalProfile()), "languages");
			}
			return
				Path.Combine(
					Path.Combine(
						basepath,
						language.GetLanguage() + "-files"),
					package.Target.Replace("language-", "") + "s");
		}

		private void update(string source, Package existingPackage, ActionParameters args) {
			if (existingPackage == null) {
				_dispatch("error|the requested package is not installed. Try install instead.");
				return;
			}
			if (!runInstallVerify(args.TempPath, args.InstallPath)) {
				printUpdateFailed(args.Package.Signature);
				return;
			}
			if (!runUpgrade(args.TempPath, args.InstallPath, "before-update")) {
				printUpdateFailed(args.Package.Signature);
				return;
			}

			var backupLocation = backupScripts(args.Package.Command, args.InstallPath);
			removePackage(args.Package, args.InstallPath, _useGlobal);
			new PackageExtractor().Extract(source, args.InstallPath);
			restoreScripts(args.Package.Command, args.InstallPath, backupLocation);

			if (!runUpgrade(args.InstallPath, args.InstallPath, "after-update")) {
				printUpdateFailed(args.Package.Signature);
				return;
			}
			
			_dispatch(
				string.Format(
					"package updated from {0} to {1}",
					existingPackage.Signature,
					args.Package.Signature));
		}

		private string backupScripts(string command, string source) {
			var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var sourcePath = Path.Combine(source, command + "-files");
			copyPluginDirectories(sourcePath, path);
			return path;
		}

		private void restoreScripts(string command, string source, string backupLocation) {
			var sourcePath = Path.Combine(source, command + "-files");
			copyPluginDirectories(backupLocation, sourcePath);
		}

		private void copyPluginDirectories(string source, string destination) {
			backupDirectoryTo(Path.Combine(source, "rscripts"), destination);
			backupDirectoryTo(Path.Combine(source, "scripts"), destination);
			backupDirectoryTo(Path.Combine(source, "snippets"), destination);
			backupDirectoryTo(Path.Combine(source, "preserved-data"), destination);
			backupDirectoryTo(Path.Combine(source, "state"), destination);
		}

		private void backupDirectoryTo(string source, string destinationRoot) {
			if (!Directory.Exists(source))
				return;
			var name = Path.GetFileName(source);
			var destination = Path.Combine(destinationRoot, name);
			if (!Directory.Exists(destination))
				Directory.CreateDirectory(destination);
			Action<string,string> copyAll = (src, dest) => {};
			copyAll = (src, dest) => {
				foreach (var dir in Directory.GetDirectories(src)) {
					var destDir = Path.Combine(dest, Path.GetFileName(source));
					if (!Directory.Exists(destDir))
						Directory.CreateDirectory(destDir);
					copyAll(dir, destDir);
				}
				foreach (var file in Directory.GetFiles(source))
					File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
			};
			copyAll(source, destination);
		}

		private void removePackage(Package package, string path, bool isGlobal) {
			foreach (var action in package.PreUninstallActions) {
				if (isGlobal && action.Global != null)
					_dispatch(action.Global);
				else
					_dispatch(action.Action);
			}
			Directory.Delete(
				Path.Combine(path, package.Command + "-files"), true);
			Directory
				.GetFiles(path)
				.Where(x => Path.GetFileNameWithoutExtension(x) == package.Command)
				.ToList()
				.ForEach(x => File.Delete(x));
			foreach (var action in package.PostUninstallActions) {
				if (isGlobal && action.Global != null)
					_dispatch(action.Global);
				else
					_dispatch(action.Action);
			}
		}

		private void printUpdateFailed(string id) {
			_dispatch("");
			_dispatch(string.Format("error|failed to update package {0}", id));
		}

		private void printUnexistingUpdate(string name, Package package) {
			_dispatch(string.Format("error|there is no installed {1} package {0} to update", name, package.Target));
		}

		private void installPackage(string source, Package package, string tempPath, string installPath, string activeProfile) {
			if (!runInstallVerify(tempPath, installPath)) {
				_dispatch("");
				_dispatch(string.Format("error|failed to install package {0}", package.Signature));
				return;
			}

			foreach (var action in package.PreInstallActions) {
				if (_useGlobal && action.Global != null)
					_dispatch(action.Global);
				else
					_dispatch(action.Action);
			}

			if (!installDependencies(package.Dependencies))
				return;
			new PackageExtractor().Extract(source, installPath);

			foreach (var action in package.PostInstallActions) {
				if (_useGlobal && action.Global != null)
					_dispatch(action.Global);
				else
					_dispatch(action.Action);
			}

			_dispatch(string.Format("installed {1} package {0} in profile {2}",
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
				_dispatch("error|failed running package verify. Make sure that environment supports script/executable type " + Path.GetExtension(command));
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
			_dispatch(
				string.Format(
					"error|there is already an installed {2} package called {0}{1}",
					name,
					pkgInfo,
					package.Target));
			if (existingPackage != null) {
				_dispatch("");
				_dispatch(existingPackage.ToVerboseString());
			}
			_dispatch("");
			_dispatch(
				"warning|to replace/update the installed package use the update command");
		}
	}
}
