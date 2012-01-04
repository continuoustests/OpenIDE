using System;
using OpenIDENet.CodeEngine.Core.Caching;
using OpenIDENet.CodeEngine.Core.Logging;

namespace OpenIDENet.CodeEngine.Core.ChangeTrackers
{
	public class CrawlHandler
	{
		private string _currentProject = null;
		private string _currentFile = null;
		
		private ICacheBuilder _builder;

		public CrawlHandler(ICacheBuilder builder)
		{
			_builder = builder;
		}

		public void Handle(string command)
		{
			try {
				var chunks = command.Trim()
					.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
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
			} catch (Exception ex) {
				Logger.Write(ex);
			}
		}
		
		private void handleProject(string[] chunks)
		{
			_currentProject = chunks[1];
			_builder.Add(new Project(_currentProject));
		}

		private void handleFile(string[] chunks)
		{
			_currentFile = chunks[1];
			_builder.Add(new ProjectFile(_currentFile, _currentProject));
		}
		
		private void handleSignature(string[] chunks)
		{
			_builder.Add(new CodeReference(
				chunks[3],
				_currentFile,
				chunks[1],
				chunks[2],
				int.Parse(chunks[4]),
				int.Parse(chunks[5]),
				int.Parse(chunks[6])));
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
	}
}
