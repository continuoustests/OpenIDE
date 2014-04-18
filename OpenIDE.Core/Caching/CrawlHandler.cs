using System;
using System.Collections.Generic;

namespace OpenIDE.Core.Caching
{
	public class CrawlHandler
	{
		private string _language;
		private bool _addAllToTypeSearch = false;
		private string _currentProject = null;
		private string _currentFile = null;
		private Action<string> _logWrite;
		
		private ICrawlResult _builder;

		public CrawlHandler(ICrawlResult builder, Action<string> logWrite)
		{
			_builder = builder;
			_logWrite = logWrite;
		}

		public void SetLanguage(string language)
		{
			_language = language;
		}

		public void TypeSearchAllTheThings() {
			_addAllToTypeSearch = true;
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
			if (chunks.Length < 3)
				return;
			_currentProject = chunks[1];
			var project = new Project(_currentProject, chunks[2]);
			var args = getArguments(chunks, 3);
			if (args.Contains("filesearch"))
				project.SetFileSearch();
			_builder.Add(project);
		}

		private void handleFile(string[] chunks)
		{
			if (chunks.Length < 2)
				return;
			_currentFile = chunks[1];
			var file = new ProjectFile(_currentFile, _currentProject);
			var args = getArguments(chunks, 2);
			if (args.Contains("filesearch"))
				file.SetFileSearch();
			_builder.Add(file);
		}
		
		private void handleSignature(string[] chunks)
		{
			if (chunks.Length < 9)
				return;
			var reference = new CodeReference(
				_language,
				chunks[4],
				_currentFile,
				chunks[1],
				chunks[2],
                chunks[3],
                chunks[5],
				tryParse(chunks[6]),
				tryParse(chunks[7]),
                chunks[8]);

			var args = getArguments(chunks, 9);
			if (_addAllToTypeSearch || args.Contains("typesearch"))
				reference.SetTypeSearch();

			_builder.Add(reference);
		}

		private void handleReference(string[] chunks)
		{
			if (chunks.Length < 4)
				return;
			_builder.Add(new SignatureReference(
				_currentFile,
				chunks[1],
				tryParse(chunks[2]),
				tryParse(chunks[3])));
		}

		private List<string> getArguments(string[] args, int fixNumber)
		{
			var list = new List<string>();
			for (int i = fixNumber; i < args.Length; i++)
				list.Add(args[i]);
			return list;
		}

		private int tryParse(string number)
		{
			int intNumber;
			if (int.TryParse(number, out intNumber))
				return intNumber;
			_logWrite("Invalid number given: " + number);
			return 0;
		}
	}
}
