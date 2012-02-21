using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using OpenIDE.Core.Caching;
using OpenIDE.CodeEngine.Core.ChangeTrackers;
using OpenIDE.CodeEngine.Core.Caching;

namespace OpenIDE.CodeEngine.Core.Tests
{
	[TestFixture]
	public class CrawlHandlerTests
	{
		private CrawlHandler _crawlHandler;
		private Fake_Cache _cache;
		
		[SetUp]
		public void Setup()
		{
			_cache = new Fake_Cache();
			_crawlHandler = new CrawlHandler(_cache, (s) => {});
		}
		
		[Test]
		public void When_given_files_and_types_it_will_add_them_to_cache()
		{
			var lines = new string[]
				{
					"file|/some/file.cs",
					"signature|NS.MyClass|MyClass|class|12|2|3",
					"signature|NS.MyOtherClass|MyOtherClass|class|104|54|6",
					"reference|NS.MyOtherClass|167|63|18"
				};
			
			lines.ToList()
				.ForEach(x => _crawlHandler.Handle(x));

			Assert.That(_cache.Files[0].File, Is.EqualTo("/some/file.cs"));
			Assert.That(_cache.Files[0].Project, Is.Null);

			Assert.That(_cache.References[0].Type, Is.EqualTo("class"));
			Assert.That(_cache.References[0].File, Is.EqualTo("/some/file.cs"));
			Assert.That(_cache.References[0].Signature, Is.EqualTo("NS.MyClass"));
			Assert.That(_cache.References[0].Name, Is.EqualTo("MyClass"));
			Assert.That(_cache.References[0].Offset, Is.EqualTo(12));
			Assert.That(_cache.References[0].Line, Is.EqualTo(2));
			Assert.That(_cache.References[0].Column, Is.EqualTo(3));

			Assert.That(_cache.References[1].Type, Is.EqualTo("class"));
			Assert.That(_cache.References[1].File, Is.EqualTo("/some/file.cs"));
			Assert.That(_cache.References[1].Signature, Is.EqualTo("NS.MyOtherClass"));
			Assert.That(_cache.References[1].Name, Is.EqualTo("MyOtherClass"));
			Assert.That(_cache.References[1].Offset, Is.EqualTo(104));
			Assert.That(_cache.References[1].Line, Is.EqualTo(54));
			Assert.That(_cache.References[1].Column, Is.EqualTo(6));

			Assert.That(_cache.SignatureReferences[0].Signature, Is.EqualTo("NS.MyOtherClass"));
			Assert.That(_cache.SignatureReferences[0].Offset, Is.EqualTo(167));
			Assert.That(_cache.SignatureReferences[0].Line, Is.EqualTo(63));
			Assert.That(_cache.SignatureReferences[0].Column, Is.EqualTo(18));
		}

		[Test]
		public void When_given_a_project_it_reference_files_to_project()
		{
			var lines = new string[]
				{
					"project|/some/project.csproj",
					"file|/some/file.cs"
				};
			
			lines.ToList()
				.ForEach(x => _crawlHandler.Handle(x));

			Assert.That(_cache.Projects[0].File, Is.EqualTo("/some/project.csproj"));		

			Assert.That(_cache.Files[0].File, Is.EqualTo("/some/file.cs"));		
			Assert.That(_cache.Files[0].Project, Is.EqualTo("/some/project.csproj"));		
		}

		[Test]
		public void When_given_an_invalid_string_it_should_not_throw_exception()
		{
			_crawlHandler.Handle("signature|invalid stuff");
		}

		[Test]
		public void When_given_file_explore_tag_it_will_mark_it()
		{
			var lines = new string[]
				{
					"project|/some/project.csproj|filesearch",
					"file|/some/file.cs|filesearch"
				};
			
			lines.ToList()
				.ForEach(x => _crawlHandler.Handle(x));

			Assert.That(_cache.Projects[0].File, Is.EqualTo("/some/project.csproj"));		
			Assert.That(_cache.Projects[0].FileSearch, Is.True);

			Assert.That(_cache.Files[0].File, Is.EqualTo("/some/file.cs"));		
			Assert.That(_cache.Files[0].Project, Is.EqualTo("/some/project.csproj"));		
			Assert.That(_cache.Files[0].FileSearch, Is.True);
		}

		[Test]
		public void When_given_type_search_tag_it_will_mark_it()
		{
			var lines = new string[]
				{
					"file|/some/file.cs",
					"signature|NS.MyClass|MyClass|class|12|2|3|typesearch"
				};
			
			lines.ToList()
				.ForEach(x => _crawlHandler.Handle(x));

			Assert.That(_cache.References[0].TypeSearch, Is.True);
		}
	}

	class Fake_Cache : ICacheBuilder, ICrawlResult
	{
		public List<CachedPlugin> Plugins { get { return null; } }
		public int ProjectCount { get; private set; }
		public int FileCount { get; private set; }
		public int CodeReferences { get; private set; }

		public bool ProjectExists(Project project) { return false; }
		public Project GetProject(string path) { return null; }
		public bool FileExists(string file) { return false; }
		public void Invalidate(string file) { }

		public List<Project> Projects = new List<Project>();
		public void Add(Project project) {
			Projects.Add(project);
		}

		public List<ProjectFile> Files = new List<ProjectFile>();
		public void Add(ProjectFile file) {
			Files.Add(file);
		}

		public List<ICodeReference> References = new List<ICodeReference>();
		public void Add(ICodeReference reference) {
			References.Add(reference);
		}

		public void Add(IEnumerable<ICodeReference> references) {
			References.AddRange(references);
		}

		public List<ISignatureReference> SignatureReferences = new List<ISignatureReference>();
		public void Add(ISignatureReference reference) {
			SignatureReferences.Add(reference);
		}
	}
}
