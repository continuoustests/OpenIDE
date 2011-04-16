using System;
using OpenIDENet.FileSystem;
using System.IO;
using System.Linq;
using System.Collections.Generic;
namespace OpenIDENet.Projects
{
	public class ProjectLocator : ILocateClosestProject
	{
		private const string CSHARP_PROJECT_EXTENTION = ".csproj";
        private const string VB_PROJECT_EXTENTION = ".vbproj";

        public string Locate(string directory)
        {
			var path = Path.GetFullPath(directory);
            if (!Directory.Exists(path))
                return null;
			var extensions = new List<string>();
			extensions.Add(CSHARP_PROJECT_EXTENTION);
			extensions.Add(VB_PROJECT_EXTENTION);
            return locateNearestProjectFile(path, extensions);
        }

        private string locateNearestProjectFile(string startDirectory, List<string> fileExtensions)
        {
            return find(new DirectoryInfo(startDirectory), new Func<FileInfo, bool>(x => fileExtensions.Contains(x.Extension)));
        }

        static string find(DirectoryInfo info, Func<FileInfo, bool> predicate)
        {
            if(info == null)
                return null;
            if (!info.Exists)
                return null;
            var files = info.GetFiles().Where(predicate).ToArray();
            if (files.Length > 0)
                return files[0].FullName;
			if (info.Parent == null)
				return null;
            return find(info.Parent, predicate);
        }
	}
}

