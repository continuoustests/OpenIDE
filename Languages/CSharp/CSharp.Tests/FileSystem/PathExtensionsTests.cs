using System;
using NUnit.Framework;
using CSharp.FileSystem;
using System.Reflection;
using System.IO;
namespace CSharp.Tests.FileSystem
{
	[TestFixture]
	public class PathExtensionsTests
	{
		[Test]
		public void Should_return_argument_path_when_completely_different()
		{
			Assert.That(() => PathExtensions.GetRelativePath("one", "andtheother"), Is.EqualTo("andtheother"));
		}
		
		[Test]
		public void Should_return_relative_path_when_it_contains_template_path()
		{
			var localAssembly = Path.GetFullPath(Assembly.GetExecutingAssembly().FullName);
			Assert.That(() => PathExtensions.GetRelativePath(localAssembly, Path.GetFullPath(Path.Combine("someotherfolder", "theotherfile.mms"))), Is.EqualTo(Path.Combine("someotherfolder", "theotherfile.mms")));
		}
		
		[Test]
		public void Should_return_relative_path_when_template_path_is_directory()
		{
			var directory = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().FullName));
			Assert.That(() => PathExtensions.GetRelativePath(directory, Path.GetFullPath(Path.Combine("someotherfolder", "theotherfile.mms"))), Is.EqualTo(Path.Combine("someotherfolder", "theotherfile.mms")));
		}
		
		[Test]
		public void Should_return_doted_path_when_argument_is_levels_behind_template()
		{
			var directory = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().FullName));
			var argument = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetFullPath("somefile.txt")))), "Somefile.txt");
			Assert.That(() => PathExtensions.GetRelativePath(directory, argument), Is.EqualTo(Path.Combine("..", Path.Combine("..", "Somefile.txt"))));
		}
		
		[Test]
		public void Should_return_argument_when_argument_is_equal_to_template()
		{
			var directory = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().FullName));
			var argument = directory;
			Assert.That(() => PathExtensions.GetRelativePath(directory, argument), Is.EqualTo(argument));
		}
		
		[Test]
		public void Should_return_argument_when_argument_is_equal_to_template_directory()
		{
			var directory = Path.GetFullPath(Assembly.GetExecutingAssembly().FullName);
			var argument = Path.GetDirectoryName(directory);
			Assert.That(() => PathExtensions.GetRelativePath(directory, argument), Is.EqualTo(argument));
		}
		
		[Test]
		public void Should_return_argument_when_argument_is_equal_to_template_directory_with_directory_separator()
		{
			var directory = Path.GetFullPath(Assembly.GetExecutingAssembly().FullName);
			var argument = Path.GetDirectoryName(directory) + Path.DirectorySeparatorChar;
			Assert.That(() => PathExtensions.GetRelativePath(directory, argument), Is.EqualTo(argument));
		}
		
		[Test]
		public void Should_return_relative_path_when_argument_is_a_directory_below_template()
		{
			var directory = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().FullName));
			var argument = Path.GetDirectoryName(directory);
			Assert.That(() => PathExtensions.GetRelativePath(directory, argument), Is.EqualTo(".."));
		}
		
		[Test]
		public void Should_return_relative_path_when_argument_and_template_are_already_relative()
		{
			var directory = "SomProject.csproj";
			var argument = "Somefile.cs";
			Assert.That(() => PathExtensions.GetRelativePath(directory, argument), Is.EqualTo("Somefile.cs"));
		}
		
		[Test]
		public void Should_return_environment_spesific_paths()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
				Assert.That(() => PathExtensions.AdjustToEnvironment("/Some/file"), Is.EqualTo("/Some/file"));
			else
				Assert.That(() => PathExtensions.AdjustToEnvironment("/Some/file"), Is.EqualTo("/some/file"));
		}
	}
}

