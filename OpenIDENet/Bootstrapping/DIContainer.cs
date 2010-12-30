using System;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using OpenIDENet.FileSystem;
using OpenIDENet.Messaging;
using OpenIDENet.Projects.Appenders;
using OpenIDENet.Projects.Readers;
using OpenIDENet.Projects.Writers;
using OpenIDENet.Versioning;
namespace OpenIDENet.Bootstrapping
{
	public class DIContainer
	{
		private WindsorContainer _container;
		
		public DIContainer()
		{
			_container = new WindsorContainer();
		}
		
		public void Configure()
		{
			_container.Kernel.Resolver.AddSubResolver(new ArrayResolver(_container.Kernel));
			_container.Register(Component.For<IFS>().ImplementedBy<FS>())
					  .Register(Component.For<IMessageBus>().ImplementedBy<MessageBus>())
					  .Register(Component.For<IResolveProjectVersion>().ImplementedBy<ProjectVersionResolver>())
					  
					  .Register(Component.For<IReadProjectsFor>().ImplementedBy<DefaultReader>())
					  .Register(Component.For<IAppendCompiledFilesFor>().ImplementedBy<VS2010FileAppender>())
					  .Register(Component.For<IWriteProjectFileToDiskFor>().ImplementedBy<DefaultWriter>())
					
					  .Register(Component.For<IProvideVersionedTypes>().ImplementedBy<VersionedTypeProvider<VS2010>>());
		}
		
		public T Resolve<T>()
		{
			return _container.Resolve<T>();
		}
		
		public T[] ResolveAll<T>()
		{
			return _container.ResolveAll<T>();
		}
	}
}

