using System;
using System.IO;
using CSharp.Responses;
using CSharp.Crawlers.TypeResolvers;
using OpenIDE.Core.EditorEngineIntegration;
using OpenIDE.Core.FileSystem;
using CSharp.Crawlers.TypeResolvers;

namespace CSharp.Commands
{
	class GoToDefinitionHandler : ICommandHandler
	{
        private IOutputWriter _globalCache;

		public string Usage {
			get {
				return
					Command + "|\"Moves to where the signature under the cursor is defined\"" +
						"POSITION|\"File position as FILE|LINE|COLUMN\" end " +
					"end ";
			}
		}

		public string Command { get { return "go-to-definition"; } }

		public GoToDefinitionHandler(IOutputWriter globalCache)
        {
            _globalCache = globalCache;
        }

		public void Execute(IResponseWriter writer, string[] args)
		{
			if (args.Length != 1)
				return;
			var chunks = args[0].Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
			if (chunks.Length != 3)
				return;
			try {
				var file = chunks[0];
				var line = int.Parse(chunks[1]);
				var column = int.Parse(chunks[2]);
				
				var cache = 
	                new DirtyFileParser(
	                    _globalCache,
	                    (fileName) => File.ReadAllText(fileName),
						(fileName) => File.Delete(fileName),
                        (fileName) => {
                            var instance = new EngineLocator(new FS()).GetInstance(Environment.CurrentDirectory);
                            if (instance != null)
                                return instance.GetDirtyFiles(fileName);
                            return "";
                        }).Parse(file);

				var name = new TypeUnderPositionResolver()
					.GetTypeName(file, File.ReadAllText(file), line, column);
				var signature = new FileContextAnalyzer(_globalCache, cache)
					.GetSignatureFromTypeAndPosition(file, name, line, column);
				
			} catch (Exception ex) {
				writer.Write(ex.ToString());
			}
		}
	}
}