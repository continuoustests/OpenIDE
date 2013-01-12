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

		public ProfileLocator(string rootPath) {
			_rootPath = rootPath;
		}

		public string GetGlobalProfilePath(string name) {
			if (name == "default")
				return GetGlobalProfilesRoot();
			else
				return Path.Combine(GetGlobalProfilesRoot(), "profile." + name);
		}

		public string GetLocalProfilePath(string name) {
			if (name == "default")
				return GetLocalProfilesRoot();
			else
				return Path.Combine(GetLocalProfilesRoot(), "profile." + name);
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
			return getActiveProfile(GetGlobalProfilesRoot());
		}

		public string GetActiveLocalProfile() {
			return getActiveProfile(GetLocalProfilesRoot());
		}

		public string GetGlobalProfilesRoot() {
			var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			return Path.Combine(appDir, ".OpenIDE");
		}

		public string GetLocalProfilesRoot() {
			var path = Environment.CurrentDirectory;
			return Path.GetDirectoryName(new Configuration(path, false).ConfigurationFile);
		}

		private string getActiveProfile(string rootPath) {
			var active = Path.Combine(rootPath, "active.profile");
			if (File.Exists(active)) {
				var name = File.ReadAllText(active)
					.Replace("\t", "")
					.Replace(Environment.NewLine, "")
					.Trim();
				if (Directory.Exists(Path.Combine(rootPath, "profile." + name)))
					return name;
			}
			return "default";
		}
	}
}