using System;
using System.IO;
using System.Linq;
using OpenIDE.CodeEngine.Core.Endpoints;
using OpenIDE.CodeEngine.Core.Handlers;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.Commands;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Profiles;


namespace OpenIDE.CodeEngine.Core.Handlers
{
    public class GetTokenPathHandler : IHandler
    {
        private CommandEndpoint _endpoint;

		public GetTokenPathHandler(CommandEndpoint endpoint) {
			_endpoint = endpoint;
		}

		public bool Handles(CommandMessage message) {
			return 
                message.Command.Equals("get-token-endpoint") ||
                message.Command.Equals("get-token-event-endpoint") ||
                message.Command.Equals("get-token-output-endpoint") ||
                message.Command.Equals("get-token-editor-endpoint") ||
                message.Command.Equals("get-token-path");
		}

		public void Handle(Guid clientID, CommandMessage message)  {
			if (clientID == Guid.Empty)
				return;
			if (message.Arguments.Count != 1)
				return;
			var tokenHint = message.Arguments[0];
            var locator = new ProfileLocator(fixPath(tokenHint));
            var response = "not-running";
            var tokenPath = locator.GetLocalProfilesRoot();
            if (tokenPath != null) {
                tokenPath = Path.GetDirectoryName(tokenPath);
                switch (message.Command) { 
                    case "get-token-path":
                        response = tokenPath;
                        break;
                    case "get-token-endpoint":
                        var instance = (new CodeEngineDispatcher(new FS())).GetInstances().FirstOrDefault(x => matchPath(tokenPath, x.Key));
                        if (instance != null)
                            response = string.Format("127.0.0.1:{0}", instance.Port);
                        break;
                    case "get-token-event-endpoint":
                        var events = (new OpenIDE.Core.EventEndpointIntegrarion.EventClient(tokenPath)).GetInstance();
                        if (events != null)
                            response = string.Format("127.0.0.1:{0}", events.Port);
                        break;
                    case "get-token-output-endpoint":
                        var output = (new OpenIDE.Core.OutputEndpointIntegration.OutputClient(tokenPath)).GetInstance();
                        if (output != null)
                            response = string.Format("127.0.0.1:{0}", output.Port);
                        break;
                    case "get-token-editor-endpoint":
                        var editor = (new OpenIDE.Core.EditorEngineIntegration.EngineLocator(new FS()))
                            .GetInstances()
                            .FirstOrDefault(x => x.Key == tokenPath);
                        if (editor != null)
                            response = string.Format("127.0.0.1:{0}", editor.Port);
                        break;
                }
            }
			_endpoint.Send(message.CorrelationID + response, clientID);
		}

        private bool matchPath(string key, string path) {
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
				return key == path;
			else
				return key.ToLower() == path.ToLower();
		}

        private string fixPath(string path)
		{
            if (File.Exists(path))
                path = Path.GetDirectoryName(path);
			if (path.Contains(":"))
				return path.ToLower();
			return path;
		}
    }
}
