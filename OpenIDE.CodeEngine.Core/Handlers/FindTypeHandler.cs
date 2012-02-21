using System;
using System.Linq;
using System.Text;
using OpenIDE.Core.Caching;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.CodeEngine.Core.Commands;
using OpenIDE.CodeEngine.Core.Endpoints;
namespace OpenIDE.CodeEngine.Core.Handlers
{
	public class FindTypeHandler : IHandler
	{
		private CommandEndpoint _endpoint;
		private ITypeCache _cache;

		public FindTypeHandler(CommandEndpoint endpoint, ITypeCache cache)
		{
			_endpoint = endpoint;
			_cache = cache;
		}

		public bool Handles(CommandMessage message)
		{
			return message.Command.Equals("find-types");
		}

		public void Handle(Guid clientID, CommandMessage message)
		{
			if (message.Arguments.Count != 1)
				return;
			if (clientID == Guid.Empty)
				return;
			var sb = new StringBuilder();
			sb.Append(message.CorrelationID);
			var formatter = new CacheFormatter();
			_cache.Find(message.Arguments[0])
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
