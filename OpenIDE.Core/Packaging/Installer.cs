using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.Profiles;
using CoreExtensions;

namespace OpenIDE.Core.Packaging
{
	public class Installer
	{
		class ActionParameters
		{
			public Package Package { get; set; }
			public string Name { get; set; }
			public string TempPath { get; set; }
			public string InstallPath { get; set; }
			public string ProfileName { get; set; }
			public IEnumerable<string> Matches { get; set; }
		}

		private string _token;
		private Action<string> _dispatch;
		private Action<string,string> _unpack;
		private bool _useGlobal = false;

		public Installer(string token, Action<string> dispatch, Action<string,string> unpack) {
			_token = token;
			_dispatch = dispatch;
			_unpack = unpack;
		}

		public void UseGlobalProfiles(bool useGlobal) {
			_useGlobal = useGlobal;
		}

		public void Install(string source) {
			prepareForAction(
				source,
				(args) => {
						if (args.Matches.Count() > 0)
							printConflictingPackage(args.Name, args.Package, args.Matches);
						else
							installPackage(source, args.Package, args.TempPath, args.InstallPath, args.ProfileName);
					});
		}

		public void Update(string source) {
			prepareForAction(
				source,
				(args) => {
						if (args.Matches.Count() == 0)
							printUnexistingUpdate(args.Name, args.Package);
						else
							update(source, args);
					});
		}

		public void Remove(string source) {
			var name = Path.GetFileNameWithoutExtension(source);
			var dir = Path.GetDirectoryName(source);
			var package = getPackage(source);
			if (package == null) {
				_dispatch(string.Format("error|There is no package {0} to remove", name));
				return;
			}
			removePackage(name, dir);
			_dispatch(string.Format("Removed package {0}", package.Signature));
		}
		
		private void prepareForAction(string source, Action<ActionParameters> actionHandler) {
			var profiles = new ProfileLocator(_token);
			string activeProfile;
			if (_useGlobal)
				activeProfile = profiles.GetActiveGlobalProfile();
			else
				activeProfile = profiles.GetActiveLocalProfile();

			string installPath;
			if (_useGlobal)
				installPath = profiles.GetGlobalProfilePath(activeProfile);
			else
				installPath = profiles.GetLocalProfilePath(activeProfile);
			var tempPath = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
			Directory.CreateDirectory(tempPath);
			try {
				var package = getInstallPackage(source, tempPath);
				if (package != null) {
					var name = package.ID;
					installPath = Path.Combine(installPath, package.Target + "s");
					if (!Directory.Exists(installPath))
						Directory.CreateDirectory(installPath);
					var matches = 
						Directory.GetFiles(installPath)
							.Where(x => matchPackage(x, name));
					actionHandler(
						new ActionParameters() {
							Package = package,
							Name = name,
							TempPath = tempPath,
							InstallPath = installPath,
							ProfileName = activeProfile,
							Matches = matches
						});
				}
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			} finally {
				Directory.Delete(tempPath, true);
			}
		}

		private void update(string source, ActionParameters args) {
			var existingPackage = getPackage(args.Matches.First());
			if (!runInstallVerify(args.TempPath, args.InstallPath)) {
				printUpdateFailed(args.Package.Signature);
				return;
			}
			if (!runUpgrade(args.TempPath, args.InstallPath, "before-update")) {
				printUpdateFailed(args.Package.Signature);
				return;
			}

			removePackage(args.Name, args.InstallPath);
			_unpack(source, args.InstallPath);

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

		private void removePackage(string name, string path) {
			Directory.Delete(
				Path.Combine(path, name + "-files"), true);
			Directory
				.GetFiles(path)
				.Where(x => Path.GetFileNameWithoutExtension(x) == name)
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

			_unpack(source, installPath);

			foreach (var action in package.PostInstallActions)
				_dispatch(action);

			_dispatch(string.Format("Installed {1} package {0} in profile {2}",
				package.Signature,
				package.Target,
				activeProfile));
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
			_unpack(source, tempPath);
			var pkgFile =
				Path.Combine(
					Path.Combine(
						tempPath,
						Path.GetFileName(Directory.GetDirectories(tempPath)[0])),
					"package.json");
			return Package.Read(File.ReadAllText(pkgFile));
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