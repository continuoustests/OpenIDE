using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OpenIDE.Core.Logging;

namespace OpenIDE.Core.Packaging
{
	public class Source
	{
		private static Func<string,string> _fileReader = (f) => File.ReadAllText(f);

		public static void ReadFilesUsing(Func<string,string> func) {
			_fileReader = func;
		}

		public static Source Read(string file) {
			try {
				var json = _fileReader(file);
				var data = JObject.Parse(json);
				var source = new Source(data["origin"].ToString(), file);
				data["packages"].Children().ToList()
						.ForEach(x => 
							source.AddPackage(
								new SourcePackage(
									x["id"].ToString(),
									x["version"].ToString(),
									x["name"].ToString(),
									x["description"].ToString(),
									source.Base + x["package"].ToString())));
				return source;
			} catch {
				Logger.Write("Failed to read source package " + file);
			}
			return null;
		}

		public class SourcePackage
		{
			public string ID { get; private set; }
			public string Version { get; private set; }
			public string Name { get; private set; }
			public string Description{ get; private set; }
			public string Package { get; private set; }

			public SourcePackage(string id, string version, string name, string description, string package) {
				ID = id;
				Version = version;
				Name = name;
				Description = description;
				Package = package;
			}

			public override string ToString() {
				return string.Format("{0} ({1} {2})", Name, ID, Version);
			}
		}

		private List<SourcePackage> _packages;

		public string Path { get; private set; }
		public string Name { get; private set; }
		public string Origin { get; private set; }
		public string Base { get; private set; }
		public List<SourcePackage> Packages { get { return _packages; } }

 		public Source(string origin, string file) {
 			Path = file;
			Name = System.IO.Path.GetFileNameWithoutExtension(file);
			Origin = origin;
			Base = getBaseFromOrigin();
			_packages = new List<SourcePackage>();
		}

		public void AddPackage(SourcePackage package) {
			_packages.Add(package);
		}

		private string getBaseFromOrigin() {
			var end = Origin.Length;
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
				end = Origin.LastIndexOf("/") + 1;
			else
				end = Origin.LastIndexOf("\\") + 1;
			return Origin.Substring(0, end);
		}
	}
}