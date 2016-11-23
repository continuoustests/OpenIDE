using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using OpenIDE.Arguments;
using OpenIDE.Core.Language;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.OutputEndpointIntegration;

namespace OpenIDE.Arguments.Handlers
{
    class OutputListener : ICommandHandler
    {
        private string _token;
        private Action<string> _dispatch;
        private ICodeEngineLocator _locator;

        public CommandHandlerParameter Usage {
            get {
                var usage = new CommandHandlerParameter(
                    "All",
                    CommandType.FileCommand,
                    Command,
                    "Listens to and prints output from the environment");
                usage.Add("PUBLISHER", "Name of the publisher to listen for");
                usage.Add("[-f]", "Keep listening even if the environment shuts down");
                usage.Add("[--all]", "Watch all active environments");
                return usage;
            }
        }

        public string Command { get { return "output-listener"; } }

        public OutputListener(string token, Action<string> dispatch, ICodeEngineLocator locator) {
            _token = token;
            _dispatch = dispatch;
            _locator = locator;
        }

        public void Execute(string[] arguments) {
            var follow = shouldFollow(ref arguments);
            var all = watchAll(ref arguments);
			string lastPublisher = null;
            Action<string,string> printer = (publisher, message) => {
				if (publisher == lastPublisher) {
					_dispatch(message);
				} else {
					//_dispatch("color|Whtie|"+publisher+":");
					_dispatch(publisher+":");
					_dispatch(Environment.NewLine+message);
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
            if (all) {
                var envList = new Dictionary<string, Thread>();
                while (true) {
                    var instances = _locator.GetInstances();
                    foreach (var instance in instances) {
                        if (!envList.ContainsKey(instance.Key)) {
                            var dirName = Path.GetFileName(instance.Key);
                            Action<string, string> instancePrinter = (publiser, message) => {
                                printer(dirName + ": " + publiser, message);
                            };
                            var thread = new Thread(watchOutput);
                            thread.Start(new WatchInstance() { Follow = true, Path = instance.Key, Printer = instancePrinter});
                            envList.Add(instance.Key, thread);
                            printer("output-listener", "started listener for " + instance.Key);
                        }
                    }
                    Thread.Sleep(5000);
                }
            } else {
                watchOutput(new WatchInstance() { Follow = follow, Path = _token, Printer = printer});
            }
        }

        private void watchOutput(object state) {
            var instance = (WatchInstance)state;
            var client = new OutputClient(instance.Path, instance.Printer);
            while (true) {
                client.Connect();
                while (client.IsConnected) {
                    Thread.Sleep(100);
                }
                if (!instance.Follow)
                    break;
                Thread.Sleep(1000);
            }
        }

        private bool shouldFollow(ref string[] args) {
            var follow = args.Contains("-f");
            args = args.Where(x => x != "-f").ToArray();
            return follow;
        }

        private bool watchAll(ref string[] args) {
            var all = args.Contains("--all");
            args = args.Where(x => x != "--all").ToArray();
            return all;
        }
    }

    class WatchInstance
    {
        public bool Follow { get; set; }
        public string Path { get; set; }
        public Action<string,string> Printer { get; set; }
    }
}
