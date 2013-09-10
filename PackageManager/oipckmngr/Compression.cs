using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using SharpCompress.Common;
using SharpCompress.Archive;
using SharpCompress.Writer;
using SharpCompress.Writer.GZip;

namespace oipckmngr
{
	class Compression
	{
		public static void Decompress(string directory, string archiveFile) {
			if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix) {
				var tarfile = 
					Path.Combine(
						directory,
						Path.GetFileName(archiveFile) + ".tar.gz");
				if (File.Exists(tarfile))
					File.Delete(tarfile);
				File.Copy(archiveFile, tarfile);
				run(directory, "gunzip", string.Format("-q \"{0}\"", tarfile));
				tarfile = 
					Path.Combine(
						Path.GetDirectoryName(tarfile),
						Path.GetFileNameWithoutExtension(tarfile));
				run(directory, "tar", string.Format("-xvf \"{0}\"", tarfile));
				File.Delete(tarfile);
	            return;
			}
			using (Stream gz = File.Open(archiveFile, FileMode.Open)) {
	            using (var gzArchive = ArchiveFactory.Open(gz)) {
	                var tarEntry = gzArchive.Entries.First();
	                using (var tar = new MemoryStream()) {
	                	tarEntry.WriteTo(tar);
	                	using (var tarArchive = ArchiveFactory.Open(tar)) {
	                		foreach (var entry in tarArchive.Entries.Where(entry => !entry.IsDirectory)) {
		                        entry.WriteToDirectory(
		                        	directory,
		                            ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
		                    }
	                	}
	                }
	            }
	        }
		}

		private static void run(string directory, string command, string arguments) {
			var proc = new Process();
            proc.StartInfo.FileName   = command;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.WorkingDirectory = directory;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.WaitForExit();
		}

		private static void createDirectories(string path) {
			if (Directory.Exists(path))
				return;
			var subPath = Path.GetDirectoryName(path);
			if (subPath != null)
				createDirectories(subPath);
			Directory.CreateDirectory(path);
		}

		public static void Compress(string directory, string command, string destination) {
			try
			{
				var filesDirectory = Path.Combine(directory, command + "-files");
				var file = destination + ".oipkg";
				using (Stream tar = new MemoryStream()) {
					using (IWriter zip = WriterFactory.Open(tar, ArchiveType.Tar, CompressionType.None)) {
						Directory
							.GetFiles(directory, command + ".*")
							.ToList()
							.ForEach(x => addFile(zip, x, directory));
						addDirectory(zip, filesDirectory, directory);

						tar.Position = 0;
						using (var gz = File.OpenWrite(file)) {
							using (var gzWriter = WriterFactory.Open(gz, ArchiveType.GZip, CompressionType.GZip)) {
								gzWriter.Write("Tar.tar", tar, null);
							}
						}
					}
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("error|Exception during processing {0}", ex);
				throw;
			}
		}

		private static void addDirectory(IWriter archive, string directory, string rootPath) {
			addDirectory(archive, directory, rootPath, "");
		}

		private static void addDirectory(IWriter archive, string directory, string rootPath, string name) {
			foreach (var dir in Directory.GetDirectories(directory))
				addDirectory(archive, Path.Combine(directory, dir), rootPath, name + Path.GetFileName(dir) + "/");

			var files = Directory.GetFiles(directory);
			foreach (var file in files)
				addFile(archive, file, rootPath);
		}

		private static void addFile(IWriter archive, string file, string rootPath) {
			var dir = Path.GetDirectoryName(file);
			var pathInArchive = dir.Substring(rootPath.Length, dir.Length - rootPath.Length);
			archive.Write(Path.Combine(pathInArchive, Path.GetFileName(file)), file);
		}
	}
}