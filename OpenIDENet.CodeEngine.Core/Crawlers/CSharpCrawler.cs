using System;
using OpenIDENet.CodeEngine.Core.Caching;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
namespace OpenIDENet.CodeEngine.Core.Crawlers
{
	public class CSharpCrawler : ICrawler
	{
		private ICacheBuilder _builder;
		
		public CSharpCrawler(ICacheBuilder builder)
		{
			_builder = builder;
		}
		
		public void InitialCrawl(CrawlOptions options)
		{
			ThreadPool.QueueUserWorkItem(initialCrawl, options);
		}
		
		private void initialCrawl(object state)
		{
			var options = (CrawlOptions) state;
			List<Project> projects;
			if (options.IsSolutionFile)
				projects = new SolutionReader(options.File).ReadProjects();
			else
				projects = getProjects(options.Directory);
			projects.ForEach(x => crawl(x));	
		}
		
		private List<Project> getProjects(string folder)
		{
			var projects = new List<Project>();
			Directory.GetDirectories(folder).ToList()
				.ForEach(x => projects.AddRange(getProjects(x)));
			Directory.GetFiles(folder, "*.csproj").ToList()
				.ForEach(x => projects.Add(new Project(x)));
			return projects;
		}
		
		private void crawl(Project project)
		{
			if (!_builder.ProjectExists(project))
				_builder.AddProject(project);
			var files = new ProjectReader(project.Fullpath).ReadFiles();
			files.ForEach(x => new CSharpFileParser(_builder).ParseFile(x, () => { return File.ReadAllText(x); }));
			
		}
	}
					
	class Point
	{
		public int Offset { get; set; }
		public int Line { get; set; }
		public int Column { get; set; }
						
		public Point(int offset, int line, int column)
		{
			Offset = offset;
			Line  = line;
			Column = column;
		}
	}
	
	class SearchPoint
	{
		public string Pattern { get; private set; }
		public Point Start { get; private set; }
		public Point End { get; private set; }
		
		public SearchPoint(string pattern, int startOffset, int startLine, int startColumn, int endOffset, int endLine, int endColumn)
		{
			Pattern = pattern;
			Start = new Point(startOffset, startLine, startColumn);
			End = new Point(endOffset, endLine, endColumn);
		}
	}
}

