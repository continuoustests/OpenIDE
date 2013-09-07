using System;
using OpenIDE.Core.Logging;
using OpenIDE.Core.Commands;
using OpenIDE.CodeEngine.Core.Endpoints;

namespace OpenIDE.CodeEngine.Core.Handlers
{
	class GetRScriptStateHandler : IHandler
	{
		private CommandEndpoint _endpoint;
		private EventEndpoint _eventEndpoint;

		public GetRScriptStateHandler(CommandEndpoint endpoint, EventEndpoint eventEndpoint) {
			_endpoint = endpoint;
			_eventEndpoint = eventEndpoint;
		}

		public bool Handles(CommandMessage message) {
			return message.Command.Equals("rscript-state");
		}

		public void Handle(Guid clientID, CommandMessage message)  {
			if (clientID == Guid.Empty)
				return;
			if (message.Arguments.Count != 1)
				return;
			var scriptName = message.Arguments[0];
			var state = _eventEndpoint.GetScriptState(scriptName);
			_endpoint.Send(message.CorrelationID + state, clientID);
		}
	}
}