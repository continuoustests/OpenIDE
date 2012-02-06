using System;
using NUnit.Framework;
using CSharp.Crawlers;
using System.IO;

namespace CSharp.Tests.Crawlers
{
	[TestFixture]
	public class SolutionReaderTests
	{
		[Test]
		public void When_file_does_not_exist_it_should_return_empty_list()
		{
			var projects = new SolutionReader("NonExisting.sln").ReadProjects();
			Assert.That(projects.Count, Is.EqualTo(0));
		}
		
		[Test]
		public void Should_read_projects()
		{
			var projects = new SolutionReader(Path.Combine("TestResources", "VSSolutionFile.sln")).ReadProjects();
			Assert.That(projects.Count, Is.EqualTo(3));
			Assert.That(projects[0].File, Is.EqualTo(Path.GetFullPath(Path.Combine(Path.Combine("TestResources", "OpenIDE"), "OpenIDE.csproj"))));
			Assert.That(projects[1].File, Is.EqualTo(Path.GetFullPath(Path.Combine(Path.Combine("TestResources", "OpenIDE.Tests"), "OpenIDE.Tests.csproj"))));
			Assert.That(projects[2].File, Is.EqualTo(Path.GetFullPath(Path.Combine(Path.Combine("TestResources", "oi"), "oi.csproj"))));
		}
	}
}

