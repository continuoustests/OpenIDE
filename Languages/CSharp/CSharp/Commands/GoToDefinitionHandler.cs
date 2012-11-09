using System;
using CSharp.Responses;

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
				
				//_resolver = 
				//	new TypeUnderPositionResolver(cache, (type) => new Namespce(null,type,0,0));
				
			} catch (Exception ex) {
				writer.Write(ex.ToString());
			}
		}
	}
}