using System;
using NUnit.Framework;
using CSharp.Crawlers;
using CSharp.Projects;
using CSharp.Responses;
using CSharp.Crawlers.TypeResolvers;

namespace CSharp.Tests.Crawlers.TypeResolvers
{
	[TestFixture]
	public class FileContextAnalyzerTests
	{
		private string _file = "file";
		private FileContextAnalyzer _analyzer;

		[SetUp]
		public void Setup()
		{
			var globalCache = new OutputWriter(new NullResponseWriter());
			globalCache.WriteMethod(
				new Method(
					new FileRef(_file, null),
					"MyNS.MyClass",
					"MyMethod",
					"private",
					9,
					2,
					"void",
					new[] { 
							new Parameter(
								new FileRef("file", null),
								"MyNS.MyClass.MyMethod",
								"word",
								"parameter",
								9,
								12,
								"System.String")
						}));
			_analyzer = new FileContextAnalyzer(null, null);
		}

		[Test]
		public void Can_get_signatur_from_method_variable()
		{
			var signature = _analyzer.GetSignatureFromTypeAndPosition(_file, "str", 13, 1);
			Assert.That(signature, Is.EqualTo("MyNS.MyClass.MyMethod(System.String).str"));
		}


	}
}