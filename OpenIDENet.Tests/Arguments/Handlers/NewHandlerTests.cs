using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using OpenIDENet.Files;
using OpenIDENet.Projects;
using OpenIDENet.Versioning;
using OpenIDENet.Arguments;
using OpenIDENet.Arguments.Handlers;
using OpenIDENet.EditorEngineIntegration;
using OpenIDENet.Languages;

namespace OpenIDENet.Tests.Arguments.Handlers
{
	[TestFixture]
	public class NewHandlerTests
	{
		private NewHandler _newHandler;
		private IResolveFileTypes _resolver;
		private ILocateEditorEngine _editorFactory;
		private IProjectHandler _projectHandler;
		private INewTemplate _template;
		
		[SetUp]
		public void Setup()
		{
			_resolver = MockRepository.GenerateMock<IResolveFileTypes>();
			_editorFactory = MockRepository.GenerateMock<ILocateEditorEngine>();
			_projectHandler = MockRepository.GenerateMock<IProjectHandler>();
			_newHandler = new NewHandler(_resolver, _editorFactory);
			_template = MockRepository.GenerateMock<INewTemplate>();

			_newHandler.OverrideProjectHandler(_projectHandler);
			_newHandler.OverrideTemplatePicker(
				(s, type) => { return _template; });

			_projectHandler.Stub(x => x.Read(null, null)).IgnoreArguments().Return(true);
			_projectHandler.Stub(x => x.Fullpath).Return("/its/a/project.csproj");
			_template.Stub(x => x.File).Return(MockRepository.GenerateMock<IFile>());
			
		}
		
		[Test]
		public void When_asked_to_run_a_template_it_will_run_a_template()
		{
			_newHandler.Execute(new string[] { "class", "somefile" }, null);
			_template.AssertWasCalled(
				x => x.Run("", "", "", "", SupportedLanguage.CSharp, null),
				x => x.IgnoreArguments());
		}

		[Test]
		public void When_given_a_path_that_doesnt_exist_it_should_create_the_folders_nessecary()
		{
			var path = Path.Combine(
							Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
							Path.Combine("somedirectory", "MoreDirectories"));
			if (Directory.Exists(path))
				Directory.Delete(path, true);
			var file = Path.Combine(path, "somefile");

			_newHandler.Execute(new string[] { "class", file }, null);
			
			Assert.That(Directory.Exists(path), Is.True);
		}
	}
}
