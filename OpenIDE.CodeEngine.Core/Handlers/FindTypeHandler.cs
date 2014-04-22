using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using OpenIDE.Core.Caching;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.CodeEngine.Core.Endpoints;
using OpenIDE.Core.Commands;
using OpenIDE.Core.Logging;

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
			Logger.Write("arg count is " + message.Arguments.Count.ToString());
			if (message.Arguments.Count > 2 || message.Arguments.Count == 0)
				return;
			if (clientID == Guid.Empty)
				return;
			var limit = 0;
			if (message.Arguments.Count == 2) {
				Logger.Write("arg is " + message.Arguments[1]);
				int limitParam;
				if (!int.TryParse(message.Arguments[1], out limitParam))
					return;
				if (limitParam <= 0)
					return;
				limit = limitParam;
			}
			var sb = new StringBuilder();
			sb.Append(message.CorrelationID);
			var formatter = new CacheFormatter();
			List<IGrouping<string, ICodeReference>> result;
			Logger.Write("Taking " + limit.ToString());
			if (limit > 0)
				result = _cache.Find(message.Arguments[0], limit).GroupBy(x => x.File).ToList();
			else
				result = _cache.Find(message.Arguments[0]).GroupBy(x => x.File).ToList();

			result.ForEach(x => {
				sb.AppendLine(formatter.FormatFile(x.Key));
				x.ToList()
					.ForEach(y => sb.AppendLine(formatter.Format(y)));
			});
			_endpoint.Send(sb.ToString(), clientID);
		}
	}
}
