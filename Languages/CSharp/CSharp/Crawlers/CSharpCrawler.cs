using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using CSharp.Crawlers.TypeResolvers;
using CSharp.Projects;
using Mono.Cecil;

namespace CSharp.Crawlers
{
	public class CSharpCrawler : ICrawler
	{
        private bool _typeMatching = true;
		private IOutputWriter _builder;
        private List<string> _handledReferences = new List<string>();

		public CSharpCrawler(IOutputWriter writer)
		{
			_builder = writer;
		}

        public void SkipTypeMatching() 
        {
            _typeMatching = false;
        }

		public void Crawl(CrawlOptions options)
		{
			List<Project> projects;
			if (!options.IsSolutionFile && options.File != null)
			{
				parseFile(new FileRef(options.File, null));
				return;
			}
			else if (options.IsSolutionFile)
				projects = new SolutionReader(options.File).ReadProjects();
			else
				projects = getProjects(options.Directory);
            loadmscorlib();
			projects.ForEach(x => crawl(x));

            if (_typeMatching) {
                _builder.BuildTypeIndex();
                new TypeResolver(new OutputWriterCacheReader(_builder))
                    .ResolveAllUnresolved(_builder);
            }

            _builder.WriteToOutput();
		}

        private void loadmscorlib()
        {
            var asm = "mscorlib";
            parseAssembly(asm);
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
            var reader = new ProjectReader(project.File);
            // Get base assemblies first to better be equiped to locate variable types
            reader
                .ReadReferences()
                .ForEach(x =>
                {
                    if (!_handledReferences.Any(y => y.Equals(x)))
                    {
                        parseAssembly(x);
                        _handledReferences.Add(x);
                    }
                });

            _builder.WriteProject(project);
            reader
                .ReadFiles()
                .ForEach(x => {
					parseFile(new FileRef(x, project));
				});
		}

        private void parseAssembly(string x)
        {
            try
			{
                _builder.SetTypeVisibility(false);
                if (_handledReferences.Contains(x))
                    return;
                _handledReferences.Add(x);
                new AssemblyParser(_builder)
                    .Parse(x);
            }
			catch (Exception ex)
			{
                parseError(x, ex);
			}
            finally
            {
                _builder.SetTypeVisibility(true);
            }
        }

		private void parseFile(FileRef x)
		{
			try
			{
				new NRefactoryParser()
					.SetOutputWriter(_builder)
					.ParseFile(x, () => { return File.ReadAllText(x.File); });
			}
			catch (Exception ex)
			{
                parseError(x.File, ex);
			}
		}

        private void parseError(string x, Exception ex)
        {
            _builder.WriteError("Failed to parse " + x);
            _builder.WriteError(ex.Message.Replace(Environment.NewLine, ""));
            if (ex.StackTrace != null)
            {
                ex.StackTrace
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList()
                    .ForEach(line => _builder.WriteError(line));
            }
        }
	}
					
	public class Point
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

