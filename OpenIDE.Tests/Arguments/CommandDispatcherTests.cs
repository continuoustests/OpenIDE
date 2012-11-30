using System;
using NUnit.Framework;
using OpenIDE.Arguments;
using Rhino.Mocks;
using OpenIDE.Core.Language;
using OpenIDE.EventIntegration;
namespace OpenIDE.Tests
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
			_execute = new CommandDispatcher(
				new ICommandHandler[] { _handler },
				() => { return new ICommandHandler[] {}; },
				new EventDispatcher(""));
		}
		
		[Test]
		public void Should_find_named_handler()
		{
			var calledNoCommandDelegate = false;
			_execute.For("MyCommand", new string[] {}, (m) => calledNoCommandDelegate = true);
			_handler.WasCalled();
			Assert.That(calledNoCommandDelegate, Is.False);
		}
		
		[Test]
		public void When_no_matching_handlers_it_should_return_null()
		{
			var calledNoCommandDelegate = false;
			_execute.For("MyOtherCommand", new string[] {}, (m) => calledNoCommandDelegate = true);
			_handler.WasNotCalled();
			Assert.That(calledNoCommandDelegate, Is.True);
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

