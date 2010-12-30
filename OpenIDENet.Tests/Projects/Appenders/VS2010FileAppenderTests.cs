using System;
using NUnit.Framework;
using OpenIDENet.Tests.Messaging;
using OpenIDENet.Projects.Appenders;
using OpenIDENet.Projects;
using OpenIDENet.Messaging.Messages;
using System.IO;
using Rhino.Mocks;
using OpenIDENet.FileSystem;
namespace OpenIDENet.Tests.Projects.Appenders
{
	[TestFixture]
	public class VS2010FileAppenderTests
	{
		private Fake_MessageBus _bus;
		private VS2010FileAppender _appender;
		
		[SetUp]
		public void Setup()
		{
			var fs = MockRepository.GenerateMock<IFS>();
			fs.Stub(f => f.FileExists("")).IgnoreArguments().Return(true);
			_bus = new Fake_MessageBus();
			_appender = new VS2010FileAppender(_bus, fs);
		}
		
		[Test]
		public void Should_publish_failure_on_invalid_xml()
		{
			var project = new Project(Path.GetFullPath("someproject.csproj"), "");
			_appender.Append(project, "somefile.cs");
			
			_bus.Published<FailMessage>();
		}
		
		[Test]
		public void Should_publish_failure_on_unrecognized_xml_format()
		{
			var project = new Project(Path.GetFullPath("someproject.csproj"), "<someelement></someelement>");
			_appender.Append(project, "somefile.cs");
			
			_bus.Published<FailMessage>();
		}
		
		[Test]
		public void Should_publish_faliure_when_file_does_not_exist()
		{
			var fs = MockRepository.GenerateMock<IFS>();
			var bus = new Fake_MessageBus();
			var appender = new VS2010FileAppender(bus, fs);
			var project = new Project(Path.GetFullPath("someproject.csproj"), "<Project><ItemGroup><Compile Include=\"BuildRunners\\MSBuildOutputParser.cs\" /></ItemGroup></Project>");
			appender.Append(project, "somefile.cs");
			
			bus.Published<FailMessage>();
		}
		
		[Test]
		public void Should_use_existing_item_group()
		{
			var project = new Project(Path.GetFullPath("someproject.csproj"), "<Project><ItemGroup><Compile Include=\"BuildRunners\\MSBuildOutputParser.cs\" /></ItemGroup></Project>");
			_appender.Append(project, "somefile.cs");
			Assert.AreEqual("<Project><ItemGroup><Compile Include=\"BuildRunners\\MSBuildOutputParser.cs\" /><Compile Include=\"somefile.cs\" /></ItemGroup></Project>", project.Content.ToString());
		}
		
		[Test]
		public void Should_create_new_group_if_a_different_type_of_item_group_is_existing()
		{
			var project = new Project(Path.GetFullPath("someproject.csproj"), "<Project><ItemGroup><Folder Include=\"Projects\\\" /></ItemGroup></Project>");
			_appender.Append(project, "somefile.cs");
			Assert.AreEqual("<Project><ItemGroup><Folder Include=\"Projects\\\" /></ItemGroup><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>", project.Content.ToString());
		}
		
		[Test]
		public void Should_create_new_if_no_item_group_exists()
		{
			var project = new Project(Path.GetFullPath("someproject.csproj"), "<Project></Project>");
			_appender.Append(project, "somefile.cs");
			Assert.AreEqual("<Project><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>", project.Content.ToString());
		}
		
		[Test]
		public void Should_preserve_heading()
		{
			var project = new Project(Path.GetFullPath("someproject.csproj"), "<?xml version=\"1.0\" encoding=\"utf-8\"?><Project></Project>");
			_appender.Append(project, "somefile.cs");
			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-8\"?><Project><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>", project.Content.ToString());
		}
		
		[Test]
		public void Should_not_add_file_twice()
		{
			var project = new Project(Path.GetFullPath("someproject.csproj"), "<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>");
			_appender.Append(project, "somefile.cs");
			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>", project.Content.ToString());
		}
		
		[Test]
		public void Should_handle_namespace()
		{
			var project = new Project(Path.GetFullPath("someproject.csproj"), "<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"></Project>");
			_appender.Append(project, "somefile.cs");
			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>", project.Content.ToString());
		}
		
		[Test]
		public void Should_handle_namespace_on_existing_items()
		{
			var project = new Project(Path.GetFullPath("someproject.csproj"), "<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>");
			_appender.Append(project, "someotherfile.cs");
			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup><Compile Include=\"somefile.cs\" /><Compile Include=\"someotherfile.cs\" /></ItemGroup></Project>", project.Content.ToString());
		}
		
		[Test]
		public void Should_use_relative_path_for_file()
		{
			var project = new Project(Path.GetFullPath("someproject.csproj"), "<Project></Project>");
			_appender.Append(project, Path.GetFullPath(Path.Combine("somesubdir", "somefile.cs")));
			Assert.AreEqual(string.Format("<Project><ItemGroup><Compile Include=\"{0}\" /></ItemGroup></Project>", Path.Combine("somesubdir", "somefile.cs")), project.Content.ToString());
		}
	}
}

