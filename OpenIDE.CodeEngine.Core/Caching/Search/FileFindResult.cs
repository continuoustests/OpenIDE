using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenIDE.CodeEngine.Core.Caching.Search
{
    public enum FileFindResultType
    {
        DirectoryInProject,
        Directory,
        Project,
        File
    }

    public class FileFindResult
    {
        public FileFindResultType Type { get; private set; }
        public string File { get; private set; }
        public string ProjectPath { get; private set; }

        public FileFindResult(FileFindResultType type, string file)
        {
            Type = type;
            File = file;
            ProjectPath = null;
        }

        public FileFindResult(FileFindResultType type, string file, string projectPath)
        {
            Type = type;
            File = file;
            ProjectPath = projectPath;
        }
    }
}
