using System;
using System.Collections.Generic;

namespace OpenIDE.Core.Caching
{
	public class CrawlHandler
	{
		private string _currentProject = null;
		private string _currentFile = null;
		private Action<string> _logWrite;
		
		private ICrawlResult _builder;

		public CrawlHandler(ICrawlResult builder, Action<string> logWrite)
		{
			_builder = builder;
			_logWrite = logWrite;
		}

		public void Handle(string command)
		{
			try {
				var chunks = command.Trim()
					.Split(new char[] { '|' }, StringSplitOptions.None);
				if (chunks.Length == 0)
					return;
				if (chunks[0] == "project")
					handleProject(chunks);
				if (chunks[0] == "file")
					handleFile(chunks);
				if (chunks[0] == "signature")
					handleSignature(chunks);
				if (chunks[0] == "reference")
					handleReference(chunks);
				if (chunks[0] == "error")
					_logWrite(command);
				if (chunks[0] == "comment")
					_logWrite(command);
			} catch (Exception ex) {
				_logWrite(ex.ToString());
			}
		}
		
		private void handleProject(string[] chunks)
		{
			_currentProject = chunks[1];
			var project = new Project(_currentProject);
			var args = getArguments(chunks, 2);
			if (args.Contains("filesearch"))
				project.SetFileSearch();
			_builder.Add(project);
		}

		private void handleFile(string[] chunks)
		{
			_currentFile = chunks[1];
			var file = new ProjectFile(_currentFile, _currentProject);
			var args = getArguments(chunks, 2);
			if (args.Contains("filesearch"))
				file.SetFileSearch();
			_builder.Add(file);
		}
		
		private void handleSignature(string[] chunks)
		{
			var reference = new CodeReference(
				chunks[3],
				_currentFile,
				chunks[1],
				chunks[2],
				int.Parse(chunks[4]),
				int.Parse(chunks[5]),
				int.Parse(chunks[6]));

			var args = getArguments(chunks, 7);
			if (args.Contains("typesearch"))
				reference.SetTypeSearch();

			_builder.Add(reference);
		}

		private void handleReference(string[] chunks)
		{
			_builder.Add(new SignatureReference(
				_currentFile,
				chunks[1],
				int.Parse(chunks[2]),
				int.Parse(chunks[3]),
				int.Parse(chunks[4])));
		}

		private List<string> getArguments(string[] args, int fixNumber)
		{
			var list = new List<string>();
			for (int i = fixNumber; i < args.Length; i++)
				list.Add(args[i]);
			return list;
		}
	}
}
