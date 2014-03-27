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

        [Test]
        [TestCase("/some/path/file1.txt", "/some/path/file2.txt", "file1.txt")]
        [TestCase("", "/some/path/file2.txt", "")]
        [TestCase("/some/path/file1.txt", "", "/some/path/file1.txt")]
        [TestCase("D:/some/path/file1.txt", "C:/some/path/file2.txt", "D:/some/path/file1.txt")]
        [TestCase("/some/file1.txt", "/some/path/file2.txt", "../file1.txt")]
        [TestCase("/file1.txt", "/some/path/file2.txt", "../../file1.txt")]
        [TestCase("/some/path/file1.txt", "/some/file2.txt", "path/file1.txt")]
        [TestCase("/some/another/path/file1.txt", "/some/other/path/file2.txt", "../../another/path/file1.txt")]
        [TestCase("/some/another/path/file1.txt", "/some/other/file2.txt", "../another/path/file1.txt")]
        public void When_getting_a_path_at_the_same_level_it_will_return_name(string path, string rootPath, string result)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) {
                Assert.That(new PathParser(path).RelativeTo(rootPath),Is.EqualTo(result));
            }
        }
	}
}

