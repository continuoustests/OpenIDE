using System;
using OpenIDE.CodeEngine.Core.Endpoints;
using OpenIDE.CodeEngine.Core.Handlers;
using OpenIDE.Core.Commands;
using OpenIDE.Core.Logging;

namespace OpenIDE.CodeEngine.Core.Handlers
{
    public class WriteOutputHandler : IHandler
    {
        private EventEndpoint _endpoint;

        public WriteOutputHandler(EventEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public bool Handles(CommandMessage message)
        {
            return message.Command.Equals("write-output");
        }

        public void Handle(Guid clientID, CommandMessage message)
        {
            Logger.Write("Handling write output");
            if (message.Arguments.Count != 2)
                return;
            _endpoint.WriteOutput(message.Arguments[0], message.Arguments[1]);
        }
    }
}
