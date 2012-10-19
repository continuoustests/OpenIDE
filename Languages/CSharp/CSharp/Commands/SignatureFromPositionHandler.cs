using System;
using System.IO;
using CSharp.Responses;
using OpenIDE.Core.EditorEngineIntegration;
using OpenIDE.Core.FileSystem;
using CSharp.Crawlers.TypeResolvers;

namespace CSharp.Commands
{
	class SignatureFromPositionHandler : ICommandHandler
	{
        private IOutputWriter _globalCache;
		public string Usage {
			get {
				return
					Command + "|\"Retrieves enclosing signature from position\"" +
						"POSITION|\"File position as FILE|LINE|COLUMN\" end " +
					"end ";
			}
		}

		public string Command { get { return "signature-from-position"; } }
		
        public SignatureFromPositionHandler(IOutputWriter globalCache)
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

				var signatureFetcher = 
					new EnclosingSignatureFromPosition(
                        _globalCache,
						(fileName) => File.ReadAllText(fileName),
						(fileName) => File.Delete(fileName),
                        (fileName) => {
                            var instance = new EngineLocator(new FS()).GetInstance(Environment.CurrentDirectory);
                            if (instance != null)
                                return instance.GetDirtyFiles(fileName);
                            return "";
                        });
				var signature = signatureFetcher.GetSignature(file, line, column);
				writer.Write(signature);
			} catch (Exception ex) {
				writer.Write(ex.ToString());
			}

			// parse arguments
            // get all dirty buffers
            // parse all dirty buffers
            // parse given file (with variable declaration)
            // get variable / member / type name under or behind position
            // find type of declaration
            // clean up temp buffers
            // return type
		}
	}
}
