using System;
namespace CSharp.Commands
{
	class AddFileHandler : ICommandHandler
	{
		private Func<string, ProviderSettings> _getTypesProviderByLocation;
		public string Usage {
			get {
					return Command + "|\"Adds a file to the closest project\"" +
								"FILE_TO_ADD|\"Relative or full path to the file to add\" end " +
						   "end ";
			}
		}

		public string Command { get { return "addfile"; } }
		
		public AddFileHandler(Func<string, ProviderSettings> provider)
		{
			_getTypesProviderByLocation = provider;
		}

		public void Execute(string[] arguments)
		{
			var provider = _getTypesProviderByLocation(arguments[0]);
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
