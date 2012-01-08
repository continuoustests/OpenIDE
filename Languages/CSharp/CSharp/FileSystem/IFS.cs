using System;
namespace CSharp.FileSystem
{
	public interface IFS
	{
		string[] GetFiles(string path, string searchPattern);
        string ReadFileAsText(string path);
		string[] ReadLines(string path);
        bool DirectoryExists(string path);
		bool FileExists(string file);
		void WriteAllText(string file, string text);
		void DeleteFile(string file);
	}
}

