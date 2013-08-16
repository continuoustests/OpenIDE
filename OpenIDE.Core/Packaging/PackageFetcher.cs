using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using OpenIDE.Core.Logging;
using OpenIDE.Core.FileSystem;
using CoreExtensions;

namespace OpenIDE.Core.Packaging
{
	public class PackageFetcher
	{
		public class FetchedPackage
		{
			public bool IsTemporaryPackage { get; private set; }
			public string Package { get; private set; }

			public FetchedPackage(string package, bool isTemporaryPackage) {
				IsTemporaryPackage = isTemporaryPackage;
				Package = package;
			}
		}

		private string _token;
		private Action<string> _dispatch;

		public PackageFetcher(string token, Action<string> dispatch) {
			_token = token;
			_dispatch = dispatch;
		}

		public FetchedPackage Fetch(string source) {
			try {
				if (File.Exists(Path.GetFullPath(source)))
					return new FetchedPackage(Path.GetFullPath(source), false);
				var package = new SourceLocator(_token).GetPackage(source);
				if (package != null) {
					source = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString() + ".oipkg");
					if (download(package.Package, source))
						return new FetchedPackage(source, true);
				}
			} catch (Exception ex) {
				Logger.Write(ex);
			}
			return null;
		}

		private bool download(string source, string destination) {
			return new FileFetcher(_dispatch).Download(source, destination);
		}
	}

	public class FileFetcher
	{
		private Action<string> _dispatch;

		public FileFetcher(Action<string> dispatch) {
			_dispatch = dispatch;
		}

		public bool Download(string source, string destination) {
			try {
				_dispatch(string.Format("Downloading {0} ...", source));
				if (File.Exists(source)) {
					File.Copy(source, destination, true);
					return true;
				}
				if (source.StartsWith("http://") || source.StartsWith("https://")) {
					var client = new WebClient();
					client.DownloadFile(source, destination);
					return true;
				}
			} catch {
			}
			return false;
		}
	}

	public class PackageExtractor
	{
		public void Extract(string source, string path) {
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