using System;
using CSharp.Responses;
using NUnit.Framework;
using CSharp.Crawlers;
using CSharp.Crawlers.TypeResolvers;

namespace CSharp.Tests.Crawlers.TypeResolvers
{
	[TestFixture]
	public class TypeUnderPositionResolverTests
	{
		private TypeUnderPositionResolver _resolver;
		
		[SetUp]
		public void Setup()
		{
			_resolver = 
				new TypeUnderPositionResolver();
		}
		
		[Test]
		public void Can_resolve_single_line_definition_from_position()
		{
			var content = 
				"using System;" + Environment.NewLine +
				Environment.NewLine +
				"\tnamespace CSharp.Crawlers.TypeResolvers" + Environment.NewLine +
				"{" + Environment.NewLine +
				"}";
			var ns = _resolver.GetTypeName("file", content, 3, 20);
			Assert.That(ns, Is.EqualTo("CSharp.Crawlers"));
		}

		[Test]
		public void Can_resolve_multi_line_definition_from_position()
		{
			var content = 
				"using System;" + Environment.NewLine +
				Environment.NewLine +
				"\tnamespace CSharp.Crawlers.TypeResolvers" + Environment.NewLine +
				"\t{" + Environment.NewLine +
				"\t\tpublic class MyClass" + Environment.NewLine +
				"\t\t{" + Environment.NewLine +
				"\t\t\t{" + Environment.NewLine +
				"\t\t\t\tpublic void " + Environment.NewLine +
				"\t\t\t\t\tMyMethod " + Environment.NewLine +
				"\t\t\t\t\t\t() " + Environment.NewLine +
				"\t\t\t\t{" + Environment.NewLine +
				"\t\t\t\t}" + Environment.NewLine +
				"\t\t\t}" + Environment.NewLine +
				"\t\t}" + Environment.NewLine +
				"\t}";
			var ns = _resolver.GetTypeName("file", content, 10, 7);
			Assert.That(ns, Is.EqualTo("MyMethod"));
		}

		[Test]
		public void Can_type_from_variable_definition()
		{
			var content = 
				"using System;" + Environment.NewLine +
				Environment.NewLine +
				"\tnamespace CSharp.Crawlers.TypeResolvers" + Environment.NewLine +
				"\t{" + Environment.NewLine +
				"\t\tpublic class MyClass" + Environment.NewLine +
				"\t\t{" + Environment.NewLine +
				"\t\t\t{" + Environment.NewLine +
				"\t\t\t\tpublic void " + Environment.NewLine +
				"\t\t\t\t\tMyMethod " + Environment.NewLine +
				"\t\t\t\t\t\t() " + Environment.NewLine +
				"\t\t\t\t{" + Environment.NewLine +
				"\t\t\t\t\tvar str = \"\";" + Environment.NewLine +
				"\t\t\t\t}" + Environment.NewLine +
				"\t\t\t}" + Environment.NewLine +
				"\t\t}" + Environment.NewLine +
				"\t}";
			var ns = _resolver.GetTypeName("file", content, 12, 12);
			Assert.That(ns, Is.EqualTo("str"));
		}
	}
}