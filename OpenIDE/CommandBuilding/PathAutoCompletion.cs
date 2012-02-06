using System;
using System.IO;
using System.Linq;

namespace OpenIDE.CommandBuilding
{
	public class PathAutoCompletion
	{
        public string AutoComplete(string command, string typedText)
        {
            var dirContent = typedText;
            var lookFor = Path.DirectorySeparatorChar;
            if (dirContent.LastIndexOf(Path.DirectorySeparatorChar) < dirContent.LastIndexOf(' ') && (dirContent.Count(x => x.Equals('"')) % 2) == 0)
                lookFor = ' ';
            var dir = dirContent.Substring(dirContent.LastIndexOf(lookFor) + 1, dirContent.Length - (dirContent.LastIndexOf(lookFor) + 1));
            var item = command;
            if (item.StartsWith(dir) && item.Length > dir.Length)
                return item.Substring(dir.Length, item.Length - dir.Length);
            else
                return item;
        }
	}
}