using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using CoreExtensions;
using OpenIDE.Bootstrapping;
using OpenIDE.Core.Language;
using OpenIDE.Core.Logging;
using OpenIDE.Core.Definitions;

namespace OpenIDE.Arguments.Handlers
{
	class TestScriptHandler : ICommandHandler
	{
        private string _token;
        private Action<string> _dispatch;

        public CommandHandlerParameter Usage {
            get {
                    var usage = new CommandHandlerParameter(
                        "All",
                        CommandType.FileCommand,
                        Command,
                        "Continuously tests the specified script when saved");
                    usage
                        .Add("SCRIPT-NAME", "Script name")
                            .Add("PARAMS", "Either script arguments or full command like: oi help mycommand");
                return usage;
            }
        }

        public string Command { get { return "repl"; } }

        public TestScriptHandler(Action<string> dispatcher, string token) {
            _token = token;
            _dispatch = dispatcher;
        }

        public void Execute(string[] arguments) {
            if (arguments.Length < 1)
                return;
            var scriptName = arguments[0];
            var script =
                Bootstrapper.GetDefinitionBuilder()
                    .Definitions
                    .FirstOrDefault(x => x.Name == scriptName &&
                                         (x.Type == DefinitionCacheItemType.Script ||
                                          x.Type == DefinitionCacheItemType.LanguageScript));
            if (script == null)
                return;

            var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var match1 = "'codemodel' 'raw-filesystem-change-filecreated' '" + script.Location + "'";
            var match2 = "'codemodel' 'raw-filesystem-change-filechanged' '" + script.Location + "'";
            Logger.Write("Looking for: " + match1);
            Logger.Write("Looking for: " + match2);
            var hash = 0;
            try {
                var proc = new Process();
                proc.Query(
                    Path.Combine(root, Path.Combine("EventListener", "OpenIDE.EventListener.exe")),
                    "",
                    false,
                    _token,
                    (error, s) => {
                        if (s == match1 || s == match2) {
                            var newHas = File.ReadAllText(script.Location).GetHashCode();
                            if (newHas != hash) {
                                hash = newHas;
                                _dispatch("Running command:");
                                runCommand(arguments);
                            }
                        }
                    }
                );
            } catch (Exception ex) {
                Logger.Write(ex);
            }
        }

        private void runCommand(string[] arguments) {
            var command = "oi";
            var args = "";
            var index = 1;
            if (arguments.Length > 1 && arguments[1] == "oi")
                index = 2;
            else
                args = arguments[0];

            for (int i = index; i < arguments.Length; i++) {
                if (args.Length > 0)
                    args += " ";
                args += "\"" + arguments[i] + "\"";
            }
            var proc = new Process();
            try {
                string[] errors;
                foreach (var line in proc.QueryAll(command, args + " --raw", false, Environment.CurrentDirectory, out errors)) {
                    Logger.Write("line is " + line);
                    _dispatch(line);
                }
                if (errors.Length > 0 && errors[0].Trim() != "") {
                    foreach (var line in errors)
                        _dispatch("error|" + line);
                }
            } catch (Exception ex) {
                Logger.Write(ex);
            }
        }
	}
}