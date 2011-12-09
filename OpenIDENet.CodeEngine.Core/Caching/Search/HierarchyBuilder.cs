using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OpenIDENet.CodeEngine.Core.Caching.Search
{
    class HierarchyBuilder
    {
        private List<string> _files;
        private List<Project> _projects;

        public HierarchyBuilder(List<string> files, List<Project> projects)
        {
            _files = files;
            _projects = projects;
        }

        public HierarchyBuilder()
        {
            _files = null;
            _projects = null;
        }

        public List<FileFindResult> GetNextStep(string directory)
        {
            var list = new List<FileFindResult>();
            _projects
                .Where(x => x.Fullpath.StartsWith(directory))
                .Select(x => new FileFindResult(FileFindResultType.Project, x.Fullpath)).ToList()
                .ForEach(x => addResult(list, x));
            _files
                .Where(x => x.StartsWith(directory))
                .Select(x => getResult(x, directory, null, FileFindResultType.Directory)).ToList()
                .ForEach(x => addResult(list, x));
            return list;
        }

        public List<FileFindResult> GetNextStepInProject(Project project)
        {
            return GetNextStepInProject(project, Path.GetDirectoryName(project.Fullpath));
        }

        public List<FileFindResult> GetNextStepInProject(Project project, string directory)
        {
            var list = new List<FileFindResult>();
            project.Files
                .Where(x => x.StartsWith(directory))
                .Select(x => getResult(x, directory, project.Fullpath, FileFindResultType.DirectoryInProject)).ToList()
                .ForEach(x => addResult(list, x));
            return list;
        }

        private void addResult(List<FileFindResult> list, FileFindResult x)
        {
            if (list.Count(y => y.File.Equals(x.File) && y.Type.Equals(x.Type)) == 0)
                list.Add(x);
        }

        private static FileFindResult getResult(string x, string searchDir, string project, FileFindResultType typeWhenDirectory)
        {
            if (x.IndexOf(Path.DirectorySeparatorChar, searchDir.Length + 1) == -1)
                return new FileFindResult(FileFindResultType.File, x, project);
            else
                return new FileFindResult(typeWhenDirectory, x.Substring(0, x.IndexOf(Path.DirectorySeparatorChar, searchDir.Length + 1)), project);
        }
    }
}
