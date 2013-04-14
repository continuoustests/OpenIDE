using System;
using CSharp.Responses;
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
		private OutputWriter _cache;
		
		[SetUp]
		public void Setup()
		{
			_cache = new OutputWriter(new NullResponseWriter());
            _parser = new NRefactoryParser()
			//_parser = new CSharpFileParser()
				.SetOutputWriter(_cache);
			_parser.ParseFile(new FileRef("file1", null), () => { return getContent(); });
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
}

