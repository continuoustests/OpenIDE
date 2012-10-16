using System;
using System.IO;
using System.Linq;
using OpenIDE.Core.CommandBuilding;
using OpenIDE.Core.Commands;
using OpenIDE.Core.Language;
using OpenIDE.CodeEngine.Core.Endpoints;
using OpenIDE.CodeEngine.Core.Caching;

namespace OpenIDE.CodeEngine.Core.Handlers
{
	class GoToDefinitionHandler : IHandler
	{
		private CommandEndpoint _endpoint;
		private TypeCache _cache;
		private PluginLocator _pluginLocator;

		public GoToDefinitionHandler(CommandEndpoint endpoint, TypeCache cache, PluginLocator locator)
		{
			_endpoint = endpoint;
			_cache = cache;
			_pluginLocator = locator;
		}

		public bool Handles(CommandMessage message)
		{
			return message.Command.Equals("goto-defiinition");
		}

		public void Handle(Guid clientID, CommandMessage message)
		{
			if (message.Arguments.Count != 1)
				return;
			var position = new OpenIDE.Core.CommandBuilding.FilePosition(message.Arguments[0]);
			var extension = Path.GetExtension(position.Fullpath);
			var language = new PluginFinder(_cache).FindLanguage(extension);
			if (language == null)
				return;
			var plugin = _pluginLocator.Locate()
				.FirstOrDefault(x => x.GetLanguage() == language);
			if (plugin == null)
				return;
			var signature = plugin.SignatureFromPosition(position);
			if (signature == null)
				return;
			var codeRef = _cache.AllSignatures()
				.FirstOrDefault(x => x.Signature == signature.Signature &&
					x.File == signature.File);
			if (codeRef == null)
				return;
			_endpoint.Editor.Send(
				string.Format("goto \"{0}|{1}|{2}\"",
					codeRef.File,
					codeRef.Line,
					codeRef.Column));
		}
	}
}
