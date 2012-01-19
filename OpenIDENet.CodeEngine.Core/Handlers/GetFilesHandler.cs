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
	class GetFilesHandler : IHandler
	{
		private CommandEndpoint _endpoint;
		private ITypeCache _cache;

		public GetFilesHandler(CommandEndpoint endpoint, ITypeCache cache)
		{
			_endpoint = endpoint;
			_cache = cache;
		}

		public bool Handles(CommandMessage message)
		{
			return message.Command.Equals("get-files");
		}

		public void Handle(Guid clientID, CommandMessage message)
		{
			if (clientID == Guid.Empty)
				return;
			var query = getQuery(message);
			var sb = new StringBuilder();
			var formatter = new CacheFormatter();
			_cache.AllFiles()
				.Where(x => filter(x, query))
				.GroupBy(x => x.Project).ToList()
				.ForEach(x =>
					{
						if (x.Key != null)
							sb.AppendLine(formatter.FormatProject(x.Key));
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
					Project = getValue(queryArgs, "project"),
					File = getValue(queryArgs, "file"),
				};
		}

		private string getValue(List<KeyValuePair<string,string>> queryArgs, string name)
		{
			var items = queryArgs.Where(x => x.Key.Equals(name));
			if (items.Count() == 0)
				return null;
			return items.ElementAt(0).Value;
		}

		private bool filter(ProjectFile reference, Query query)
		{
			if (query == null)
				return true;
			if (query.Project != null && !wildcardmatch(reference.Project, query.Project))
				return false;
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
			public string Project { get; set; }
			public string File { get; set; }
		}
	}
}
