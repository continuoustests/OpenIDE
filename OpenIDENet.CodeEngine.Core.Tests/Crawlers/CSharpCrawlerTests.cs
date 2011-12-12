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
		public List<Namespace> Namespaces = new List<Namespace>();
		public List<Class> Classes = new List<Class>();
		public List<Interface> Interfaces = new List<Interface>();
		public List<Struct> Structs = new List<Struct>();
		public List<EnumType> Enums = new List<EnumType>();

		public int ProjectCount { get { return 0; } }
		public int FileCount { get { return Files.Count; } }
		public int NamespaceCount { get { return Namespaces.Count; } }
		public int TypeCount { get { return Classes.Count + Interfaces.Count + Structs.Count + Enums.Count; } }
		
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

		public void AddFile (string file)
		{
			Files.Add(file);
		}
		
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

		public void AddClasses (IEnumerable<Class> classes)
		{
			Classes.AddRange(classes);
		}
		
		public void AddInterface(Interface iface)
		{
			Interfaces.Add(iface);
		}
		
		public void AddInterfaces(IEnumerable<Interface> interfaces)
		{
			Interfaces.AddRange(interfaces);
		}
		
		public void AddStruct(Struct str)
		{
			Structs.Add(str);
		}
			
		public void AddStructs(IEnumerable<Struct> structs)
		{
			Structs.AddRange(structs);
		}
		
		public void AddEnum(EnumType enu)
		{
			Enums.Add(enu);
		}
		
		public void AddEnums(IEnumerable<EnumType> enums)
		{
			Enums.AddRange(enums);
		}
	}
}

