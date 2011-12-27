using System;
namespace CSharp.Commands
{
	class AddFileHandler : ICommandHandler
	{
		public string Usage {
			get {
					return Command + "|\"Adds a file to the closest project\"" +
								"FILE_TO_ADD|\"Relative or full path to the file to add\" end" +
						   "end";
			}
		}

		public string Command { get { return "addfile"; } }
		
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
			with.FileAppenderFor(file).Append(project, file);
			with.Writer().Write(project);*/
		}
	}
}
