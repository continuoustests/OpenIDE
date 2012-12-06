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
			var cache = new OutputWriter(new NullResponseWriter());
            cache.WriteMethod(
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
						}).SetEndPosition(17,3));
            cache.WriteVariable(
				new Variable(
					new FileRef(_file, null),
					"MyNS.MyClass.MyMethod",
					"str",
					"local",
					10,
					4,
					"System.String"));
            _analyzer = new FileContextAnalyzer(
                new OutputWriter(new NullResponseWriter()),
                cache);
		}

		[Test]
		public void Can_get_signatur_from_method_variable()
		{
			var signature = _analyzer.GetSignatureFromTypeAndPosition(_file, "str", 13, 1);
			Assert.That(signature, Is.EqualTo("void MyNS.MyClass.MyMethod(System.String).str"));
		}
	}
}