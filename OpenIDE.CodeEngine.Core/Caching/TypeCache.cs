using System;
using System.Collections.Generic;
using System.Linq;
using OpenIDE.CodeEngine.Core.Caching.Search;
using OpenIDE.Core.Caching;
using System.IO;
namespace OpenIDE.CodeEngine.Core.Caching
{
	public class TypeCache : ICacheBuilder, ITypeCache, ICrawlResult
	{
		private List<CachedPlugin> _plugins = new List<CachedPlugin>();
		private List<Project> _projects = new List<Project>();
		private List<ProjectFile> _files = new List<ProjectFile>();
		private List<ICodeReference> _codeReferences = new List<ICodeReference>();
		private List<ISignatureReference> _signatureReferences = new List<ISignatureReference>();

		public List<CachedPlugin> Plugins { get { return _plugins;  } }

		public int ProjectCount { get { return _projects.Count; } }
		public int FileCount { get { return _files.Count(x => x.FileSearch); } }
		public int CodeReferences { get { return _codeReferences.Count(x => x.TypeSearch); } }
		
		public IEnumerable<Project> AllProjects()
		{
			return _projects;
		}

		public IEnumerable<ProjectFile> AllFiles()
		{
			return _files;
		}

		public IEnumerable<ICodeReference> AllReferences()
		{
			return _codeReferences;
		}

		public IEnumerable<ISignatureReference> AllSignatures()
		{
			return _signatureReferences;
		}

		public List<ICodeReference> Find(string name)
		{
			return _codeReferences
				.Where(x => x.TypeSearch &&
					  	(
					  		x.Signature.ToLower().Contains(name.ToLower()) ||
							x.File.ToLower().Contains(name.ToLower())
					  	)
					  )
				.OrderBy(x => nameSort(x.Name, x.Signature, x.File, name)).ToList();
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
			var project = GetProject(file);
			if (project != null) {
				lock (_files) {
					_files.RemoveAll(x => x.Project != null && x.Project.Equals(file));
				}
			}
			else {
				lock (_files) {
					lock (_codeReferences) {
						_files.RemoveAll(x => x.File.Equals(file));
						_codeReferences.RemoveAll(x => x.File.Equals(file));
						_signatureReferences.RemoveAll(x => x.File.Equals(file));
					}
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

		public void Add(ISignatureReference reference)
		{
			lock (_signatureReferences)
				_signatureReferences.Add(reference);
		}
		
		private int nameSort(string name, string signature, string filename, string compareString)
		{
			return new SearchSorter().Sort(name, signature, filename, compareString);
		}
    }
}

