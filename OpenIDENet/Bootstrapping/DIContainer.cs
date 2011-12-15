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
using OpenIDENet.Files;
using OpenIDENet.Projects;
using OpenIDENet.Arguments;
using OpenIDENet.Arguments.Handlers;
using OpenIDENet.Projects.Removers;
using OpenIDENet.Projects.Referencers;
using OpenIDENet.EditorEngineIntegration;
using OpenIDENet.CodeEngineIntegration;

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
			_container
					  .Register(Component.For<ICommandHandler>().ImplementedBy<EditorHandler>())
					  .Register(Component.For<ICommandHandler>().ImplementedBy<CodeEngineGoToHandler>())
					  .Register(Component.For<ICommandHandler>().ImplementedBy<CodeEngineExploreHandler>())
					  .Register(Component.For<ICommandHandler>().ImplementedBy<CreateHandler>())
					  .Register(Component.For<ICommandHandler>().ImplementedBy<NewHandler>())
					  .Register(Component.For<ICommandHandler>().ImplementedBy<AddFileHandler>())
					  .Register(Component.For<ICommandHandler>().ImplementedBy<RemoveFileHandler>())
					  .Register(Component.For<ICommandHandler>().ImplementedBy<DeleteFileHandler>())
					  .Register(Component.For<ICommandHandler>().ImplementedBy<ReferenceHandler>())
					  .Register(Component.For<ICommandHandler>().ImplementedBy<DereferenceHandler>())
					  .Register(Component.For<ICommandHandler>().ImplementedBy<RunCommandHandler>())

					  .Register(Component.For<IFS>().ImplementedBy<FS>())
					  .Register(Component.For<IMessageBus>().ImplementedBy<MessageBus>())
					  .Register(Component.For<ILocateClosestProject>().ImplementedBy<ProjectLocator>())
					  .Register(Component.For<IResolveProjectVersion>().ImplementedBy<ProjectVersionResolver>())
					  
					  .Register(Component.For<IReadProjectsFor>().ImplementedBy<DefaultReader>())
					  .Register(Component.For<IResolveFileTypes>().ImplementedBy<VSFileTypeResolver>())
					  .Register(Component.For<IAppendFiles>().ImplementedBy<VSFileAppender>())
					  .Register(Component.For<IRemoveFiles>().ImplementedBy<DefaultRemover>())
					  .Register(Component.For<IAddReference>()
					  					 .Forward<IRemoveReference>().ImplementedBy<AssemblyReferencer>())
					  .Register(Component.For<IAddReference>()
					  					 .Forward<IRemoveReference>().ImplementedBy<ProjectReferencer>())
					  .Register(Component.For<IWriteProjectFileToDiskFor>().ImplementedBy<DefaultWriter>())
					
					  .Register(Component.For<IProvideVersionedTypes>().ImplementedBy<VersionedTypeProvider<VS2010>>())
			
					  .Register(Component.For<ILocateEditorEngine>().ImplementedBy<EngineLocator>())
					  .Register(Component.For<ICodeEngineLocator>().ImplementedBy<CodeEngineDispatcher>());
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

