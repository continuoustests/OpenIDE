using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using OpenIDENet.CodeEngine.Core.Caching;
using OpenIDENet.CodeEngine.Core.Crawlers;
using System.Linq;
namespace OpenIDENet.CodeEngine.Core.ChangeTrackers
{
	public class CSharpFileTracker
	{
		private FileChangeTracker _tracker;
		private ICacheBuilder _cache;
		
		public void Start(string path, ICacheBuilder cache)
		{
			_cache = cache;
			_tracker = new FileChangeTracker();
			_tracker.Start(path, "*.cs*", handleChanges);
		}
			               
		private void handleChanges(Stack<FileSystemEventArgs> buffer)
		{
			var files = getChanges(buffer);
			files.ForEach(x => handle(x));
		}
		
		private List<FileSystemEventArgs> getChanges(Stack<FileSystemEventArgs> buffer)
		{
			var list = new List<FileSystemEventArgs>();
			while (buffer.Count != 0)
			{
				var item = buffer.Pop();
				if (!list.Contains(item))
					list.Add(item);
			}
			return list;
		}
		
		private void handle(FileSystemEventArgs file)
		{
			if (file == null)
			{
				Console.WriteLine("FS args is null???");
				return;
			}
			var extension = Path.GetExtension(file.FullPath).ToLower();
			if (extension == null)
				return;
			if (extension.Equals(".csproj"))
				handleProject(file);
			else if (extension.Equals(".cs"))
				handleFile(file);
		}
		
		private void handleProject(FileSystemEventArgs file)
		{
			lock (_cache)
			{
				var project = _cache.GetProject(file.FullPath);
				var files = new ProjectReader(file.FullPath).ReadFiles();
				if (project == null)
					addProject(file.FullPath, files);
				else
					updateProject(project, files);
			}
			
		}
		
		private void addProject(string file, List<string> files)
		{
			var project = new Project(file);
			project.Files.AddRange(files);
			_cache.AddProject(project);
			project.Files.ForEach(x => addFile(x));
		}
		
		private void updateProject(Project project, List<string> files)
		{
			project.Files.Where(x => !files.Contains(x)).ToList()
				.ForEach(x => {
								project.Files.Remove(x);
								removeFile(x);
							  });
			files.Where(x => !project.Files.Contains(x)).ToList()
				.ForEach(x => {
								project.Files.Add(x);
								addFile(x);
							   });
		}
		
		private void addFile(string file)
		{
			if (!_cache.FileExists(file))
				new CSharpFileParser(_cache).ParseFile(file, () => { return File.ReadAllText(file); });
		}
		
		private void removeFile(string file)
		{
			_cache.Invalidate(file);
		}
		
		private void handleFile(FileSystemEventArgs file)
		{
			_cache.Invalidate(file.FullPath);
			if (file.ChangeType != WatcherChangeTypes.Deleted)
				new CSharpFileParser(_cache).ParseFile(file.FullPath, () => { return File.ReadAllText(file.FullPath); });
		}
	}
}

