using System;
namespace CSharp.Commands
{
	class RemoveFileHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					SupportedLanguage.CSharp,
					CommandType.FileCommand,
					Command,
					"Removes a file from the closest project");
				usage.Add("FILE_TO_REMOVE", "Relative or full path to the file to remove");
				return usage;
			}
		}

		public string Command { get { return "removefile"; } }
		
		public void Execute(string[] arguments)
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

