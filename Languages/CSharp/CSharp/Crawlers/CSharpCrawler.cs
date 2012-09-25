using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using CSharp.Projects;
using Mono.Cecil;

namespace CSharp.Crawlers
{
	public class CSharpCrawler : ICrawler
	{
		private IOutputWriter _builder;
        private List<string> _handledReferences = new List<string>();

		public CSharpCrawler(IOutputWriter writer)
		{
			_builder = writer;
		}

		public void Crawl(CrawlOptions options)
		{
			List<Project> projects;
			if (!options.IsSolutionFile && options.File != null)
			{
				parseFile(options.File);
				return;
			}
			else if (options.IsSolutionFile)
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
			_builder.AddProject(project);
            var reader = new ProjectReader(project.File);
            reader
                .ReadFiles()
                .ForEach(x => {
					parseFile(x);
				});

            reader
                .ReadReferences()
                .ForEach(x => {
                    if (!_handledReferences.Any(y => y.Equals(x))) {
                        parseAssembly(x);
                        _handledReferences.Add(x);
                    }
                });
		}

        private void parseAssembly(string x)
        {
            try
			{
                new AssemblyParser(_builder)
                    .Parse(x);
            }
			catch (Exception ex)
			{
                parseError(x, ex);
			}
        }

		private void parseFile(string x)
		{
			try
			{
				new NRefactoryParser()
					.SetOutputWriter(_builder)
					.ParseFile(x, () => { return File.ReadAllText(x); });
			}
			catch (Exception ex)
			{
                parseError(x, ex);
			}
		}

        private void parseError(string x, Exception ex)
        {
            _builder.Error("Failed to parse " + x);
            _builder.Error(ex.Message.Replace(Environment.NewLine, ""));
            if (ex.StackTrace != null)
            {
                ex.StackTrace
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList()
                    .ForEach(line => _builder.Error(line));
            }
        }
	}
					
	class Point
	{
		public int Line { get; set; }
		public int Column { get; set; }
						
		public Point(int line, int column)
		{
			Line  = line;
			Column = column;
		}
	}
	
	class SearchPoint
	{
		public string Pattern { get; private set; }
		public Point Start { get; private set; }
		public Point End { get; private set; }
		
		public SearchPoint(string pattern, int startLine, int startColumn, int endLine, int endColumn)
		{
			Pattern = pattern;
			Start = new Point(startLine, startColumn);
			End = new Point(endLine, endColumn);
		}
	}
}

