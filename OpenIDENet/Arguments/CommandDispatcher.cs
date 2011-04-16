using System;
using System.Linq;
using OpenIDENet.Versioning;
using OpenIDENet.Projects;
using System.IO;

namespace OpenIDENet.Arguments
{
	class CommandDispatcher
	{
		private ICommandHandler[] _handlers;
		private ILocateClosestProject _projectResolver;
		private IResolveProjectVersion _versionResolver;
		
		public CommandDispatcher(ICommandHandler[] handlers, ILocateClosestProject projectResolver, IResolveProjectVersion versionResolver)
		{
			_handlers = handlers;
			_projectResolver = projectResolver;
			_versionResolver = versionResolver;
		}
		
		public void For(string name, string[] arguments)
		{
			var command = _handlers.FirstOrDefault(x => x.Command.Equals(name));
			if (command == null)
			{
				Console.WriteLine("Invalid arguments. Unsupported command {0}", name);
				return;
			}
			command.Execute(arguments, getTypesProvider);
		}
		
		private ProviderSettings getTypesProvider(string location)
		{
			location = Path.GetFullPath(location);
			if (File.Exists(location))
				location = Path.GetDirectoryName(location);
			var projectFile = _projectResolver.Locate(location);
			if (projectFile == null)
			{
				Console.WriteLine("Could not locate a project file for {0}", location);
				return null;
			}
			var project = _versionResolver.ResolveFor(projectFile);
			if (project == null)
				Console.WriteLine("Unsupported poject version for project {0}", projectFile);
			return new ProviderSettings(projectFile, project);
		}
	}
}

