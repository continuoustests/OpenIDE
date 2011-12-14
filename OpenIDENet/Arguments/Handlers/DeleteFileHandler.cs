using System;
using System.IO;
using OpenIDENet.Languages;
namespace OpenIDENet.Arguments.Handlers
{
	class DeleteFileHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					SupportedLanguage.CSharp,
					CommandType.FileCommand,
					"deletefile",
					"Removes a file from the closest project and deletes it");
				usage.Add("FILE_TO_DELETE", "Relative or full path to the file to delete");
				return usage;
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

