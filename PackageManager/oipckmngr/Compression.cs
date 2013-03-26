using System;
using System.IO;
using System.Linq;
using Ionic.Zip;

namespace oipckmngr
{
	class Compression
	{
		public static void Decompress(string directory, string archiveFile) {
			using (var zip = ZipFile.Read(archiveFile)) {
				zip.ExtractAll(directory);
			}
		}

		public static void Compress(string directory, string name, string destination) {
			try
			{
				var filesDirectory = Path.Combine(directory, name + "-files");
				var file = destination + ".oipkg";
				using (var zip = new ZipFile(file)) {
					Console.WriteLine("Writing to " + file);
					Directory
						.GetFiles(directory, name + ".*")
						.ToList()
						.ForEach(x => addFile(zip, x, directory));
					addDirectory(zip, filesDirectory, directory);
					zip.Save();
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("error|Exception during processing {0}", ex);
				throw;
			}
		}

		private static void addDirectory(ZipFile archive, string directory, string rootPath) {
			addDirectory(archive, directory, rootPath, "");
		}

		private static void addDirectory(ZipFile archive, string directory, string rootPath, string name) {
			foreach (var dir in Directory.GetDirectories(directory))
				addDirectory(archive, Path.Combine(directory, dir), rootPath, name + Path.GetFileName(dir) + "/");

			var files = Directory.GetFiles(directory);
			foreach (var file in files)
				addFile(archive, file, rootPath);
		}

		private static void addFile(ZipFile archive, string file, string rootPath) {
			Console.WriteLine("Adding file " + file);
			var dir = Path.GetDirectoryName(file);
			var pathInArchive = dir.Substring(rootPath.Length, dir.Length - rootPath.Length);
			archive.AddFile(file, pathInArchive);
		}
	}
}