using System;
using NUnit.Framework;
using CSharp.Projects.Readers;
using Rhino.Mocks;
using CSharp.FileSystem;
using System.Text;
using CSharp.Projects;
namespace CSharp.Tests.Projects.Readers
{
	[TestFixture]
	public class DefaultReaderTests
	{
		private Project _project;
		
		[SetUp]
		public void Setup()
		{
			var fs = MockRepository.GenerateMock<IFS>();
			fs.Stub(x => x.ReadFileAsText(null)).IgnoreArguments().Return(getProjectContent());
			_project = new DefaultReader(fs).Read("someproject.csproj");
		}
		
		[Test]
		public void Should_parse_project_type()
		{
			Assert.That(_project.Settings.Type, Is.EqualTo("C#"));
		}
		
		[Test]
		public void Should_parse_out_default_namespace()
		{
			Assert.That(_project.Settings.DefaultNamespace, Is.EqualTo("MyName.Space"));
		}
		
		private string getProjectContent()
		{
			var sb = new StringBuilder();
			sb.AppendLine("<Project>");
			sb.AppendLine("<PropertyGroup>");
			sb.AppendLine("<RootNamespace>MyName.Space</RootNamespace>");
			sb.AppendLine("</PropertyGroup>");
			sb.AppendLine("</Project>");
			return sb.ToString();
		}
	}
}

