using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using CSharp.Responses;

namespace CSharp.Commands
{
	class AddFileHandler : ICommandHandler
	{
		private Func<string, ProviderSettings> _getTypesProviderByLocation;
		public string Usage {
			get {
					return Command + "|\"Adds a file to the closest project\" " +
								"FILE_TO_ADD|\"Relative or full path to the file to add. Supports *\" " +
									"[--recursive]|\"Will search folders recursively for file pattern.\" end " +
									"[-r]|\"Short version for --recursive\" end " +
								"end " +
						   "end ";
			}
		}

		public string Command { get { return "addfile"; } }
		
		public AddFileHandler(Func<string, ProviderSettings> provider)
		{
			_getTypesProviderByLocation = provider;
		}

		public void Execute(IResponseWriter writer, string[] arguments)
		{
			var args = parseArguments(writer, arguments);
			if (args == null)
				return;

			var provider = _getTypesProviderByLocation(args.File);
			if (provider == null)
				return;

			var files = filesFromArgument(args);
			var with = provider.TypesProvider;
			var project = with.Reader().Read(provider.ProjectFile);
			foreach (var fileToAdd in files)
			{
				var file = with.FileTypeResolver().Resolve(fileToAdd);
				if (file == null)
					return;
				with.FileAppenderFor(file).Append(project, file);
				with.Writer().Write(project);
			}
		}

		private IEnumerable<string> filesFromArgument(CommandArguments args)
		{
			var filename = Path.GetFileName(args.File);
			var directory = Path.GetDirectoryName(args.File);
			if (!args.Recursive)
				return filesFromArgument(directory, filename);
			var files = new List<string>();
			foreach (var dir in getDirectoriesRecursive(directory))
				files.AddRange(filesFromArgument(dir, filename));
			return files;
		}

		private IEnumerable<string> getDirectoriesRecursive(string directory)
		{
			var dirs = new List<string>();
			dirs.Add(directory);
			foreach (var dir in Directory.GetDirectories(directory))
				dirs.AddRange(getDirectoriesRecursive(dir));
			return dirs;
		}

		private IEnumerable<string> filesFromArgument(string directory, string filename)
		{
			if (!filename.Contains("*"))
				return new[] { Path.Combine(directory, filename) };
			return Directory.GetFiles(directory)
				.Where(x => wildcardmatch(x, filename));
		}

		private bool wildcardmatch(string path, string pattern)
		{
			var rgx = new Regex(
				"^" + Regex.Escape(pattern)
					.Replace( "\\*", ".*" )
					.Replace( "\\?", "." ) + "$");
			return rgx.IsMatch(Path.GetFileName(path));
		}

		private CommandArguments parseArguments(IResponseWriter writer, string[] arguments)
		{
			var file = arguments.FirstOrDefault(x => !x.StartsWith("-"));
			if (file == null)
			{
				writer.Write("error|No file argument provided");
				return null;
			}
			return new CommandArguments()
				{
					File = file,
					Recursive = arguments.Contains("--recursive") || arguments.Contains("-r")
				};
		}

		class CommandArguments
		{
			public string File { get; set; }
			public bool Recursive { get; set; }
		}
	}
}
