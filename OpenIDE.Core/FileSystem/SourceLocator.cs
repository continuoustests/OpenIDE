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
		private string[] _sourcePrioritization;
		private ProfileLocator _locator;

		public SourceLocator(string token, string[] sourcePrioritization) {
			_token = token;
			_sourcePrioritization = sourcePrioritization;
			_locator = new ProfileLocator(_token);
		}

		public Source[] GetSources() {
			var sources = new List<Source>();
			addSources(sources, getLocalPath("default"));
			addSources(sources, getLocalPath(_locator.GetActiveLocalProfile()));
			addSources(sources, getGlobalPath("default"));
			addSources(sources, getGlobalPath(_locator.GetActiveGlobalProfile()));
			return prioritizeList(sources);
		}

		public Source[] GetLocalSources() {
			var sources = new List<Source>();
			addSources(sources, getLocalPath("default"));
			addSources(sources, getLocalPath(_locator.GetActiveLocalProfile()));
			return prioritizeList(sources);
		}

		public Source[] GetGlobalSources() {
			var sources = new List<Source>();
			addSources(sources, getGlobalPath("default"));
			addSources(sources, getGlobalPath(_locator.GetActiveGlobalProfile()));
			return prioritizeList(sources);
		}

		public Source[] GetSourcesFrom(string dir) {
			var sources = new List<Source>();
			addSources(sources, dir);
			return prioritizeList(sources);
		}

		public string GetLocalDir() {
			var path = _locator.GetActiveLocalProfile();
			if (path == null)
				return null;
			return getLocalPath(path);
		}

		public string GetGlobalDir() {
			return getGlobalPath(_locator.GetActiveGlobalProfile());
		}

		public Source.SourcePackage GetPackage(string name) {
			var sources = GetSources();
			foreach (var source in sources) {
				foreach (var package in source.Packages) {
					if (package.ID == name)
						return package;
				}
			}
			return null;
		}

		private void addSources(List<Source> sources, string path) {
			if (path == null)
				return;
			var list = getSources(path);
			foreach (var source in list) {
				if (!sources.Any(x => x.Origin == source.Origin))
					sources.Add(source);
			}
		}

		private string getLocalPath(string profile) {
			var path = _locator.GetLocalProfilePath(profile);
			if (path == null)
				return null;
			return getPath(path);
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
				var source = Source.Read(file);
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

		private Source[] prioritizeList(IEnumerable<Source> packageSources) {
			var list = _sourcePrioritization;
			var sources = new List<Source>(packageSources);
            for (int i = list.Length - 1; i >= 0; i--) {
                var itm = sources.FirstOrDefault(x => x.Name == list[i]);
                if (itm == null)
                	continue;
                sources.Remove(itm);
                sources.Insert(0, itm);
            }
            return sources.ToArray();;
		}
	}
}