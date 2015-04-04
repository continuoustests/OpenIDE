using System;
using System.IO;
namespace OpenIDE.Core.FileSystem
{	
	public interface IFS
	{
		string[] GetFiles(string path, string searchPattern);
		string[] GetFiles(string path, string searchPattern, SearchOption option);
        string ReadFileAsText(string path);
		string[] ReadLines(string path);
        bool DirectoryExists(string path);
		bool FileExists(string file);
		void WriteAllText(string file, string text);
		void DeleteFile(string file);
	}
}

