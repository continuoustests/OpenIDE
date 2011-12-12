using System;
using OpenIDENet.Versioning;
namespace OpenIDENet.Arguments.Handlers
{
	class AddFileHandler : ICommandHandler
	{
		public CommandHandlerParameters Usage {
			get {
				return new CommandHandlerParameters()
					.Add("addfile", "Adds a file to the closest project")
					.Add("FILE_TO_ADD", "Relative or full path to the file to add");
			}
		}

		public string Command { get { return "addfile"; } }
		
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
			with.FileAppenderFor(file).Append(project, file);
			with.Writer().Write(project);
		}
	}
}
