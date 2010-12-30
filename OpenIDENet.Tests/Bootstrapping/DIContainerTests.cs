using System;
using NUnit.Framework;
using OpenIDENet.Bootstrapping;
using OpenIDENet.FileSystem;
using OpenIDENet.Messaging;
using OpenIDENet.Versioning;
using OpenIDENet.Projects.Readers;
using OpenIDENet.Projects.Parsers;
using OpenIDENet.Projects.Appenders;
using OpenIDENet.Projects.Writers;

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
			
			Assert.That(container.Resolve<IFS>(), Is.InstanceOf<IFS>());
			Assert.That(container.Resolve<IMessageBus>(), Is.InstanceOf<IMessageBus>());
			Assert.That(container.Resolve<IResolveProjectVersion>(), Is.InstanceOf<IResolveProjectVersion>());
			
			Assert.That(container.ResolveAll<IProvideVersionedTypes>().Length, Is.EqualTo(1));
			
			Assert.That(container.ResolveAll<IReadProjectsFor>().Length, Is.EqualTo(1));
			Assert.That(container.ResolveAll<IAppendCompiledFilesFor>().Length, Is.EqualTo(1));
			Assert.That(container.ResolveAll<IWriteProjectFileToDiskFor>().Length, Is.EqualTo(1));
		}
	}
}

