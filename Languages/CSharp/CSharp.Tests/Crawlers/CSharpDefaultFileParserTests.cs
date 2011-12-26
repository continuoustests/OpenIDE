using System;
using NUnit.Framework;
using System.Reflection;
using System.IO;
using System.Linq;
namespace OpenIDENet.CodeEngine.Core.Tests.Crawlers
{
	[TestFixture]
	public class CSharpDefaultFileParserTests
	{
		private CSharpFileParser _parser;
		private Fake_CacheBuilder _cache;
		
		[SetUp]
		public void Setup()
		{
			_cache = new Fake_CacheBuilder();
			_parser = new CSharpFileParser(_cache);
			_parser.ParseFile("file1", () => { return getContent(); });
		}
		
		[Test]
		public void Should_find_file()
		{
			Assert.That(_cache.Files.Count, Is.EqualTo(1));
			Assert.That(_cache.Files[0], Is.EqualTo("file1"));
		}
		
		[Test]
		public void Should_find_basic_namespace()
		{
			var cache = new Fake_CacheBuilder();
			_parser = new CSharpFileParser(cache);
			_parser.ParseFile("TestFile", () =>
				{
					return "namespace MyFirstNS {}";
				});
			var ns = cache.Namespaces.ElementAt(0);
			Assert.That(ns.Fullpath, Is.EqualTo("TestFile"));
			Assert.That(ns.Signature, Is.EqualTo("MyFirstNS"));
			Assert.That(ns.Name, Is.EqualTo("MyFirstNS"));
			Assert.That(ns.Offset, Is.EqualTo(11));
			Assert.That(ns.Line, Is.EqualTo(1));
			Assert.That(ns.Column, Is.EqualTo(11));
		}

		[Test]
		public void Should_find_namespace()
		{
			var ns = _cache.Namespaces.Where(x => x.Name.Equals("MyNamespace1")).FirstOrDefault();
			Assert.That(ns.Fullpath, Is.EqualTo("file1"));
			Assert.That(ns.Signature, Is.EqualTo("MyNamespace1"));
			Assert.That(ns.Name, Is.EqualTo("MyNamespace1"));
			xPlatformAssert(ns.Offset, 26, 28);
			Assert.That(ns.Line, Is.EqualTo(3));
			Assert.That(ns.Column, Is.EqualTo(10));
		}
		
		[Test]
		public void Should_find_class()
		{
			var cls = _cache.Classes.Where(x => x.Name.Equals("AVerySimpleClass")).FirstOrDefault();
			Assert.That(cls.Fullpath, Is.EqualTo("file1"));
			Assert.That(cls.Signature, Is.EqualTo("MyNamespace1.AVerySimpleClass"));
			Assert.That(cls.Namespace, Is.EqualTo("MyNamespace1"));
			Assert.That(cls.Name, Is.EqualTo("AVerySimpleClass"));
			xPlatformAssert(cls.Offset, 48, 52);
			Assert.That(cls.Line, Is.EqualTo(5));
			Assert.That(cls.Column, Is.EqualTo(7));
		}
		
		[Test]
		public void Should_find_inherited_class()
		{
			var cls = _cache.Classes.Where(x => x.Name.Equals("MyClass1")).FirstOrDefault();
			Assert.That(cls.Fullpath, Is.EqualTo("file1"));
			Assert.That(cls.Signature, Is.EqualTo("MyNamespace1.MyClass1"));
			Assert.That(cls.Namespace, Is.EqualTo("MyNamespace1"));
			Assert.That(cls.Name, Is.EqualTo("MyClass1"));
			xPlatformAssert(cls.Offset, 80, 88);
			Assert.That(cls.Line, Is.EqualTo(9));
			Assert.That(cls.Column, Is.EqualTo(7));
		}
		
		[Test]
		public void Should_find_multiline_namespace()
		{
			var ns = _cache.Namespaces.Where(x => x.Name.Equals("MyNamespace2")).FirstOrDefault();
			Assert.That(ns.Fullpath, Is.EqualTo("file1"));
			Assert.That(ns.Signature, Is.EqualTo("MyNamespace2"));
			Assert.That(ns.Name, Is.EqualTo("MyNamespace2"));
			xPlatformAssert(ns.Offset, 123, 137);
			Assert.That(ns.Line, Is.EqualTo(15));
			Assert.That(ns.Column, Is.EqualTo(0));
		}
		
		[Test]
		public void Should_find_multiline_classes()
		{
			var cls = _cache.Classes.Where(x => x.Name.Equals("MyClass2")).FirstOrDefault();
			Assert.That(cls.Fullpath, Is.EqualTo("file1"));
			Assert.That(cls.Signature, Is.EqualTo("MyNamespace2.MyClass2"));
			Assert.That(cls.Namespace, Is.EqualTo("MyNamespace2"));
			Assert.That(cls.Name, Is.EqualTo("MyClass2"));
			xPlatformAssert(cls.Offset, 154, 172);
			Assert.That(cls.Line, Is.EqualTo(19));
			Assert.That(cls.Column, Is.EqualTo(1));
		}
		
		[Test]
		public void Should_find_single_line_namespace()
		{
			var ns = _cache.Namespaces.Where(x => x.Name.Equals("MyNamespace3")).FirstOrDefault();
			Assert.That(ns.Fullpath, Is.EqualTo("file1"));
			Assert.That(ns.Signature, Is.EqualTo("MyNamespace3"));
			Assert.That(ns.Name, Is.EqualTo("MyNamespace3"));
			xPlatformAssert(ns.Offset, 182, 205);
			Assert.That(ns.Line, Is.EqualTo(24));
			Assert.That(ns.Column, Is.EqualTo(10));
		}
		
		[Test]
		public void Should_find_single_line_classes()
		{
			var cls = _cache.Classes.Where(x => x.Name.Equals("MyClass3")).FirstOrDefault();
			Assert.That(cls.Fullpath, Is.EqualTo("file1"));
			Assert.That(cls.Signature, Is.EqualTo("MyNamespace3.MyClass3"));
			Assert.That(cls.Namespace, Is.EqualTo("MyNamespace3"));
			Assert.That(cls.Name, Is.EqualTo("MyClass3"));
			xPlatformAssert(cls.Offset, 212, 235);
			Assert.That(cls.Line, Is.EqualTo(24));
			Assert.That(cls.Column, Is.EqualTo(40));
		}
		
		[Test]
		public void Should_find_bizarro_namespace()
		{
			var ns = _cache.Namespaces.Where(x => x.Name.Equals("MyNamespace4")).FirstOrDefault();
			Assert.That(ns.Fullpath, Is.EqualTo("file1"));
			Assert.That(ns.Signature, Is.EqualTo("MyNamespace4"));
			Assert.That(ns.Name, Is.EqualTo("MyNamespace4"));
			xPlatformAssert(ns.Offset, 248, 277);
			Assert.That(ns.Line, Is.EqualTo(30));
			Assert.That(ns.Column, Is.EqualTo(4));
		}
		
		[Test]
		public void Should_find_bizarro_classes()
		{
			var cls = _cache.Classes.Where(x => x.Name.Equals("MyClass4")).FirstOrDefault();
			Assert.That(cls.Fullpath, Is.EqualTo("file1"));
			Assert.That(cls.Signature, Is.EqualTo("MyNamespace4.MyClass4"));
			Assert.That(cls.Namespace, Is.EqualTo("MyNamespace4"));
			Assert.That(cls.Name, Is.EqualTo("MyClass4"));
			xPlatformAssert(cls.Offset, 294, 332);
			Assert.That(cls.Line, Is.EqualTo(39));
			Assert.That(cls.Column, Is.EqualTo(5));
		}
		
		[Test]
		public void Should_find_struct()
		{
			var str = _cache.Structs.Where(x => x.Name.Equals("MyStruct1")).FirstOrDefault();
			Assert.That(str.Fullpath, Is.EqualTo("file1"));
			Assert.That(str.Signature, Is.EqualTo("MyNamespace5.MyStruct1"));
			Assert.That(str.Namespace, Is.EqualTo("MyNamespace5"));
			Assert.That(str.Name, Is.EqualTo("MyStruct1"));
			xPlatformAssert(str.Offset, 349, 393);
			Assert.That(str.Line, Is.EqualTo(45));
			Assert.That(str.Column, Is.EqualTo(8));
		}
		
		[Test]
		public void Should_find_enum()
		{
			var str = _cache.Enums.Where(x => x.Name.Equals("MyEnum1")).FirstOrDefault();
			Assert.That(str.Fullpath, Is.EqualTo("file1"));
			Assert.That(str.Signature, Is.EqualTo("MyNamespace5.MyEnum1"));
			Assert.That(str.Namespace, Is.EqualTo("MyNamespace5"));
			Assert.That(str.Name, Is.EqualTo("MyEnum1"));
			xPlatformAssert(str.Offset, 373, 421);
			Assert.That(str.Line, Is.EqualTo(49));
			Assert.That(str.Column, Is.EqualTo(6));
		}
		
		[Test]
		public void Should_find_interface()
		{
			var iface = _cache.Interfaces.Where(x => x.Name.Equals("MyInterface1")).FirstOrDefault();
			Assert.That(iface.Fullpath, Is.EqualTo("file1"));
			Assert.That(iface.Signature, Is.EqualTo("MyNamespace5.MyInterface1"));
			Assert.That(iface.Namespace, Is.EqualTo("MyNamespace5"));
			Assert.That(iface.Name, Is.EqualTo("MyInterface1"));
			xPlatformAssert(iface.Offset, 416, 469);
			Assert.That(iface.Line, Is.EqualTo(54));
			Assert.That(iface.Column, Is.EqualTo(11));
		}
		
		[Test]
		public void File_test()
		{
			/*var file = "/home/ack/src/mono/mcs/class/System.XML/Test/System.Xml/nist_dom/fundamental/Element/Element.cs";
			var cache = new Fake_CacheBuilder();
			var parser = new CSharpFileParser(cache);
			
			try
			{
				parser.ParseFile("file1", () => { return File.ReadAllText(file); });
			}
			catch
			{
				throw new Exception(string.Format("{0}, {1}, {2}", cache.Namespaces.Count, cache.Classes.Count, cache.Interfaces.Count));
			}
			Assert.Fail();*/
		}
		
		private string getContent()
		{
			return File.ReadAllText(
				Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestResources"),
				"DefaultCSharp.txt"));
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

