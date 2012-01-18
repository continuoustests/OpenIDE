using System;
using System.Linq;
using System.Text;
using OpenIDENet.CodeEngine.Core.Caching;
using OpenIDENet.CodeEngine.Core.Commands;
using OpenIDENet.CodeEngine.Core.Endpoints;
using OpenIDENet.CodeEngine.Core.Logging;

namespace OpenIDENet.CodeEngine.Core.Handlers
{
	public class GetCodeRefsHandler : IHandler
	{
		private CommandEndpoint _endpoint;
		private ITypeCache _cache;

		public GetCodeRefsHandler(CommandEndpoint endpoint, ITypeCache cache)
		{
			_endpoint = endpoint;
			_cache = cache;
		}

		public bool Handles(CommandMessage message)
		{
			return message.Command.Equals("get-code-refs");
		}

		public void Handle(Guid clientID, CommandMessage message)
		{
			if (clientID == Guid.Empty)
				return;
			var sb = new StringBuilder();
			var formatter = new CacheFormatter();
			_cache.All()
				.GroupBy(x => x.File).ToList()
				.ForEach(x =>
					{
						sb.AppendLine(formatter.FormatFile(x.Key));
						x.ToList()
							.ForEach(y => sb.AppendLine(formatter.Format(y)));
					});
			_endpoint.Send(sb.ToString(), clientID);
		}
	}
}
