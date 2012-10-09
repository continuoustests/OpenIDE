using System;
using NUnit.Framework;
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
			_resolver = new TypeUnderPositionResolver();
		}
		
		[Test]
		public void Can_resolve_single_line_namespace_from_position()
		{
			var content = 
				"using System;" + Environment.NewLine +
				Environment.NewLine +
				"\tnamespace CSharp.Crawlers.TypeResolvers" + Environment.NewLine +
				"{";
			Assert.That(_resolver.GetTypeName("file", content, 3, 20), Is.EqualTo("CSharp.Crawlers.TypeResolvers"));
		}

		[Test]
		public void Can_resolve_multi_line_namespace_from_position()
		{
			Assert.Fail("Not implemented");
		}
	}
}