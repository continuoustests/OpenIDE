using System;
namespace OpenIDENet.Arguments.Handlers
{
	class RemoveFileHandler : ICommandHandler
	{
		public string Command { get { return "removefile"; } }
		
		public void Execute(string[] arguments, Func<string, ProviderSettings> getTypesProviderByLocation)
		{
			var provider = getTypesProviderByLocation(arguments[0]);
			if (provider == null)
				return;
			var with = provider.TypesProvider;
			var file = with.FileTypeResolver().Resolve(arguments[0]);
			if (file == null)
				return;
			var project = with.Reader().Read(provider.ProjectFile);
			with.FileRemoverFor(file).Remove(project, file);
			with.Writer().Write(project);
		}
	}
}

