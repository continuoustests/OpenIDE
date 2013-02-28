using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Config;
using OpenIDE.Core.Profiles;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
	class ProfileHandler : ICommandHandler
	{
		private ConfigurationHandler _configHandler;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"Creates, lists, activates and removes profiles. A profile contains scripts and configuration");

				usage.Add("list", "Lists available profiles");

				var activate = usage.Add("load", "Loads a profile");
				var name = activate.Add("NAME", "Profile name");
				name.Add("[--global]", "Activating global profile (setting profile globally does not override local profile)");
				name.Add("[-g]", "Short version of --global");

				var init = usage.Add("init", "Initializes a new profile");
				name = init.Add("NAME", "Profile name");
				name.Add("[--global]", "Creates application global profile");
				name.Add("[-g]", "Short version of --global");

				var clone = usage.Add("clone", "Clones active profile");
				name = clone.Add("NAME", "Name of new cloned profile");
				name.Add("[--global]", "Clones from active global profile");
				name.Add("[-g]", "Short version of --global");

				var del = usage.Add("rm", "Removes profile");
				name = del.Add("NAME", "Profile name");
				name.Add("[--global]", "Removes the globally defined profile");
				name.Add("[-g]", "Short version of --global");

				return usage;
			}
		}

		public string Command { get { return "profile"; } }

		public ProfileHandler(ConfigurationHandler configHandler) {
			_configHandler = configHandler;
		}

		public void Execute(string[] arguments) {
			var args = getArgs(arguments);
			if (args.Arguments.Length == 1 && args.Arguments[0] == "list")
				listProfiles();
			else if (args.Arguments.Length == 2 && args.Arguments[0] == "init")
				createProfile(args);
			else if (args.Arguments.Length == 2 && args.Arguments[0] == "rm")
				deleteProfile(args);
			else if (args.Arguments.Length == 2 && args.Arguments[0] == "load")
				loadProfile(args);
			else if (args.Arguments.Length == 2 && args.Arguments[0] == "clone")
				cloneProfile(args);
			else
				Console.WriteLine("Invalid arguments. For valid arguments run oi help profile");
		}

		private Args getArgs(string[] arguments) {
			var args = new Args();
			args.Arguments = arguments.Where(x => !x.StartsWith("-")).ToArray();
			args.IsGlobal = 
				arguments.Contains("-g") ||
				arguments.Contains("--global");
			args.Overwrite = 
				arguments.Contains("--overwrite");
			return args;
		}

		private void listProfiles() {
			var profileLocator = new ProfileLocator(Environment.CurrentDirectory);
			Console.WriteLine("Active global profile: " + profileLocator.GetActiveGlobalProfile());
			Console.WriteLine("Active local profile:  " + profileLocator.GetActiveLocalProfile());
			var globalProfiles = 
				profileLocator.GetProfilesForPath(
					profileLocator.GetGlobalProfilesRoot());
			globalProfiles.Insert(0, "default");
			Console.WriteLine();
			Console.WriteLine("Global profiles:");
			globalProfiles.ForEach(x => Console.WriteLine("\t" + x));

			if (Directory.Exists(profileLocator.GetLocalProfilesRoot())) {
				var localProfiles = 
				profileLocator.GetProfilesForPath(
					profileLocator.GetLocalProfilesRoot());
				localProfiles.Insert(0, "default");
				Console.WriteLine();
				Console.WriteLine("Local profiles:");
				localProfiles.ForEach(x => Console.WriteLine("\t" + x));
			}
			
			Console.WriteLine();
		}

		private void createProfile(Args args) {
			var profileLocator = new ProfileLocator(Environment.CurrentDirectory);
			// Make sure we have a config point
			if (!args.IsGlobal)
				_configHandler.Execute(new[]{"init"});
			var profileDir = getProfilePath(args, args.Arguments[1]);
			if (Directory.Exists(profileDir))
				return;
			Directory.CreateDirectory(profileDir);
			File.WriteAllText(Path.Combine(profileDir, "oi.config"), "");
			Console.WriteLine("Profile '{0}' created", args.Arguments[1]);
		}

		private void deleteProfile(Args args) {
			var profileDir = getProfilePath(args, args.Arguments[1]);
			if (!Directory.Exists(profileDir)) {
				Console.WriteLine("Could not find profile at " + profileDir);
				return;
			}
			try {
				removeDir(profileDir);
				Console.WriteLine("Profile '{0}' removed", args.Arguments[1]);
			} catch {
				Console.WriteLine("Failed to remove profile '{0}'", args.Arguments[1]);
			}
		}

		private void loadProfile(Args args) {
			var profileDir = getProfilePath(args, args.Arguments[1]);
			if (args.Arguments[1] == "default") {
				var activityFile = Path.Combine(profileDir, "active.profile");
				if (File.Exists(activityFile))
					File.Delete(activityFile);
			} else {
				if (!Directory.Exists(profileDir)) {
					Console.WriteLine("Profile {0} does not exist", args.Arguments[1]);
					return;
				}

				File.WriteAllText(
					Path.Combine(
						Path.GetDirectoryName(profileDir),
						"active.profile"), args.Arguments[1]);
			}
			Console.WriteLine("Profile '{0}' loaded", args.Arguments[1]);
		}

		private void cloneProfile(Args args) {
			var profileDir = getProfilePath(args, getActiveProfile(args));
			var cloneDir = getProfilePath(args, args.Arguments[1]);
			if (Directory.Exists(cloneDir)) {
				Console.WriteLine("Profile {0} already exists", args.Arguments[1]);
				return;
			}
			copyRecursive(
				profileDir + Path.DirectorySeparatorChar,
				cloneDir,
				(item, isDir) => {
					if (isDir)
						return !Path.GetFileName(item).StartsWith("profile.");
					return true;
				});
			Console.WriteLine("Profile '{0}' created", args.Arguments[1]);
		}

		private void copyRecursive(string source, string destination, Func<string, bool, bool> copyValidator) {
			var folder = Path.Combine(destination, Path.GetFileName(source));
			Directory.CreateDirectory(folder);
			Directory.GetDirectories(source)
				.Where(x => copyValidator(x, true)).ToList()
				.ForEach(x => copyRecursive(x, folder, copyValidator));
			Directory.GetFiles(source)
				.Where(x => copyValidator(x, false)).ToList()
				.ForEach(x => File.Copy(x, Path.Combine(folder, Path.GetFileName(x))));
		}

		private string getActiveProfile(Args args) {
			var profileLocator = new ProfileLocator(Environment.CurrentDirectory);
			if (args.IsGlobal)
				return profileLocator.GetActiveGlobalProfile();
			else
				return profileLocator.GetActiveLocalProfile();
		}

		private string getProfilePath(Args args, string name) {
			var profileLocator = new ProfileLocator(Environment.CurrentDirectory);
			if (args.IsGlobal)
				return profileLocator.GetGlobalProfilePath(name);
			else
				return profileLocator.GetLocalProfilePath(name);
		}

		private void removeDir(string dir) {
			Directory.GetDirectories(dir).ToList()
				.ForEach(x => removeDir(x));
			Directory.GetFiles(dir).ToList()
				.ForEach(x => File.Delete(x));
			Directory.Delete(dir);
		}

		class Args
		{
			public string[] Arguments { get; set; }
			public bool IsGlobal { get; set;}
			public bool Overwrite { get; set; }
		}
	}
}