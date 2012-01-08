using System;
using NUnit.Framework;
using CSharp.Files;
namespace CSharp.Tests.Files
{
	[TestFixture]
	public class ComplieFileTests
	{
		[Test]
		public void Should_know_default_file_type_for_csharp_projects()
		{
			Assert.That(CompileFile.DefaultExtensionFor("C#"), Is.EqualTo(".cs"));
		}
		
		[Test]
		public void Should_know_csharp_file_as_known_file_type()
		{
			Assert.That(CompileFile.SupportsExtension(".cs"), Is.True);
		}
	}
}

