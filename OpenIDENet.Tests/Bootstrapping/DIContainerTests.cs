using System;
using NUnit.Framework;
using OpenIDENet.Bootstrapping;
using OpenIDENet.FileSystem;
using OpenIDENet.Messaging;
using OpenIDENet.Versioning;
using OpenIDENet.Projects.Readers;
using OpenIDENet.Projects.Appenders;
using OpenIDENet.Projects.Writers;
using OpenIDENet.Files;
using OpenIDENet.Projects;
using OpenIDENet.Arguments;
using OpenIDENet.Projects.Removers;
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
			container.Configure();
			
			Assert.That(container.ResolveAll<ICommandHandler>().Length, Is.EqualTo(10));
			
			Assert.That(container.Resolve<IFS>(), Is.InstanceOf<IFS>());
			Assert.That(container.Resolve<IMessageBus>(), Is.InstanceOf<IMessageBus>());
			Assert.That(container.Resolve<ILocateClosestProject>(), Is.InstanceOf<ILocateClosestProject>());
			Assert.That(container.Resolve<IResolveProjectVersion>(), Is.InstanceOf<IResolveProjectVersion>());
			Assert.That(container.Resolve<ILocateEditorEngine>(), Is.InstanceOf<ILocateEditorEngine>());
			Assert.That(container.Resolve<ICodeEngineLocator>(), Is.InstanceOf<ICodeEngineLocator>());
			
			Assert.That(container.ResolveAll<IProvideVersionedTypes>().Length, Is.EqualTo(1));
			
			Assert.That(container.ResolveAll<IReadProjectsFor>().Length, Is.EqualTo(1));
			Assert.That(container.ResolveAll<IResolveFileTypes>().Length, Is.EqualTo(1));
			Assert.That(container.ResolveAll<IAppendFiles>().Length, Is.EqualTo(1));
			Assert.That(container.ResolveAll<IRemoveFiles>().Length, Is.EqualTo(1));
			Assert.That(container.ResolveAll<IAddReference>().Length, Is.EqualTo(2));
			Assert.That(container.ResolveAll<IRemoveReference>().Length, Is.EqualTo(2));
			Assert.That(container.ResolveAll<IWriteProjectFileToDiskFor>().Length, Is.EqualTo(1));
		}
	}
}

