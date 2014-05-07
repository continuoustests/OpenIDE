using System;
using System.IO;
using OpenIDE.Core.Logging;

namespace OpenIDE.Core.FileSystem
{
	public class FS : IFS
	{
        public static string GetTempPath()
        {
            if (OS.IsOSX) {
                return "/tmp";
            }
            return Path.GetTempPath();
        }

        public static string GetTempFileName()
        {
            var rnd = new Random();
            var path = Path.Combine(GetTempPath(), "tmp" + rnd.Next().ToString("x") + ".tmp");
            while (File.Exists(path)) {
                path = Path.Combine(GetTempPath(), "tmp" + rnd.Next().ToString("x") + ".tmp");
            }
            return path;
        }

		public string[] GetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
        }

		public string[] ReadLines(string path)
		{
			return File.ReadAllLines(path);
		}
		
        public string ReadFileAsText(string path)
        {
            using (var reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }
		
		public bool FileExists(string file)
		{
			return File.Exists(file);
		}
		
		public void WriteAllText(string file, string text)
		{
			File.WriteAllText(file, text);
		}
		
		public void DeleteFile(string file)
		{
			File.Delete(file);
		}
	}
}

