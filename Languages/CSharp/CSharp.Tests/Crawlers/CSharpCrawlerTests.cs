using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using CSharp.FileSystem;
using System.Reflection;
using System.Threading;

namespace CSharp.Tests.Crawlers
{
	// TODO Fix, cannot test using cache
	/*[TestFixture]
	public class CSharpCrawlerTests
	{
		[Test]
		public void Should_crawl_this_project()
		{
			var cache = new Fake_CacheBuilder();
			new CSharpCrawler(cache)
				.InitialCrawl(new CrawlOptions(new PathParser(string.Format("..{0}..{0}", Path.DirectorySeparatorChar)).ToAbsolute(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))));
			Thread.Sleep(1500);
			Assert.That(cache.Classes.Count, Is.GreaterThan(0));
			Assert.That(cache.Namespaces[0].Name, Is.EqualTo("OpenIDENet.CodeEngine.Core.Tests.Caching"));
			Assert.That(cache.Namespaces[0].Line, Is.EqualTo(10));
			xPlatformAssert(cache.Namespaces[0].Column, 10, 11);
			Assert.That(cache.Classes[0].Name, Is.EqualTo("HierarchyBuilderTests"));
			Assert.That(cache.Classes[0].Signature, Is.EqualTo("OpenIDENet.CodeEngine.Core.Tests.Caching.HierarchyBuilderTests"));
			Assert.That(cache.Classes[0].Line, Is.EqualTo(13));
			Assert.That(cache.Classes[0].Column, Is.EqualTo(17));
			
			Assert.That(cache.Classes.Exists(x => x.Name.Equals("Fake_CacheBuilder")), Is.True);
		}

		private void xPlatformAssert(int column, int expectedNix, int expectedWin)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
				Assert.That(column, Is.EqualTo(expectedNix));
			else
				Assert.That(column, Is.EqualTo(expectedWin));
		}
	}
	
	class Fake_CacheBuilder : ICacheBuilder
	{
		public List<string> Files = new List<string>();
		public List<ICodeReference> Enums = new List<ICodeReference>();

		public int ProjectCount { get { return 0; } }
		public int FileCount { get { return Files.Count; } }
		public int CodeReferences { get { return Classes.Count + Interfaces.Count + Structs.Count + Enums.Count; } }
		
		public bool ProjectExists(Project project)
		{
			return true;
		}
		
		public void AddProject(Project project)
		{
		}
		
		public Project GetProject(string fullpath)
		{
			return null;
		}
		
		public bool FileExists(string file)
		{
			return true;
		}
		
		public void Invalidate(string file)
		{
		}
	}*/
}

