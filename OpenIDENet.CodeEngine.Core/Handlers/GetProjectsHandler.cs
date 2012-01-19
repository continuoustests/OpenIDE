using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenIDENet.CodeEngine.Core.Caching;
using OpenIDENet.CodeEngine.Core.Commands;
using OpenIDENet.CodeEngine.Core.Endpoints;
using OpenIDENet.CodeEngine.Core.Logging;

namespace OpenIDENet.CodeEngine.Core.Handlers
{
	class GetProjectsHandler : IHandler
	{
		private CommandEndpoint _endpoint;
		private ITypeCache _cache;

		public GetProjectsHandler(CommandEndpoint endpoint, ITypeCache cache)
		{
			_endpoint = endpoint;
			_cache = cache;
		}

		public bool Handles(CommandMessage message)
		{
			return message.Command.Equals("get-projects");
		}

		public void Handle(Guid clientID, CommandMessage message)
		{
			if (clientID == Guid.Empty)
				return;
			var query = getQuery(message);
			var sb = new StringBuilder();
			var formatter = new CacheFormatter();
			_cache.AllProjects()
				.Where(x => filter(x, query)).ToList()
				.ForEach(x => sb.AppendLine(formatter.Format(x)));
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
					File = getValue(queryArgs, "file")
				};
		}

		private string getValue(List<KeyValuePair<string,string>> queryArgs, string name)
		{
			var items = queryArgs.Where(x => x.Key.Equals(name));
			if (items.Count() == 0)
				return null;
			return items.ElementAt(0).Value;
		}

		private bool filter(Project reference, Query query)
		{
			if (query == null)
				return true;
			if (query.File != null && !wildcardmatch(reference.File, query.File))
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
		}
	}
}
