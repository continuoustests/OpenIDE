using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Core.Commands;
using OpenIDE.Core.CommandBuilding;
using OpenIDE.Core.UI;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Config;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.Core.Logging;
using OpenIDE.Core.EditorEngineIntegration;
using OpenIDE.CodeEngine.Core.Endpoints;

namespace OpenIDE.CodeEngine.Core.Handlers
{
	public class CompleteSnippetHandler : IHandler
	{
		private ICacheBuilder _cache;
		private string _keyPath;
		private Action<string> _dispatch;
        private CommandEndpoint _endpoint;
        private Instance _editor;

		public CompleteSnippetHandler(ICacheBuilder cache, string keyPath, CommandEndpoint endpoint)
		{
			_cache = cache;
			_keyPath = keyPath;
			_endpoint = endpoint;
            _dispatch = (msg) => {
            	Logger.Write("dispatching " + msg);
            	_endpoint.Handle(msg);
            };
            _editor = new EngineLocator(new FS()).GetInstance(_keyPath);
		}

		public bool Handles(CommandMessage message)
		{
			return message.Command.Equals("complete-snippet");
		}

		// Handles complete-snippet command
		public void Handle(Guid clientID, CommandMessage message)
		{
			Logger.Write("Handling complete-snippet");
            var caret = _editor.GetCaret();
            if (caret == "")
                return;
            Logger.Write("caret is: " + caret);
            var lines = caret.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            var position = new FilePosition(lines[0]);
            var line = lines[position.Line];
            var word = Word.Extract(line, position.Column);
            _editor.Send(string.Format(
                "remove \"{0}|{1}|{2}\" \"{3}|{4}\"",
                position.Fullpath,
                position.Line.ToString(),
                word.Column.ToString(),
                position.Line.ToString(),
                (word.Column + word.Content.Length).ToString()
            ));
            var whitespaces = getWhitespacePrefix(line);
            // Dispatch message to be handled by the handle method if not overridden by another
            // external handler
            _dispatch(string.Format(
                "snippet-complete \"{0}\" \"{1}\" \"{2}|{3}|{4}\" \"{5}\"",
                Path.GetExtension(position.Fullpath),
                word.Content,
                position.Fullpath,
                position.Line,
                word.Column,
                whitespaces
            ));
        }

        private string getWhitespacePrefix(string line) {
            var sb = new StringBuilder();
            foreach (var chr in line)
            {
                if (chr == ' ')
                    sb.Append("s");
                else if (chr == '\t')
                    sb.Append("t");
                else
                    break;
            }
            return sb.ToString();
        }

        public class Word
        {
            public static Word Extract(string line, int position)
            {
                return new Word(line, position);
            }

            private string _line;
            private int _position;
            
            public string Content { get; private set; }
            public int Column { get; private set; }

            public Word(string line, int position)
            {
                _line = line;
                if (position != 0)
                    _position = position - 1;
                var start = getStart();
                var end = getEnd(start);
                Content = _line.Substring(start, end - start);
                Column = start + 1;
                if (start == position)
                    Column = start;
            }

            private int getStart()
            {
                var separators = new List<int>();
                separators.Add(_line.LastIndexOf(" ", _position));
                separators.Add(_line.LastIndexOf("\t", _position));
                if (separators.Max(x => x) == -1)
                    return 0;
                return separators.Max(x => x) + 1;
            }

            private int getEnd(int after)
            {
                var separators = new List<int>();
                separators.Add(_line.IndexOf(" ", after));
                separators.Add(_line.IndexOf("\t", after));
                if (separators.Max(x => x) == -1)
                    return _line.Length;
                return separators.Max(x => x);
            }
        }

        // Handles snippet-complete command
		public void Handle(string[] arguments)
		{
			if (arguments.Length < 3)
				return;
			var language = getLanguage(arguments[0]);
			if (language == null)
				return;
			var file = getLocal(arguments);
			if (!File.Exists(file))
				file = getGlobal(arguments);

			if (!File.Exists(file))
				return;
			var lines = File.ReadAllLines(file);
			if (lines.Length < 3)
				return;
			var placeHolders = lines[0];
			var offsetLine = lines[1];
			var snippet = new StringBuilder();
			for (int i = 2; i < lines.Length; i++)
				snippet.AppendLine(lines[i]);
			var indentation = "";
			if (arguments.Length == 4)
				indentation = arguments[3].Replace("t", "\t").Replace("s", " ");
			var form = new SnippetForm(
				new OpenIDE.Core.CommandBuilding.CommandStringParser(',').Parse(placeHolders).ToArray(),
				snippet.ToString());
			int lineCount = 0;
			int lastLineLength = 0;
			form.OnRun((content) =>
				{
					try {
						var sb = new StringBuilder();
						var contentLines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
						for (int i = 0; i < contentLines.Length; i++)
						{
							var contentLine  = contentLines[i];
							if (i == 0)
							{
								sb.AppendLine(contentLine);
								lastLineLength = contentLine.Length;
								lineCount++;
							}
							else if (i == contentLines.Length - 1 && contentLine == "")
								break;
							else if (i == contentLines.Length - 1)
							{
								sb.Append(indentation + contentLine);
								lastLineLength = indentation.Length + contentLine.Length;
								lineCount++;
							}
							else
							{
								sb.AppendLine(indentation + contentLine);
								lastLineLength = indentation.Length + contentLine.Length;
								lineCount++;
							}
						}

						form = null;
						var tempFile = Path.GetTempFileName();
						File.WriteAllText(tempFile, sb.ToString());
						var offset = new OpenIDE.Core.CommandBuilding.Position(offsetLine);
						if (offset.Line == -1 && offset.Column == -1)
							offset = new OpenIDE.Core.CommandBuilding.Position(lineCount - 1, lastLineLength);
						else
							offset.AddToColumn(indentation.Length);
			            var insertMessage = "insert \"" + tempFile + "\" \"" +  arguments[2] + "\" \"" + offset.ToCommand() + "\"";
			            if (_editor != null)
							_editor.Send(insertMessage);
					} catch (Exception ex) {
						Logger.Write(ex);
					}
				});
			form.Show();
			form.BringToFront();
		}
		
		private string getGlobal(string[] arguments)
		{
				return getPath(
					Path.GetDirectoryName(
						Path.GetDirectoryName(
							Assembly.GetExecutingAssembly().Location)), arguments);
		}

		private string getLocal(string[] arguments)
		{
				return getPath(Path.GetDirectoryName(
					Configuration.GetConfigFile(_keyPath)), arguments);
		}

		private string getPath(string path, string[] arguments)
		{
			return Path.Combine(
				path,
				Path.Combine(
					".OpenIDE",
					"languages",
					Path.Combine(
						getLanguage(arguments[0]) + "-files",
						Path.Combine("snippets", arguments[1] + ".snippet"))));
		}

		private string getLanguage(string param)
		{
			return new PluginFinder(_cache).FindLanguage(param);
		}
	}
}
