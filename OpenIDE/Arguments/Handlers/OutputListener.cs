using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using OpenIDE.Arguments;
using OpenIDE.Core.Language;
using OpenIDE.Core.OutputEndpointIntegration;

namespace OpenIDE.Arguments.Handlers
{
    class OutputListener : ICommandHandler
    {
        private string _token;
        private Action<string> _dispatch;

        public CommandHandlerParameter Usage {
            get {
                var usage = new CommandHandlerParameter(
                    "All",
                    CommandType.FileCommand,
                    Command,
                    "Listens to and prints output from the environment");
                usage.Add("PUBLISHER", "Name of the publisher to listen for");
                usage.Add("[-f]", "Keep listening even if the environment shuts down");
                return usage;
            }
        }

        public string Command { get { return "output-listener"; } }

        public OutputListener(string token, Action<string> dispatch) {
            _token = token;
            _dispatch = dispatch;
        }

        public void Execute(string[] arguments) {
            var follow = shouldFollow(ref arguments);
			string lastPublisher = null;
            Action<string,string> printer = (publisher, message) => {
				if (publisher == lastPublisher) {
					_dispatch(message);
				} else {
					_dispatch(publisher+":"+Environment.NewLine+message);
				}
				lastPublisher = publisher;
            };
            if (arguments.Length == 1) {
                var matcher = new Regex(
                    "^" + Regex.Escape(arguments[0])
                        .Replace( "\\*", ".*" )
                        .Replace( "\\?", "." ) + "$");
                printer = (publisher, message) => {
                    if (!matcher.IsMatch(publisher))
                        return;
                    _dispatch(message);
                };
            }
            var client = new OutputClient(_token, printer);
            while (true) {
                client.Connect();
                while (client.IsConnected) {
                    Thread.Sleep(100);
                }
                if (!follow)
                    break;
                Thread.Sleep(1000);
            }
        }

        private bool shouldFollow(ref string[] args) {
            var follow = args.Contains("-f");
            args = args.Where(x => x != "-f").ToArray();
            return follow;
        }
    }
}
