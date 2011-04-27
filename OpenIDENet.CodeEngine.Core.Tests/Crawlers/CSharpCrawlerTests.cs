using System;
using NUnit.Framework;
using OpenIDENet.CodeEngine.Core.Caching;
using System.Collections.Generic;
using OpenIDENet.CodeEngine.Core.Crawlers;
using System.IO;
using OpenIDENet.CodeEngine.Core.FileSystem;
using System.Reflection;
using System.Threading;
namespace OpenIDENet.CodeEngine.Core.Tests.Crawlers
{
	[TestFixture]
	public class CSharpCrawlerTests
	{
		[Test]
		public void Should_crawl_this_project()
		{
			var cache = new Fake_CacheBuilder();
			new CSharpCrawler(cache)
				.InitialCrawl(new CrawlOptions(new PathParser(string.Format("..{0}..{0}", Path.DirectorySeparatorChar)).ToAbsolute(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))));
			Thread.Sleep(300);
			Assert.That(cache.Classes.Count, Is.GreaterThan(0));
			Assert.That(cache.Namespaces[0].Name, Is.EqualTo("OpenIDENet.CodeEngine.Core.Tests.Crawlers"));
			Assert.That(cache.Namespaces[0].Line, Is.EqualTo(5));
			Assert.That(cache.Namespaces[0].Column, Is.EqualTo(0));
			Assert.That(cache.Classes[0].Name, Is.EqualTo("SolutionReaderTests"));
			Assert.That(cache.Classes[0].Signature, Is.EqualTo("OpenIDENet.CodeEngine.Core.Tests.Crawlers.SolutionReaderTests"));
			Assert.That(cache.Classes[0].Line, Is.EqualTo(8));
			Assert.That(cache.Classes[0].Column, Is.EqualTo(8));
			
			Assert.That(cache.Classes.Exists(x => x.Name.Equals("Fake_CacheBuilder")), Is.True);
		}
	}
	
	class Fake_CacheBuilder : ICacheBuilder
	{
		public List<Namespace> Namespaces = new List<Namespace>();
		public List<Class> Classes = new List<Class>();

		public void AddNamespace (Namespace ns)
		{
			Namespaces.Add(ns);
		}

		public void AddNamespaces (IEnumerable<Namespace> namespaces)
		{
			Namespaces.AddRange(namespaces);
		}

		public void AddClass (Class cls)
		{
			Classes.Add(cls);
		}

		public void AddClasss (IEnumerable<Class> classes)
		{
			Classes.AddRange(classes);
		}
	}
}

