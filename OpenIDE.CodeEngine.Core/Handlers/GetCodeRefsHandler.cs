using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenIDE.Core.Caching;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.CodeEngine.Core.Commands;
using OpenIDE.CodeEngine.Core.Endpoints;
using OpenIDE.CodeEngine.Core.Logging;

namespace OpenIDE.CodeEngine.Core.Handlers
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
			return message.Command.Equals("get-signatures");
		}

		public void Handle(Guid clientID, CommandMessage message)
		{
			if (clientID == Guid.Empty)
				return;
			var query = getQuery(message);
			var sb = new StringBuilder();
			sb.Append(message.CorrelationID);
			var formatter = new CacheFormatter();
			_cache.AllReferences()
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
                    Language = getValue(queryArgs, "language"),
					Type = getValue(queryArgs, "type"),
					File = getValue(queryArgs, "file"),
					Signature = getValue(queryArgs, "signature"),
					Name = getValue(queryArgs, "name"),
                    Parent = getValue(queryArgs, "parent"),
                    Custom = getValue(queryArgs, "custom")
				};
		}

		private string getValue(List<KeyValuePair<string,string>> queryArgs, string name)
		{
			var items = queryArgs.Where(x => x.Key.Equals(name));
			if (items.Count() == 0)
				return null;
			return items.ElementAt(0).Value;
		}

		private bool filter(ICodeReference reference, Query query)
		{
			if (query == null)
				return true;
            if (query.Language != null &&  !wildcardmatch(reference.Language, query.Language))
                return false;
			if (query.Type != null && !wildcardmatch(reference.Type, query.Type))
				return false;
			if (query.File != null && !wildcardmatch(reference.File, query.File))
				return false;
			if (query.Signature != null && !wildcardmatch(reference.Signature, query.Signature))
				return false;
			if (query.Name != null && !wildcardmatch(reference.Name, query.Name))
				return false;
            if (query.Parent != null && !wildcardmatch(reference.Parent, query.Parent))
				return false;
            if (query.Custom != null && !wildcardmatch(reference.JSON, query.Custom))
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
            public string Language { get; set; }
			public string Type { get; set; }
			public string File { get; set; }
			public string Signature { get; set; }
			public string Name { get; set; }
            public string Parent { get; set; }
            public string Custom { get; set; }
		}
	}
}
