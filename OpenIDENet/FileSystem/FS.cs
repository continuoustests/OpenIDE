using System;
using System.IO;
namespace OpenIDENet.FileSystem
{
	public class FS : IFS
	{
		public string[] GetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
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
	}
}

