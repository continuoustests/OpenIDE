using System;
using NUnit.Framework;
using OpenIDENet.Arguments.Handlers;
using OpenIDENet.FileSystem;
using Rhino.Mocks;

namespace OpenIDENet.Tests.Arguments.Handlers
{
	[TestFixture]
	public class ReferenceHandlerTests
	{
		private ReferenceHandler _handler;
		
		[SetUp]
		public void Setup()
		{
			_handler = new ReferenceHandler();
		}
		
		[Test]
		public void Should_handle_reference_commands()
		{
			Assert.That(_handler.Command, Is.EqualTo("reference"));
		}
	}
}
