using System;
using System.Collections.Generic;
using System.Linq;
using OpenIDENet.CodeEngine.Core.Caching.Search;
using System.IO;
namespace OpenIDENet.CodeEngine.Core.Caching
{
	public class TypeCache : ICacheBuilder, ITypeCache
	{
		
		
		private List<Project> _projects = new List<Project>();
		private List<ProjectFile> _files = new List<ProjectFile>();
		private List<ICodeReference> _codeReferences = new List<ICodeReference>();

		public int ProjectCount { get { return _projects.Count; } }
		public int FileCount { get { return _files.Count; } }
		public int CodeReferences { get { return _codeReferences.Count; } }
		
		public List<ICodeReference> Find(string name)
		{
			return _codeReferences.OrderBy(x => nameSort(x.Name, name)).ToList();
		}

        public List<FileFindResult> FindFiles(string searchString)
        {
            return new FileFinder(_files, _projects).Find(searchString);
        }

        public List<FileFindResult> GetFilesInDirectory(string directory)
        {
            return new HierarchyBuilder(_files, _projects).GetNextStep(directory);
        }

        public List<FileFindResult> GetFilesInProject(string project)
        {
            var prj = GetProject(project);
            if (prj == null)
                return new List<FileFindResult>();
            return new HierarchyBuilder(_files, _projects).GetNextStepInProject(prj);
        }

        public List<FileFindResult> GetFilesInProject(string project, string path)
        {
            var prj = GetProject(project);
            if (prj == null)
                return new List<FileFindResult>();
            return new HierarchyBuilder(_files, _projects).GetNextStepInProject(prj, path);
        }
	
		public bool ProjectExists(Project project)
		{
			return _projects.Exists(x => x.File.Equals(project.File));
		}
		
		public void Add(Project project)
		{
			lock (_projects)
			{
				_projects.Add(project);
			}
		}
		
		public Project GetProject(string fullpath)
		{
			lock (_projects)
			{
				return _projects.FirstOrDefault(x => x.File.Equals(fullpath));
			}
		}
		
		public bool FileExists(string file)
		{
			return _files.Count(x => x.File.Equals(file)) != 0;
		}
		
		public void Invalidate(string file)
		{
			lock (_files) {
				lock (_codeReferences) {
					_files.RemoveAll(x => x.File.Equals(file));
					_codeReferences.RemoveAll(x => x.File.Equals(file));
				}
			}
		}
		
		public void Add(ProjectFile file)
		{
			lock (_files)
				_files.Add(file);
		}
		
		public void Add(ICodeReference reference)
		{
			lock (_codeReferences)
				_codeReferences.Add(reference);
		}

		public void Add(IEnumerable<ICodeReference> references)
		{
			lock (_codeReferences)
				_codeReferences.AddRange(references);
		}
		
		private int nameSort(string name, string compareString)
		{
			if (name.Equals(compareString))
				return 1;
			if (name.ToLower().Equals(compareString.ToLower()))
				return 2;
			if (name.StartsWith(compareString))
				return 3;
			if (name.ToLower().StartsWith(compareString.ToLower()))
				return 4;
			if (name.EndsWith(compareString))
				return 5;
			if (name.ToLower().EndsWith(compareString.ToLower()))
				return 6;
			if (name.Contains(compareString))
				return 7;
			return 8;
		}
    }
}

