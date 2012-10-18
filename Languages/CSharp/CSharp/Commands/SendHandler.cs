using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Tcp;
using OpenIDE.Core.Commands;

namespace CSharp.Commands
{
    class SendHandler : ICommandHandler
    {
        private CSharpClient _client;

        public SendHandler(CSharpClient client) {
            _client = client;
        }

        public string Usage {
            get {
                return Command + "|\"Sends a command to a service hosted CSharp plugin\" " +
                                "COMMAND-ARGS|\"A list of arguments representing the command (shutdown exits the service)\" end " +
                           "end ";
            }
        }

        public string Command { get { return "send"; } }

        public void Execute(Responses.IResponseWriter writer, string[] arguments) {
            var cmd = CommandMessage.New(arguments);
            _client.Request(cmd.ToString());
        }
    }
}
