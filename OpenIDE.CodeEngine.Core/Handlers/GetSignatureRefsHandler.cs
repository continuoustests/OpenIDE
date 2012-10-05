using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenIDE.Core.Caching;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.CodeEngine.Core.Commands;
using OpenIDE.CodeEngine.Core.Endpoints;
using OpenIDE.Core.Logging;

namespace OpenIDE.CodeEngine.Core.Handlers
{
	public class GetSignatureRefsHandler : IHandler
	{
		private CommandEndpoint _endpoint;
		private ITypeCache _cache;

		public GetSignatureRefsHandler(CommandEndpoint endpoint, ITypeCache cache)
		{
			_endpoint = endpoint;
			_cache = cache;
		}

		public bool Handles(CommandMessage message)
		{
			return message.Command.Equals("get-signature-refs");
		}

		public void Handle(Guid clientID, CommandMessage message)
		{
			if (clientID == Guid.Empty)
				return;
			var query = getQuery(message);
			var sb = new StringBuilder();
			sb.Append(message.CorrelationID);
			var formatter = new CacheFormatter();
			_cache.AllSignatures()
				.Where(x => filter(x, query))
				.GroupBy(x => x.File).ToList()
				.ForEach(x =>
					{
						sb.AppendLine(formatter.FormatFile(x.Key));
						x.ToList()
							.ForEach(y => sb.AppendLine(formatter.Format(y)));
					});
			_endpoint.Send(sb.ToString(), clientID);
		}

		private Query getQuery(CommandMessage message)
		{
			var query = "";
			if (message.Arguments.Count > 0)
				query = message.Arguments[0];
			var queryArgs = new QueryArgumentParser().Parse(query);
			if (queryArgs == null)
				return null;
			return new Query()
				{
					File = getValue(queryArgs, "file"),
					Signature = getValue(queryArgs, "signature")
				};
		}

		private string getValue(List<KeyValuePair<string,string>> queryArgs, string name)
		{
			var items = queryArgs.Where(x => x.Key.Equals(name));
			if (items.Count() == 0)
				return null;
			return items.ElementAt(0).Value;
		}

		private bool filter(ISignatureReference reference, Query query)
		{
			if (query == null)
				return true;
			if (query.File != null && !wildcardmatch(reference.File, query.File))
				return false;
			if (query.Signature != null && !wildcardmatch(reference.Signature, query.Signature))
				return false;
			return true;
		}
		
		private bool wildcardmatch(string word, string pattern)
		{
			var rgx = new Regex(
				"^" + Regex.Escape(pattern)
					.Replace( "\\*", ".*" )
					.Replace( "\\?", "." ) + "$");
			return rgx.IsMatch(word);
		}

		class Query
		{
			public string File { get; set; }
			public string Signature { get; set; }
		}
	}
}
