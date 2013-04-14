using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.CodeEngine.Core.Endpoints;
using Commands = OpenIDE.Core.Commands;
using OpenIDE.Core.Language;

namespace OpenIDE.CodeEngine.Core.Handlers
{
    class LanguageCommandHandler : IHandler
    {
        private CommandEndpoint _endpoint;
        private ICacheBuilder _cache;
        private PluginLocator _pluginLocator;

        public LanguageCommandHandler(CommandEndpoint endpoint, ICacheBuilder cache, PluginLocator pluginLocator) {
            _endpoint = endpoint;
            _cache = cache;
            _pluginLocator = pluginLocator;
        }

        public bool Handles(Commands.CommandMessage message) {
            return new PluginFinder(_cache).FindLanguage(message.Command) != null;
        }

        public void Handle(Guid clientID, Commands.CommandMessage message) {
            var response = new StringBuilder();
            if (message.CorrelationID != null)
                response.Append(message.CorrelationID);

            if (message.Arguments.Count < 2)
                return;

            var language = new PluginFinder(_cache).FindLanguage(message.Command);
            if (language == null)
                return;

            var plugin = _pluginLocator.Locate().FirstOrDefault(x => x.GetLanguage() == language);
            if (plugin == null)
                return;


            if (message.Arguments[0] == "command") {
                plugin.Run(message.Arguments.Skip(1).ToArray());
                return;
            }
            if (message.Arguments[0] == "query") {
                foreach (var line in plugin.Query(message.Arguments.Skip(1).ToArray()))
                    response.AppendLine(line);
                _endpoint.Send(response.ToString(), clientID);
            }
        }
    }
}
