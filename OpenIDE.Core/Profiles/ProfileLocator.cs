using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Config;

namespace OpenIDE.Core.Profiles
{
	public class ProfileLocator
	{
		private string _rootPath;
		private string _appRoot;

		public string AppRootPath { get { return _appRoot; } }
		public static string ActiveGlobalProfile { get; set; }
		public static string ActiveLocalProfile { get; set; }

		public ProfileLocator(string tokenPath) {
			_rootPath = tokenPath;
			_appRoot = getAppRoot();
		}


		private string getAppRoot() {
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (!File.Exists(Path.Combine(path, "oi.exe")))
				path = Path.GetDirectoryName(path);
			return path;
		}

		public string GetGlobalProfilePath(string name) {
			if (name == "default")
				return GetGlobalProfilesRoot();
			else
				return Path.Combine(GetGlobalProfilesRoot(), "profile." + name);
		}

		public string GetLocalProfilePath(string name) {
			var root = GetLocalProfilesRoot();
			if (root == null)
				return null;
			if (name == "default")
				return root;
			else
				return Path.Combine(root, "profile." + name);
		}

		public List<string> GetProfilesForPath(string path) {
			var profiles = new List<string>();
			Directory.GetDirectories(path)
				.Select(x => Path.GetFileName(x))
				.Where(x => x.StartsWith("profile.")).ToList()
				.ForEach(x => profiles.Add(x.Substring(8, x.Length -8)));
			return profiles;
		}

		public string  GetActiveGlobalProfile() {
			var rootPath = GetGlobalProfilesRoot();
			if (ActiveGlobalProfile != null) {
				if (profileExists(ActiveGlobalProfile, rootPath))
					return ActiveGlobalProfile;
			}
			return getActiveProfile(rootPath);
		}

		public string GetActiveLocalProfile() {
			var rootPath = GetLocalProfilesRoot();
			if (ActiveLocalProfile != null) {
				if (profileExists(ActiveLocalProfile, rootPath))
					return ActiveLocalProfile;
			}
			return getActiveProfile(GetLocalProfilesRoot());
		}

		public string GetGlobalProfilesRoot() {
			//var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var appDir = _appRoot;
			return Path.Combine(appDir, ".OpenIDE");
		}

		public string GetLocalProfilesRoot() {
			var path = _rootPath;
			return getConfigPoint(path);
		}

		public string GetGlobalProfileToken() {
			//var appDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ".OpenIDE");
			var appDir = Path.Combine(_appRoot, ".OpenIDE");
			return Path.Combine(appDir, "active.profile");
		}

		public string GetLocalProfileToken() {
			if (_rootPath == null)
				return null;
			var appDir = Path.Combine(_rootPath, ".OpenIDE");
			return Path.Combine(appDir, "active.profile");
		}

		public IEnumerable<string> GetFilesCurrentProfiles() {
			return GetFiles(null);
		}

		public IEnumerable<string> GetPathsCurrentProfiles() {
			// PS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			// Always populate list in the following order. Others depend on it!!!!!:
			// Local active, local default, global active, global default
			var paths = new List<string>();
			// Get from local profile
			var localProfile = GetActiveLocalProfile();
			var path = GetLocalProfilePath(localProfile);
			if (Directory.Exists(path))
				paths.Add(path);

			// Get from local default profile
			if (localProfile != "default") {
				path = GetLocalProfilePath("default");
				if (Directory.Exists(path))
					paths.Add(path);
			}

			// Get from global profile
			var globalProfile = GetActiveGlobalProfile();
			path = GetGlobalProfilePath(globalProfile);
			if (Directory.Exists(path))
				paths.Add(path);

			// Get from global default profile
			if (globalProfile != "default") {
				path = GetGlobalProfilePath("default");
				if (Directory.Exists(path))
					paths.Add(path);
			}
			return paths;
		}

		public IEnumerable<string> GetFilesCurrentProfiles(string pattern) {
			var files = new List<string>();
			var paths = GetPathsCurrentProfiles();
			foreach (var path in paths) {
				getFilesRecursive(path, files, pattern);
			}
			return files;
		}

		public IEnumerable<string> GetFiles(string path) {
			return GetFiles(path, null);
		}

		public IEnumerable<string> GetFiles(string path, string pattern) {
			var files = new List<string>();
			getFilesRecursive(path, files, pattern);
			return files;
		}

		private void getFilesRecursive(string path, List<string> files, string pattern) {
			Directory.GetDirectories(path).ToList()
				.ForEach(x => {
						if (Path.GetFileName(x).StartsWith("profile."))
							return;
						getFilesRecursive(x, files, pattern);
					});

			if (pattern != null)
				Directory.GetFiles(path, pattern).ToList().ForEach(x => files.Add(x));
			else
				Directory.GetFiles(path).ToList().ForEach(x => files.Add(x));
		}

		private string getActiveProfile(string rootPath) {
			if (rootPath == null)
				return null;
			var active = Path.Combine(rootPath, "active.profile");
			if (File.Exists(active)) {
				var name = File.ReadAllText(active)
					.Replace("\t", "")
					.Replace(Environment.NewLine, "")
					.Trim();
				if (profileExists(name, rootPath))
					return name;
			}
			return "default";
		}

		private bool profileExists(string name, string rootPath) {
			return Directory.Exists(Path.Combine(rootPath, "profile." + name));
		}

		private static string getConfigPoint(string path) {
			if (path == null)
				return null;
			var dir = Path.Combine(path, ".OpenIDE");
			if (!Directory.Exists(dir))
			{
				try {
					return getConfigPoint(Path.GetDirectoryName(path));
				} catch {
					return null;
				}
			}
			return dir;
		}
	}
}