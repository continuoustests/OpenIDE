using System;
using System.IO;
using CSharp.Responses;

namespace CSharp.Commands
{
	class DeleteFileHandler : ICommandHandler
	{
		private Func<string, ProviderSettings> _getTypesProviderByLocation;

		public string Usage {
			get {
				return Command + "|\"Removes a file from the closest project and deletes it\"" +
							"FILE_TO_DELETE|\"Relative or full path to the file to delete\" end " +
					   "end ";
			}
		}

		public string Command { get { return "deletefile"; } }
		
		public DeleteFileHandler(Func<string, ProviderSettings> provider)
		{
			_getTypesProviderByLocation = provider;
		}

		public void Execute(IResponseWriter writer, string[] arguments)
		{
			var provider = _getTypesProviderByLocation(arguments[0]);
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

