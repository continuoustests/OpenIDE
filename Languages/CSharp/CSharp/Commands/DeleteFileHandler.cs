using System;
using System.IO;
namespace CSharp.Commands
{
	class DeleteFileHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"C#",
					CommandType.FileCommand,
					Command,
					"Removes a file from the closest project and deletes it");
				usage.Add("FILE_TO_DELETE", "Relative or full path to the file to delete");
				return usage;
			}
		}

		public string Command { get { return "deletefile"; } }
		
		public void Execute(string[] arguments)
		{
			// TODO fix implementation
			/*var provider = getTypesProviderByLocation(arguments[0]);
			if (provider == null)
				return;
			var with = provider.TypesProvider;
			var file = with.FileTypeResolver().Resolve(arguments[0]);
			if (file == null)
				return;
			var project = with.Reader().Read(provider.ProjectFile);
			with.FileRemoverFor(file).Remove(project, file);
			with.Writer().Write(project);
			
			File.Delete(file.Fullpath);*/
		}
	}
}

