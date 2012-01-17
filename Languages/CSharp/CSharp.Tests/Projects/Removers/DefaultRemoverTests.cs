using System;
using NUnit.Framework;
using CSharp.Projects.Removers;
using CSharp.Files;
using CSharp.Projects;
using System.IO;
using CSharp.FileSystem;
using Rhino.Mocks;
namespace CSharp.Tests.Projects.Removers
{
	[TestFixture]
	public class DefaultRemoverTests
	{
		private DefaultRemover _remover;
		
		[SetUp]
		public void Setup()
		{
			var fs = MockRepository.GenerateMock<IFS>();
			fs.Stub(f => f.FileExists("")).IgnoreArguments().Return(true);
			_remover = new DefaultRemover(fs);
		}

		[Test]
		public void Should_support_compile_files()
		{
			Assert.That(_remover.SupportsFile(new CompileFile()), Is.True);
		}
		
		[Test]
		public void Should_support_none_files()
		{
			Assert.That(_remover.SupportsFile(new NoneFile()), Is.True);
		}

		[Test]
		public void Should_publish_failure_on_invalid_xml()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "");
			_remover.Remove(project, new CompileFile("somefile.cs"));
			
		}
		
		[Test]
		public void Should_remove_compile_file_from_xml()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>");
			_remover.Remove(project, new CompileFile("somefile.cs"));
			
			Assert.That(project.Content, Is.EqualTo("<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup></ItemGroup></Project>"));
		}
		
		[Test]
		public void Should_remove_none_file_from_xml()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup><None Include=\"somefile.txt\" /></ItemGroup></Project>");
			_remover.Remove(project, new CompileFile("somefile.txt"));
			
			Assert.That(project.Content, Is.EqualTo("<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup></ItemGroup></Project>"));
		}

		private Project getProject(string file, string content)
		{
			return new Project(file, content, new ProjectSettings() { Type = "C#" });
		}
	}
}

