using System;
using NUnit.Framework;
using OpenIDE.CodeEngine.Core.FileSystem;
namespace OpenIDE.CodeEngine.Core.Tests.FileSystem
{
	[TestFixture]
	public class PathParserTests
	{
		[Test]
		public void When_no_relative_path_it_should_return_passed_path()
		{
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
			    Assert.That(new PathParser(".ignorefile").ToAbsolute("/some/reference/path"), Is.EqualTo("/some/reference/path/.ignorefile"));
            else
                Assert.That(new PathParser(".ignorefile").ToAbsolute("C:\\some\\reference\\path"), Is.EqualTo("C:\\some\\reference\\path\\.ignorefile"));
		}
		
		[Test]
		public void When_relative_path_it_should_return_path_relative_to_passed_path()
		{
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
			    Assert.That(new PathParser("../.ignorefile").ToAbsolute("/some/reference/path"), Is.EqualTo("/some/reference/.ignorefile"));
            else
                Assert.That(new PathParser("..\\.ignorefile").ToAbsolute("C:\\some\\reference\\path"), Is.EqualTo("C:\\some\\reference\\.ignorefile"));
		}
	}
}

