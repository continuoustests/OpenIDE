using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

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
               	run(directory, "tar", string.Format("--warning=none -xvf \"{0}\"", tarfile));
               	File.Delete(tarfile);
               	return;
           	}
			var inStream = File.OpenRead(archiveFile);
		    var gzipStream = new GZipInputStream(inStream);

		    var tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
		    tarArchive.ExtractContents(directory);
		    tarArchive.Close();

		    gzipStream.Close();
		    inStream.Close();
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
            proc.StartInfo.RedirectStandardError = true;
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
			if (!Directory.Exists(directory))
				return;

			var filesDirectory = Path.Combine(directory, command + "-files");
			var file = destination + ".oipkg";

			if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix) {
				var args = new StringBuilder();
				args.Append("\"" + file + "\" ");
				Directory
					.GetFiles(directory, command + ".*")
					.ToList()
					.ForEach(x => args.Append("\"" + toRelative(x, directory) + "\" "));
				args.Append("\"" + toRelative(filesDirectory, directory) + "\"");
				run(directory, "tar", "-czf " + args.ToString());
				return;
			}

			var currentDirectory = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(directory);
			try
			{
				var outStream = File.Create(file);
			    var gzoStream = new GZipOutputStream(outStream);
			    var tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

			    // Note that the RootPath is currently case sensitive and must be forward slashes e.g. "c:/temp"
			    // and must not end with a slash, otherwise cuts off first char of filename
			    // This is scheduled for fix in next release
			    tarArchive.RootPath = directory.Replace('\\', '/');
			    if (tarArchive.RootPath.EndsWith("/"))
			        tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

			    Directory
					.GetFiles(directory, command + ".*")
					.ToList()
					.ForEach(x => addFile(tarArchive, x));
				addDirectory(tarArchive, filesDirectory);

			    tarArchive.Close();
			}
			catch(Exception ex)
			{
				Console.WriteLine("error|Exception during processing {0}", ex);
				throw;
			}
			finally
			{
				Directory.SetCurrentDirectory(currentDirectory);
			}
		}

		private static void addDirectory(TarArchive tarArchive, string directory) {
			TarEntry tarEntry = TarEntry.CreateEntryFromFile(directory);
		    tarArchive.WriteEntry(tarEntry, false);

		    string[] filenames = Directory.GetFiles(directory);
		    foreach (string filename in filenames) {
		        addFile(tarArchive, filename);
		    }

	        string[] directories = Directory.GetDirectories(directory);
	        foreach (string dir in directories)
	            addDirectory(tarArchive, dir);
		}

		private static void addFile(TarArchive tarArchive, string filename) {
			var tarEntry = TarEntry.CreateEntryFromFile(filename);
		    tarArchive.WriteEntry(tarEntry, true);
		}

		private static string toRelative(string file, string basepath) {
			return file.Substring(basepath.Length + 1, file.Length - (basepath.Length + 1));
		}
	}
}