using System;
using NUnit.Framework;
using OpenIDENet.Tests.Messaging;
using OpenIDENet.Projects.Appenders;
using OpenIDENet.Projects;
using OpenIDENet.Messaging.Messages;
namespace OpenIDENet.Tests.Projects.Appenders
{
	[TestFixture]
	public class VS2010FileAppenderTests
	{
		[Test]
		public void Should_publish_failure_on_invalid_xml()
		{
			var bus = new Fake_MessageBus();
			var appender = new VS2010FileAppender(bus);
			var project = new Project("", "");
			appender.Append(project, "");
			
			bus.Published<FailMessage>();
		}
	}
}

