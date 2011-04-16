using System;
using NUnit.Framework;
using OpenIDENet.Projects.Removers;
using OpenIDENet.Tests.Messaging;
using OpenIDENet.Messaging.Messages;
using OpenIDENet.Files;
using OpenIDENet.Projects;
using System.IO;
using OpenIDENet.FileSystem;
using Rhino.Mocks;
namespace OpenIDENet.Tests.Projects.Removers
{
	[TestFixture]
	public class DefaultRemoverTests
	{
		private Fake_MessageBus _bus;
		private DefaultRemover _remover;
		
		[SetUp]
		public void Setup()
		{
			var fs = MockRepository.GenerateMock<IFS>();
			fs.Stub(f => f.FileExists("")).IgnoreArguments().Return(true);
			_bus = new Fake_MessageBus();
			_remover = new DefaultRemover(_bus, fs);
		}
		
		[Test]
		public void Should_publish_failure_on_invalid_xml()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "");
			_remover.Remove(project, new CompileFile("somefile.cs"));
			
			_bus.Published<FailMessage>();
		}
		
		[Test]
		public void Should_remove_file_from_xml()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>");
			_remover.Remove(project, new CompileFile("somefile.cs"));
			
			Assert.That(project.Content, Is.EqualTo("<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup></ItemGroup></Project>"));
		}
		
		private Project getProject(string file, string content)
		{
			return new Project(file, content, new ProjectSettings(ProjectType.CSharp, ""));
		}
	}
}

