using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using OpenIDENet.Files;
using OpenIDENet.Versioning;
using OpenIDENet.Arguments;
using OpenIDENet.Arguments.Handlers;
using OpenIDENet.EditorEngineIntegration;

namespace OpenIDENet.Tests.Arguments.Handlers
{
	[TestFixture]
	public class NewHandlerTests
	{
		private NewHandler _newHandler;
		private IResolveFileTypes _resolver;
		private ILocateEditorEngine _editorFactory;
		private IProvideVersionedTypes _typesProvider;
		
		[SetUp]
		public void Setup()
		{
			_resolver = MockRepository.GenerateMock<IResolveFileTypes>();
			_editorFactory = MockRepository.GenerateMock<ILocateEditorEngine>();
			_typesProvider = MockRepository.GenerateMock<IProvideVersionedTypes>();
			_newHandler = new NewHandler(_resolver, _editorFactory);
		}
		
		[Test]
		public void When_given_a_path_that_doesnt_exist_it_should_create_the_folders_nessecary()
		{
			/*var path = Path.Combine(
							Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
							"somedirectory");
			if (Directory.Exists(path))
				Directory.Delete(path);
			var file = Path.Combine(path, "somefile");
			
			_newHandler.Execute(
				new string[] { "class", file },
				(s) => new ProviderSettings("", _typesProvider));
			
			var exists = File.Exists(file);
			if (File.Exists(file))
				File.Delete(file);
			Assert.That(exists, Is.True);*/
		}
	}
}
