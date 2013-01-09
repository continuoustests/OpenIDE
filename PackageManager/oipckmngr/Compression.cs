using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace oipckmngr
{
	class Compression
	{
		public static void Decompress(string directory, string archiveFile) {
			using (var s = new GZipInputStream(File.OpenRead(archiveFile))) {
				using (var archive = TarArchive.CreateInputTarArchive(s, TarBuffer.DefaultBlockFactor)) {
					archive.ExtractContents(directory);
					archive.Close();
				}
				s.Close();
			}
		}

		public static void Compress(string directory, string filename) {
			try
			{
				var file = Path.GetTempFileName() + ".tar.gz";
				using (GZipOutputStream s = new GZipOutputStream(File.Create(file))) {
					using (var archive = TarArchive.CreateOutputTarArchive(s, TarBuffer.DefaultBlockFactor)) {
						addDirectory(archive, directory);
						archive.Close();
					}
					s.Finish();
					s.Close();
				}
				File.Move(file, filename + ".tar.gz");
			}
			catch(Exception ex)
			{
				Console.WriteLine("error|Exception during processing {0}", ex);
				throw;
			}
		}

		private static void addDirectory(TarArchive archive, string directory) {
			addDirectory(archive, directory, null);
		}

		private static void addDirectory(TarArchive archive, string directory, string name) {
			if (name != null) {
				var dir = TarEntry.CreateTarEntry(name);
				archive.WriteEntry(dir, true);
			} else {
				name = "";
			}

			foreach (var dir in Directory.GetDirectories(directory))
				addDirectory(archive, Path.Combine(directory, dir), name + Path.GetFileName(dir) + "/");

			var files = Directory.GetFiles(directory);
			foreach (var file in files)
				addFile(archive, file);
		}

		private static void addFile(TarArchive archive, string file) {
			var entry = TarEntry.CreateEntryFromFile(file);
			archive.WriteEntry(entry, true);
		}
	}
}