using System;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using OpenIDENet.FileSystem;
using OpenIDENet.Messaging;
using OpenIDENet.Arguments;
using OpenIDENet.Arguments.Handlers;
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
					  .Register(Component.For<ICommandHandler>().ImplementedBy<RunCommandHandler>())

					  .Register(Component.For<IFS>().ImplementedBy<FS>())
					  .Register(Component.For<IMessageBus>().ImplementedBy<MessageBus>())
					  
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

