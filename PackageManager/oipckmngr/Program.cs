using System;
using System.IO;
using System.Linq;

namespace oipckmngr
{
	class Program
	{
		public static void Main(string[] args) {
			if (args.Length == 0) {
				Console.WriteLine("Valid parameters are build or install");
				return;
			}

			if (args[0] == "build" && args.Length == 4) {
				var name = args[1];
				var path = args[2];
				var destination = args[3];
				path = Path.GetFullPath(path);
				destination = Path.GetFullPath(destination);
				var filesPath = Path.Combine(path, name + "-files");
				var json = Path.Combine(filesPath, "package.json");
				if (!Directory.Exists(path)) {
					Console.WriteLine("Invalid Package: Package directory does not exist: " + path);
					return;
				}
				if (!Directory.Exists(filesPath)) {
					Console.WriteLine("Invalid Package: Package files directory does not exist: " + filesPath);
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
				if (!Directory.Exists(destination)) {
					Console.WriteLine("Invalid Destination: Directory {0} does not exist", destination);
					return;
				}
				destination = Path.Combine(destination, package.ID);
				Compression.Compress(path, name, destination);
			} else {
				Console.WriteLine("Valid parameters are build or install");
			}
		}
	}
}
