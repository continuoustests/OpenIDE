using System;
namespace CSharp.Commands
{
	class RemoveFileHandler : ICommandHandler
	{
		public string Usage {
			get {
				return
					Command + "|\"Removes a file from the closest project\"" +
						"FILE_TO_REMOVE|\"Relative or full path to the file to remove\" end" +
					"end";
			}
		}

		public string Command { get { return "removefile"; } }
		
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
			with.Writer().Write(project);*/
		}
	}
}

