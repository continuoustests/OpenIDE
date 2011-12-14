using System;
using NUnit.Framework;
using OpenIDENet.Arguments;
using OpenIDENet.Versioning;
using OpenIDENet.Projects;
using Rhino.Mocks;
using OpenIDENet.Languages;
namespace OpenIDENet.Tests
{
	[TestFixture]
	public class CommandDispatcherTests
	{
		private FakeHandler _handler;
		private CommandDispatcher _execute;
		private ILocateClosestProject _projectLocator;
		private IResolveProjectVersion _versionResolver;
		
		[SetUp]
		public void Setup()
		{
			_projectLocator = MockRepository.GenerateMock<ILocateClosestProject>();
			_versionResolver = MockRepository.GenerateMock<IResolveProjectVersion>();
			
			_projectLocator.Stub(x => x.Locate(null)).IgnoreArguments().Return("file");
			_versionResolver.Stub(x => x.ResolveFor("file")).Return(MockRepository.GenerateMock<IProvideVersionedTypes>());
			
			_handler = new FakeHandler();
			_execute = new CommandDispatcher(new ICommandHandler[] { _handler }, _projectLocator, _versionResolver);
		}
		
		[Test]
		public void Should_find_named_handler()
		{
			_execute.For("MyCommand", new string[] {});
			_handler.WasCalled();
		}
		
		[Test]
		public void When_no_matching_handlers_it_should_return_null()
		{
			_execute.For("MyOtherCommand", new string[] {});
			_handler.WasNotCalled();
		}
	}
	
	class FakeHandler : ICommandHandler
	{
		private bool _wasExecuted = false;
		
		public CommandHandlerParameter Usage {
			get {
				return new CommandHandlerParameter(SupportedLanguage.CSharp, CommandType.FileCommand, "", "");
			}
		}

		public string Command { get { return "MyCommand"; } }
		
		public void Execute(string[] arguments, Func<string, ProviderSettings> with)
		{
			_wasExecuted = true;
		}
		
		public void WasCalled()
		{
			Assert.That(_wasExecuted, Is.True);
		}
		
		public void WasNotCalled()
		{
			Assert.That(_wasExecuted, Is.False);
		}
	}
}

