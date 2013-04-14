using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharp.FileSystem;
using System.Reflection;
using System.Threading;
using CSharp.Crawlers;

namespace CSharp.Tests.Crawlers
{
	[TestFixture]
	public class CSharpCrawlerTests
	{
		[Test]
		public void Should_crawl_this_project()
		{
			/*var cache = new OutputWriter();
            var dir = new PathParser(
                string.Format("..{0}..{0}", Path.DirectorySeparatorChar))
                    .ToAbsolute(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			var crawler = new CSharpCrawler(cache);
            crawler.SkipTypeMatching();
	        crawler.Crawl(new CrawlOptions(dir));
            var project = cache.Projects.FirstOrDefault(x => x.File == Path.Combine(dir, "CSharp.Tests.csproj"));
			Assert.That(cache.Classes.Count, Is.GreaterThan(0));
			Assert.That(cache.Namespaces[0].Name, Is.EqualTo("CSharp.Tests.Crawlers"));
			Assert.That(cache.Namespaces[0].Line, Is.EqualTo(8));
			Assert.That(cache.Namespaces[0].Column, Is.EqualTo(11));
            var cls = cache.Classes.FirstOrDefault(x => x.Name == "AssemblyParserTests");
            Assert.That(cls, Is.Not.Null);
            Assert.That(cls.Signature, Is.EqualTo("CSharp.Tests.Crawlers.AssemblyParserTests"));
            Assert.That(cls.Line, Is.EqualTo(11));
            Assert.That(cls.Column, Is.EqualTo(18));
			
			Assert.That(cache.Classes.Exists(x => x.Name.Equals("Fake_CacheBuilder")), Is.True);*/
		}

		private void xPlatformAssert(int column, int expectedNix, int expectedWin)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
				Assert.That(column, Is.EqualTo(expectedNix));
			else
				Assert.That(column, Is.EqualTo(expectedWin));
		}
	}
}

