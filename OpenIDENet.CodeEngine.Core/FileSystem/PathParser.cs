using System;
using System.IO;
using System.Collections.Generic;
namespace OpenIDENet.CodeEngine.Core.FileSystem
{
	public class PathParser
    {
        private string _path;

        public PathParser(string path)
        {
            _path = path;
        }

        public string ToAbsolute(string relativeTo)
        {
            try
            {
                if (relativeTo.Trim().Length.Equals(0))
                    return _path;
                relativeTo = Path.GetFullPath(relativeTo);
                if (File.Exists(relativeTo))
                    relativeTo = Path.GetDirectoryName(relativeTo);
                var combinedPath = Path.Combine(relativeTo, _path);
                var chunkList = new List<string>();
                if (combinedPath.StartsWith(Path.DirectorySeparatorChar.ToString()))
                    chunkList.Add(Path.DirectorySeparatorChar.ToString());
                chunkList.AddRange(combinedPath.Split(Path.DirectorySeparatorChar));
                var chunks = chunkList.ToArray();
                var folders = makeAbsolute(chunks);
                var path = buildPathFromArray(folders);
                return Path.GetFullPath(path);
            }
            catch
            {
            }
            return _path;
        }

        private List<string> makeAbsolute(string[] chunks)
        {
            List<string> folders = new List<string>();
            foreach (var chunk in chunks)
            {
                if (chunk.Equals(".."))
                {
                    folders.RemoveAt(folders.Count - 1);
                    continue;
                }
                folders.Add(chunk);
            }
            return folders;
        }

        private string buildPathFromArray(List<string> folders)
        {
            var path = "";
            foreach (var item in folders)
            {
                if (path.Length == 0)
                    path += item;
                else
                {
                    if (path.Equals(Path.DirectorySeparatorChar.ToString()))
                        path += item;
                    else
                        path += string.Format("{0}{1}", Path.DirectorySeparatorChar, item);
                }
            }
            return path;
        }
    }
}

