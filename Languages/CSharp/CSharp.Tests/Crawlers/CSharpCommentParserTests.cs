using System;
using NUnit.Framework;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using CSharp.Crawlers;
using CSharp.Projects;

namespace CSharp.Tests.Crawlers
{
	[TestFixture]
	public class CSharpCommentParserTests
	{
		private ICSharpParser _parser;
		private Fake_CacheBuilder _cache;
		
		[SetUp]
		public void Setup()
		{
			_cache = new Fake_CacheBuilder();
			_parser = new CSharpFileParser()
				.SetOutputWriter(_cache);
			_parser.ParseFile("file1", () => { return getContent(); });
		}
		
		[Test]
		public void Should_not_parse_content_of_multiline_comments()
		{
			Assert.That(_cache.Classes.FirstOrDefault(x => x.Name.Equals("CSharpComments")), Is.Null);
		}
		
		[Test]
		public void Should_look_behind_comments()
		{
			Assert.That(_cache.Classes.FirstOrDefault(x => x.Name.Equals("InComment")), Is.Null);
			Assert.That(_cache.Classes.FirstOrDefault(x => x.Name.Equals("BehindComment")), Is.Not.Null);
		}
		
		[Test]
		public void Should_look_in_front_of_comments()
		{
			Assert.That(_cache.Classes.FirstOrDefault(x => x.Name.Equals("InFronOfCOmment")), Is.Not.Null);
			Assert.That(_cache.Classes.FirstOrDefault(x => x.Name.Equals("ClassBehind")), Is.Null);
		}
		
		private string getContent()
		{
			return File.ReadAllText(Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestResources"), "CSharpComments.txt"));
		}
	}

	class Fake_CacheBuilder : IOutputWriter 
	{
		public List<string> Files = new List<string>();
		public List<Namespace> Namespaces = new List<Namespace>();
		public List<Class> Classes = new List<Class>();
		public List<Interface> Interfaces = new List<Interface>();
		public List<Struct> Structs = new List<Struct>();
		public List<EnumType> Enums = new List<EnumType>();

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

		public void AddFile (string file)
		{
			Files.Add(file);
		}
		
		public void AddNamespace (Namespace ns)
		{
			Namespaces.Add(ns);
		}

		public void AddClass (Class cls)
		{
			Classes.Add(cls);
		}
		
		public void AddInterface(Interface iface)
		{
			Interfaces.Add(iface);
		}
		
		public void AddStruct(Struct str)
		{
			Structs.Add(str);
		}
		
		public void AddEnum(EnumType enu)
		{
			Enums.Add(enu);
		}

		public void Error(string description)
		{
		}
	}
}

