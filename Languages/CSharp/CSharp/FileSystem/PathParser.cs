using System;
using System.IO;
using System.Collections.Generic;

namespace CSharp.FileSystem
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

        public string RelativeTo(string rootPath)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix) 
                rootPath = rootPath.ToLower();
            var pathChunks = _path.Split(new[] {Path.DirectorySeparatorChar});
            var rootPathChunks = rootPath.Split(new[] {Path.DirectorySeparatorChar});
            if (_path.Length == 0)
                return "";
            if (rootPath.Length == 0)
                return _path;
            if (!_path.StartsWith(Path.DirectorySeparatorChar.ToString())) {
                if (pathChunks[0] != rootPathChunks[0])
                    return _path;
            }
            var relativePath = "";
            var max = pathChunks.Length > rootPathChunks.Length ? pathChunks.Length : rootPathChunks.Length;
            var isInPath = true;
            var paralellPath = "";
            for (int i = 0; max > i; i++) {
                if (i >= pathChunks.Length) {
                    relativePath = Path.Combine("..", relativePath);
                    continue;
                }
                if (i >= rootPathChunks.Length) {

                    if (isInPath)
                        relativePath = Path.Combine(relativePath, pathChunks[i]);
                    else
                        paralellPath = Path.Combine(paralellPath, pathChunks[i]);
                    continue;
                }
                if (isInPath && pathChunks[i] != rootPathChunks[i]) {
                    relativePath = Path.Combine(relativePath, pathChunks[i]);
                    isInPath = false;
                    continue;
                }
                if (!isInPath) {
                    relativePath = Path.Combine("..", relativePath);
                    paralellPath = Path.Combine(paralellPath, pathChunks[i]);
                    continue;
                }
            }
            if (paralellPath != "")
                relativePath = Path.Combine(relativePath, paralellPath);
            return relativePath;
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

