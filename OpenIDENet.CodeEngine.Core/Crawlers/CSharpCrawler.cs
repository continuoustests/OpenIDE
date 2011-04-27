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
			var files = new ProjectReader(project.Fullpath).ReadFiles();
			files.ForEach(x => parseFile(x));
		}
		
		private void parseFile(string file)
		{
			var content = File.ReadAllText(file);
			var lines = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
			var namespaces = getNamespaces(file, content, lines);
			var classes = getClasses(file, content, lines, namespaces);
			lock (_builder)
			{
				_builder.AddNamespaces(namespaces);
				_builder.AddClasss(classes);
			}
		}
		
		private List<Namespace> getNamespaces(string file, string content, string[] lines)
		{
			var namespaces = getInstances(file, lines, "namespace", "{");
			return namespaces.Select(x => new Namespace(file, trim(content.Substring(x.Start.Offset, x.End.Offset - x.Start.Offset), x.Pattern), x.Start.Offset, x.Start.Line, x.Start.Column)).ToList();
		}
		
		private List<Class> getClasses(string file, string content, string[] lines, List<Namespace> namespaces)
		{
			var classes = getInstances(file, lines, "class", "{");
			return classes.Select(x => new Class(
											file,
			                                getNamespaceFor(x, namespaces),
			                                getClassName(trim(content.Substring(x.Start.Offset, x.End.Offset - x.Start.Offset), x.Pattern)),
			                                x.Start.Offset,
			                                x.Start.Line,
			                                x.Start.Column)).ToList();
		}
		
		private string getClassName(string text)
		{
			var colon = text.IndexOf(":");
			if (colon != -1)
				return text.Substring(0, colon);
			return text;
		}
		
		private string getNamespaceFor(SearchPoint s, List<Namespace> namespaces)
		{
			var ns = namespaces.Where(n => n.Offset < s.Start.Offset).OrderByDescending(y => y.Offset).FirstOrDefault();
			if (ns == null)
				return null;
			return ns.Name;
		}
			                                     
		private string trim(string text, string additionalTrimPattern)
		{
			if (text == null)
				return "";
			return text
				.Replace(additionalTrimPattern, "")
				.Replace(Environment.NewLine, "")
				.Replace("\t", "")
				.Replace(" ", "");
		}
		
		private List<SearchPoint> getInstances(string file, string[] content, string keyword, string terminatedBy)
		{
			return getOccurences(content,
									new string[] {
											string.Format("\t{0} ", keyword),
											string.Format("\t{0}\t", keyword),
											string.Format("{0} ", keyword),
								            string.Format("{0}\t", keyword),
											string.Format(" {0} ", keyword),
											string.Format(" {0}\t", keyword)
										}, terminatedBy);
		}
		
		private List<SearchPoint> getOccurences(string[] content, string[] patterns, string terminatedBy)
		{
			int offset = 0;
			var points = new List<SearchPoint>();
			for (int i = 0; i < content.Length; i++)
			{
				// Find new patterns
				var hit = patterns
							.Where(x => content[i].IndexOf(x) != -1)
							.Select(x => new SearchPoint(x,
									   				offset + content[i].IndexOf(x),
									                i + 1, 
									                content[i].IndexOf(x, 0),
									                offset + content[i].IndexOf(terminatedBy, content[i].IndexOf(x) + x.Length),
									                i + 1,
									                content[i].IndexOf(terminatedBy, content[i].IndexOf(x) + x.Length))).ToList()
							.FirstOrDefault();
				if (hit != null)
					points.Add(hit);
				// Find end of still open points
				points
					.Where(x => x.End.Column.Equals(-1))
					.Where(x => content[i].IndexOf(terminatedBy) != -1).ToList()
					.ForEach(x => {
									x.End.Offset = offset + content[i].IndexOf(terminatedBy);
									x.End.Line = i + 1;
									x.End.Column = content[i].IndexOf(terminatedBy);
								  });
				offset += content[i].Length + Environment.NewLine.Length;
			}
			return points;
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

