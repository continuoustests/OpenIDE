using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenIDE.Core.Packaging;

namespace oipckmngr
{
	class SourceHandler
	{
		public void Handle(string[] args) {
			if (args.Length == 4 && args[1] == "create") {
				var source = Path.GetFullPath(args[2]);
				var origin = args[3];
				var dir = Path.GetDirectoryName(source);
				if (File.Exists(source)) {
					writeError("Source {0} already exists. Try using update instead", source);
					return;
				}
				writeSource(source, origin, dir);
				Console.WriteLine("Created " + source);
				return;
			}
			if (args.Length == 3 && args[1] == "update") {
				var source = Path.GetFullPath(args[2]);
				if (!File.Exists(source)) {
					writeError("Source {0} does not exists.", source);
					return;
				}
				var src = OpenIDE.Core.Packaging.Source.Read(source);
				if (src == null) {
					writeError("{0} is not a valid source file.", source);
					return;
				}
				var origin = src.Origin;
				var basePath = src.Base; 
				writeSource(source, origin, basePath);
				return;
			}
		}

		private static void writeSource(string source, string origin, string basePath) {
			var json = new Source();
			json.Origin = origin;
			json.Base = basePath;
			foreach (var package in Directory.GetFiles(Path.GetDirectoryName(source), "*.oipkg")) {
				json.AddPackage(package);
			}

			File.WriteAllText(
				source,
				LowercaseJsonSerializer.SerializeObject(json));
		}

		private static void writeError(string error, params object[] args) {
			Console.WriteLine(error, args);
		}
	}

	class Source
	{
		public string Origin { get; set; }
		public string Base { get; set; }
		public List<PackageItem> Packages = new List<PackageItem>();

		public void AddPackage(string packageFile) {
			var package = readPackage(packageFile);
			if (package == null)
				return;
			Packages.Add(
				new PackageItem() {
						ID = package.ID,
						Version = package.Version,
						Package = Path.GetFileName(packageFile)
					});
		}

		private Package readPackage(string packageFile) {
			Package package = null;
			var tempPath = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
			Directory.CreateDirectory(tempPath);
			try {
				package = getInstallPackage(packageFile, tempPath);
			} catch (Exception ex) {
				Console.WriteLine("Failed to read package: " + Path.GetFileName(packageFile));
			} finally {
				Directory.Delete(tempPath, true);
			}
			return package;
		}

		private Package getInstallPackage(string source, string tempPath) {
			extractPackage(source, tempPath);
			var pkgFile =
				Path.Combine(
					Path.Combine(
						tempPath,
						Path.GetFileName(Directory.GetDirectories(tempPath)[0])),
					"package.json");
			return Package.Read(pkgFile);
		}

		private void extractPackage(string source, string path) {
			Compression.Decompress(path, source);
		}
	}

	class PackageItem
	{
		public string ID { get; set; }
		public string Version { get; set; }
		public string Package { get; set; }
	}

	class LowercaseJsonSerializer : DefaultContractResolver
	{
	    private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	    {
	        ContractResolver = new LowercaseContractResolver()
	    };

	    public static string SerializeObject(object o)
	    {
	        return JsonConvert.SerializeObject(o, Formatting.Indented, Settings);
	    }

	    public class LowercaseContractResolver : DefaultContractResolver
	    {
	        protected override string ResolvePropertyName(string propertyName)
	        {
	            return propertyName.ToLower();
	        }
	    }
	}
}
