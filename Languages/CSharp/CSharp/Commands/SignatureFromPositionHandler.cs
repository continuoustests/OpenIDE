using System;
using System.IO;
using OpenIDE.Core.FileSystem;
using CSharp.Crawlers.TypeResolvers;
using OpenIDE.Core.CodeEngineIntegration;

namespace CSharp.Commands
{
	class SignatureFromPositionHandler : ICommandHandler
	{
		public string Usage { get { return null; } }

		public string Command { get { return "signature-from-position"; } }
		
		public void Execute(string[] args)
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
						() => new CodeEngineDispatcher(new FS()).GetInstance(Environment.CurrentDirectory),
						(fileName) => File.ReadAllText(fileName),
						(fileName) => File.Delete(fileName));
				var signature = signatureFetcher.GetSignature(file, line, column);
				Console.WriteLine(signature);
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
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
