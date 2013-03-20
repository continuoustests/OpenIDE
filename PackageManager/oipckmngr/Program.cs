using System;
using System.IO;
using System.Linq;

namespace oipckmngr
{
	class Program
	{
		public static void Main(string[] args) {
			if (args.Length == 0) {
				Console.WriteLine("Valid parameters are init, package or install");
				return;
			}

			if (args[0] == "init") {
				var NL = Environment.NewLine;
				var package = 
					"{" + NL +
					"\t\"PS\": \"# is used to comment out optional fields\"," + NL +
					"\t\"id\": \"\"," + NL +
					"\t\"description\": \"\"," + NL +
					"\t\"#pre-install-actions\": []," + NL +
					"\t\"contents\":" + NL +
					"\t\t[" + NL +
					"\t\t\t{" + NL +
					"\t\t\t\t\"#pre-install-actions\": []," + NL +
					"\t\t\t\t\"target\": \"Valid options: script, rscript, language\"," + NL +
					"\t\t\t\t\"file-root\": \"Directory name of directory containing files\"," + NL +
					"\t\t\t\t\"#post-install-actions\": []" + NL +
					"\t\t\t}" + NL +
					"\t\t]," + NL +
					"\t\"#dependencies\": []," + NL +
					"\t\"#post-install-actions\": []" + NL +
					"}";
				var file = Path.Combine(Environment.CurrentDirectory, "package.json");
				File.WriteAllText(file, package);
			} else if (args[0] == "package" && args.Length == 3) {
				var name = args[1];
				var path = args[2];
				var json = Path.Combine(path, "package.json");
				if (!Directory.Exists(path)) {
					Console.WriteLine("Invalid Package: Package directory does not exist: " + path);
					return;
				}
				if (!File.Exists(json)) {
					Console.WriteLine("Invalid Package: Cannot find package description: " + json);
					return;
				}
				var package = Package.Read(File.ReadAllText(json));
				if (package == null) {
					Console.WriteLine("Invalid package format");
					return;
				}
				if (!package.IsValid()) {
					Console.WriteLine("Invalid Package: Package description file does not contain all required information");
					return;
				}
				var dirs = 
					Directory.GetDirectories(path)
						.Where(x => 
							Directory.GetDirectories(Path.Combine(path, x)).Length > 0 ||
							Directory.GetFiles(Path.Combine(path, x)).Length > 0);
				
				Compression.Compress(Environment.CurrentDirectory, name);
			} else {
				Console.WriteLine("Valid parameters are init, package or install");
			}
			//Compression.Compress(Environment.CurrentDirectory, "test");
			//Compression.Decompress("/home/ack/tmp/testdir", "/home/ack/src/OpenIDE/PackageManager/oipckmngr/test.tar.gz");
		}
	}
}
