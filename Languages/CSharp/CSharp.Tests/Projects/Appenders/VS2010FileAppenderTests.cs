using System;
using NUnit.Framework;
using CSharp.Projects.Appenders;
using CSharp.Projects;
using System.IO;
using Rhino.Mocks;
using CSharp.FileSystem;
using CSharp.Files;
namespace CSharp.Tests.Projects.Appenders
{
	[TestFixture]
	public class VS2010FileAppenderTests
	{
		private VSFileAppender _appender;
		
		[SetUp]
		public void Setup()
		{
			var fs = MockRepository.GenerateMock<IFS>();
			fs.Stub(x => x.FileExists(new CompileFile("somefile.cs").Fullpath)).Return(true);
			fs.Stub(x => x.FileExists(new CompileFile("someotherfile.cs").Fullpath)).Return(true);
			fs.Stub(x => x.FileExists(new CompileFile(Path.GetFullPath(Path.Combine("somesubdir", "somefile.cs"))).Fullpath)).Return(true);
			_appender = new VSFileAppender(fs);
		}
		
		[Test]
		public void Should_publish_failure_on_invalid_xml()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "");
			_appender.Append(project, new CompileFile("somefile.cs"));
			
			Assert.That(project.Content, Is.EqualTo(""));
		}
		
		[Test]
		public void Should_publish_failure_on_unrecognized_xml_format()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "<someelement></someelement>");
			_appender.Append(project, new CompileFile("somefile.cs"));
			
			Assert.That(project.Content, Is.EqualTo("<someelement></someelement>"));
		}
		
		[Test]
		public void Should_publish_faliure_when_file_does_not_exist()
		{
			var fs = MockRepository.GenerateMock<IFS>();
			var appender = new VSFileAppender(fs);
			var project = getProject(Path.GetFullPath("someproject.csproj"), "<Project><ItemGroup><Compile Include=\"BuildRunners\\MSBuildOutputParser.cs\" /></ItemGroup></Project>");
			appender.Append(project, new CompileFile("somefile.cs"));
			
			Assert.That(project.Content, Is.EqualTo("<Project><ItemGroup><Compile Include=\"BuildRunners\\MSBuildOutputParser.cs\" /></ItemGroup></Project>"));
		}
		
		[Test]
		public void Should_use_existing_item_group()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "<Project><ItemGroup><Compile Include=\"BuildRunners\\MSBuildOutputParser.cs\" /></ItemGroup></Project>");
			_appender.Append(project, new CompileFile("somefile.cs"));
			Assert.AreEqual("<Project><ItemGroup><Compile Include=\"BuildRunners\\MSBuildOutputParser.cs\" /><Compile Include=\"somefile.cs\" /></ItemGroup></Project>", project.Content.ToString());
		}
		
		[Test]
		public void Should_create_new_group_if_a_different_type_of_item_group_is_existing()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "<Project><ItemGroup><Folder Include=\"Projects\\\" /></ItemGroup></Project>");
			_appender.Append(project, new CompileFile("somefile.cs"));
			Assert.AreEqual("<Project><ItemGroup><Folder Include=\"Projects\\\" /></ItemGroup><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>", project.Content.ToString());
		}
		
		[Test]
		public void Should_create_new_if_no_item_group_exists()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "<Project></Project>");
			_appender.Append(project, new CompileFile("somefile.cs"));
			Assert.AreEqual("<Project><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>", project.Content.ToString());
		}
		
		[Test]
		public void Should_preserve_heading()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "<?xml version=\"1.0\" encoding=\"utf-8\"?><Project></Project>");
			_appender.Append(project, new CompileFile("somefile.cs"));
			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-8\"?><Project><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>", project.Content.ToString());
		}
		
		[Test]
		public void Should_not_add_file_twice()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>");
			_appender.Append(project, new CompileFile("somefile.cs"));
			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>", project.Content.ToString());
		}
		
		[Test]
		public void Should_handle_namespace()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"></Project>");
			_appender.Append(project, new CompileFile("somefile.cs"));
			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>", project.Content.ToString());
		}
		
		[Test]
		public void Should_handle_namespace_on_existing_items()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup><Compile Include=\"somefile.cs\" /></ItemGroup></Project>");
			_appender.Append(project, new CompileFile("someotherfile.cs"));
			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-8\"?><Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup><Compile Include=\"somefile.cs\" /><Compile Include=\"someotherfile.cs\" /></ItemGroup></Project>", project.Content.ToString());
		}
		
		[Test]
		public void Should_use_relative_path_for_file()
		{
			var project = getProject(Path.GetFullPath("someproject.csproj"), "<Project></Project>");
			_appender.Append(project, new CompileFile(Path.GetFullPath(Path.Combine("somesubdir", "somefile.cs"))));
			Assert.AreEqual(string.Format("<Project><ItemGroup><Compile Include=\"{0}\" /></ItemGroup></Project>", "somesubdir\\somefile.cs"), project.Content.ToString());
		}
		
		private Project getProject(string file, string content)
		{
			return new Project(file, content, new ProjectSettings() { Type = "C#" });
		}
	}
}

