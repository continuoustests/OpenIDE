using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenIDE.Core.Packaging
{
	public class Source
	{
		public static Source Read(string json, string name) {
			try {
				var data = JObject.Parse(json);
				var source = new Source(data["origin"].ToString(), name);
				var baseLocation = data["base"].ToString();
				data["packages"].Children().ToList()
						.ForEach(x => 
							source.AddPackage(
								new SourcePackage(
									x["id"].ToString(),
									x["version"].ToString(),
									baseLocation + x["package"].ToString())));
				return source;
			} catch {
			}
			return null;
		}

		public class SourcePackage
		{
			public string ID { get; private set; }
			public string Version { get; private set; }
			public string Package { get; private set; }

			public SourcePackage(string id, string version, string package) {
				ID = id;
				Version = version;
				Package = package;
			}
		}

		private List<SourcePackage> _packages;

		public string Name { get; private set; }
		public string Origin { get; private set; }
		public List<SourcePackage> Packages { get { return _packages; } }

 		public Source(string origin, string name) {
 			Name = name;
			Origin = origin;
			_packages = new List<SourcePackage>();
		}

		public void AddPackage(SourcePackage package) {
			_packages.Add(package);
		}
	}
}