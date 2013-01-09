using System;
using System.IO;
using System.Linq;

namespace oipckmngr
{
	class Program
	{
		public static void Main(string[] args) {
			if (args.Length == 0) {
				Console.WriteLine("Valid parameters are create or install");
				return;
			}
			if (args[0] == "create" && args.Length == 3) {
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
				if (!package.IsValid()) {
					Console.WriteLine("Invalid Package: Package description file does not contain all required information");
					return;
				}
				var dirs = 
					Directory.GetDirectories(path)
						.Where(x => 
							Directory.GetDirectories(Path.Combine(path, x)).Length > 0 ||
							Directory.GetFiles(Path.Combine(path, x)).Length > 0);
				var nonExistentDirs = 
					dirs
						.Where(x => !package.Contents.Any(y => y.FileRoot.Equals(x)));
				if (nonExistentDirs.Count() > 0) {
					Console.WriteLine("The following content blocks does not have a matching directory containing items");
					nonExistentDirs.ToList()
						.ForEach(x => Console.WriteLine("\t" + x));
				}
			}
			//Compression.Compress(Environment.CurrentDirectory, "test");
			//Compression.Decompress("/home/ack/tmp/testdir", "/home/ack/src/OpenIDE/PackageManager/oipckmngr/test.tar.gz");
		}
	}
}
