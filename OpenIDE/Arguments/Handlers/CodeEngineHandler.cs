using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.Caching;
using OpenIDE.Core.Language;
namespace OpenIDE.Arguments.Handlers
{
	class CodeEngineGoToHandler : ICommandHandler
	{
		private ICodeEngineLocator _codeEngineFactory;
		private OpenIDE.Core.EditorEngineIntegration.ILocateEditorEngine _editorEngineFactory;
		
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Launches the code engines type search window");
				usage.Add("[SEARCH-STRING]", "When passed it will perform the search in the command line");
				return usage;
			}
		}

		public string Command { get { return "gototype"; } }
		
		public CodeEngineGoToHandler(ICodeEngineLocator codeEngineFactory, OpenIDE.Core.EditorEngineIntegration.ILocateEditorEngine editorFactory)
		{
			_codeEngineFactory = codeEngineFactory;
			_editorEngineFactory = editorFactory;
		}
		
		public void Execute (string[] arguments)
		{
			var instance = _codeEngineFactory.GetInstance(Environment.CurrentDirectory);
			if (instance == null)
				return;
			if (arguments.Length == 0)
				instance.GoToType();
			else
				consoleSearch(instance, arguments[0]);
		}

		private void consoleSearch(Instance instance, string search)
		{
			var result = instance.FindTypes(search);
			var searchResult = new SearchResult();
			var handler = new CrawlHandler(searchResult, (s) => {});
			handler.TypeSearchAllTheThings();
			result
				.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
				.ToList()
				.ForEach(x => {
					var line = x;
					var signatureStart = line.IndexOf("|signature|");
					if (signatureStart > 0) {
						line = line.Substring(signatureStart + 1, line.Length - (signatureStart + 1));
					}
					handler.Handle(line);
				});
			for (int i = 0; i < searchResult.Signatures.Count; i++)
				Console.WriteLine("{0} - {1}",
					i + 1,
					searchResult.Signatures[i].Signature);
			if (searchResult.Signatures.Count == 0)
				return;
			var selection = Console.ReadLine();
			int number;
			if (!int.TryParse(selection, out number))
				return;
			if (number < 1 || number > (searchResult.Signatures.Count))
				return;
			var signature = searchResult.Signatures[number - 1];
			var editor = _editorEngineFactory.GetInstance(Environment.CurrentDirectory);
			if (editor == null)
				return;
			editor.GoTo(signature.File, signature.Line, signature.Column);
		}
	}

	class SearchResult : ICrawlResult
	{
		public List<ICodeReference> Signatures = new List<ICodeReference>();

		public void Add(Project project)
		{
		}

		public void Add(ProjectFile file)
		{
		}

		public void Add(ICodeReference reference)
		{
			Signatures.Add(reference);
		}

		public void Add(IEnumerable<ICodeReference> references)
		{
			Signatures.AddRange(references);
		}

		public void Add(ISignatureReference reference)
		{
		}
	}
}

