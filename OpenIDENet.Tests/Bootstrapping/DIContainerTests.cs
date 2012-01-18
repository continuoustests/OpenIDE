using System;
using System.Linq;
using NUnit.Framework;
using OpenIDENet.Bootstrapping;
using OpenIDENet.FileSystem;
using OpenIDENet.Messaging;
using OpenIDENet.Arguments;
using OpenIDENet.EditorEngineIntegration;
using OpenIDENet.CodeEngineIntegration;

namespace OpenIDENet.Tests
{
	[TestFixture]
	public class DIContainerTests
	{
		[Test]
		public void Should_resolve_types()
		{
			var container = new DIContainer();
			
			Assert.That(container.ICommandHandlers().Count(), Is.EqualTo(7));
			
			Assert.That(container.IFS(), Is.InstanceOf<IFS>());
			Assert.That(container.IMessageBus(), Is.InstanceOf<IMessageBus>());
			Assert.That(container.ILocateEditorEngine(), Is.InstanceOf<ILocateEditorEngine>());
			Assert.That(container.ICodeEngineLocator(), Is.InstanceOf<ICodeEngineLocator>());
		}
	}
}

