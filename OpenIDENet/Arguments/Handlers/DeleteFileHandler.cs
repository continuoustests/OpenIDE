using System;
using System.IO;
namespace OpenIDENet.Arguments.Handlers
{
	class DeleteFileHandler : ICommandHandler
	{
		public CommandHandlerParameters Usage {
			get {
				return new CommandHandlerParameters()
					.Add("deletefile", "Removes a file from the closest project and deletes it")
					.Add("FILE_TO_DELETE", "Relative or full path to the file to delete");
			}
		}

		public string Command { get { return "deletefile"; } }
		
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
			
			File.Delete(file.Fullpath);
		}
	}
}

