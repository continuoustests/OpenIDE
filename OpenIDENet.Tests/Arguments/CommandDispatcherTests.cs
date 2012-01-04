using System;
using NUnit.Framework;
using OpenIDENet.Arguments;
using Rhino.Mocks;
using OpenIDENet.Core.Language;
namespace OpenIDENet.Tests
{
	[TestFixture]
	public class CommandDispatcherTests
	{
		private FakeHandler _handler;
		private CommandDispatcher _execute;
		
		[SetUp]
		public void Setup()
		{
			_handler = new FakeHandler();
			_execute = new CommandDispatcher(new ICommandHandler[] { _handler });
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
				return new CommandHandlerParameter("C#", CommandType.FileCommand, "", "");
			}
		}

		public string Command { get { return "MyCommand"; } }
		
		public void Execute(string[] arguments)
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

