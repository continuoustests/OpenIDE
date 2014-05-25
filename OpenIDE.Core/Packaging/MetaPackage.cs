using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenIDE.Core.Packaging
{
    public class MetaPackage
    {
        public class Package
        {
            public string Id { get; private set; }
            public string Version { get; private set; }

            public Package(string id, string version)
            {
                Id = id;
                Version = version;
            }
        }

        private Func<string,string> _fileReader;

        public string File { get; private set; }
        public string[] OS { get; private set; }
        public string Id { get; private set; }
        public string Version { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public MetaPackage.Package[] Packages { get; private set; }

        public static MetaPackage Read(string file) {
            try {
                return new MetaPackage(System.IO.File.ReadAllText, file);
            } catch {
                return null;
            }
        }

        public MetaPackage(Func<string,string> fileReader, string file) {
            _fileReader = fileReader;
            File = file;
            read();
        }

        private void read() {
            var data = JObject.Parse(_fileReader(File));
            var os = new List<string>();
            data["os"].Children().ToList()
                .ForEach(x => os.Add(x.ToString()));
            OS = os.ToArray();
            Id = data["id"].ToString();
            Version = data["version"].ToString();
            Name = data["name"].ToString();
            Description = data["description"].ToString();

            var packages = new List<MetaPackage.Package>();
            if (data["packages"] != null) {
                    data["packages"].Children().ToList()
                        .ForEach(x => 
                            packages.Add(
                                new MetaPackage.Package(
                                    x["id"].ToString(),
                                    x["version"].ToString())));
            }
            Packages = packages.ToArray();
        }
    }
}
