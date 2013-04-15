using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.Packaging;
using OpenIDE.Core.Profiles;

namespace OpenIDE.Core.FileSystem
{
	public class SourceLocator
	{
		private string _token;
		private ProfileLocator _locator;

		public SourceLocator(string token) {
			_token = token;
			_locator = new ProfileLocator(_token);
		}

		public Source[] GetSources() {
			var sources = new List<Source>();
			addSources(sources, getLocalPath("default"));
			addSources(sources, getLocalPath(_locator.GetActiveLocalProfile()));
			addSources(sources, getGlobalPath("default"));
			addSources(sources, getGlobalPath(_locator.GetActiveGlobalProfile()));
			return sources.ToArray();
		}

		public string GetLocalDir() {
			return getLocalPath(_locator.GetActiveLocalProfile());
		}

		public string GetGlobalDir() {
			return getGlobalPath(_locator.GetActiveGlobalProfile());
		}

		private void addSources(List<Source> sources, string path) {
			var list = getSources(path);
			foreach (var source in list) {
				if (!sources.Any(x => x.Origin == source.Origin))
					sources.Add(source);
			}
		}

		private string getLocalPath(string profile) {
			return getPath(_locator.GetLocalProfilePath(profile));
		}

		private string getGlobalPath(string profile) {
			return getPath(_locator.GetGlobalProfilePath(profile));
		}

		private string getPath(string root) {
			return Path.Combine(root, "sources");
		}

		private Source[] getSources(string dir) {
			if (!Directory.Exists(dir))
				return new Source[] {};
			var sources = new List<Source>();
			var files = Directory.GetFiles(dir);
			foreach (var file in files) {
				var source = Source.Read(getContent(file), Path.GetFileNameWithoutExtension(file));
				if (source == null)
					continue;
				sources.Add(source);
			}
			return sources.ToArray();
		}

		private string getContent(string file) {
			try {
				return File.ReadAllText(file);
			} catch {
				return "";
			}
		}
	}
}