using System;
using System.IO;
using System.Linq;
using OpenIDE.Core.Packaging;

namespace oipckmngr
{
	class Program
	{
		public static void Main(string[] args) {
			if (args.Length == 5 && args[0] == "build") {
				var name = args[1];
				var command = args[2];
				var path = args[3];
				var destination = args[4];
				path = Path.GetFullPath(path);
				destination = Path.GetFullPath(destination);
				var filesPath = Path.Combine(path, command + "-files");
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
				var package = Package.Read(json);
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
				destination = Path.Combine(destination, name);
				Compression.Compress(path, command, destination);
				Console.WriteLine("Package created: " + destination + ".oipkg");
			} else if (args.Length == 3 && args[0] == "extract") {
				var file = Path.GetFullPath(args[1]);
				var path = Path.GetFullPath(args[2]);
				Compression.Decompress(path, file);
			} else if (args.Length > 2 && args[0] == "source") {
				new SourceHandler().Handle(args);
			} else {
				Console.WriteLine("Usage:");
				Console.WriteLine("\tbuild ID COMMAND SOURCE-DIR DESTINATION-DIR");
				Console.WriteLine("\textract FILE DESTINATION-DIR");
				Console.WriteLine("\tsource create SOURCE-FILE ORIGIN");
				Console.WriteLine("\tsource update SOURCE-FILE");
			}
		}
	}
}
