using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using CoreExtensions;
using OpenIDE.Core.Logging;
using OpenIDE.Core.Language;
using OpenIDE.Core.RScripts;

namespace OpenIDE.Arguments.Handlers
{
	class TestReactiveScriptHandler : ICommandHandler
	{
        private string _token;
        private Action<string> _dispatch;
        private Func<PluginLocator> _pluginLocator;

        public CommandHandlerParameter Usage {
            get {
                    var usage = new CommandHandlerParameter(
                        "All",
                        CommandType.FileCommand,
                        Command,
                        "Tests reactive scripts");
                    return usage;
            }
        }

        public string Command { get { return "repl"; } }

        public TestReactiveScriptHandler(Action<string> dispatch, Func<PluginLocator> locator, string token)
        {
            _token = token;
            _dispatch = dispatch;
            _pluginLocator = locator;
        }

        public void Execute(string[] arguments)
        {
            if (arguments.Length < 2)
                return;
            var scripts = new ReactiveScriptReader(
                _token,
                _pluginLocator,
                (m) => {})
                .Read();
            var script = scripts.FirstOrDefault(x => x.Name.Equals(arguments[0]));
            if (script == null)
                return;

            var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var match1 = "'codemodel' 'raw-filesystem-change-filecreated' '" + script.File + "'";
            var match2 = "'codemodel' 'raw-filesystem-change-filechanged' '" + script.File + "'";
            Logger.Write("Looking for: " + match1);
            Logger.Write("Looking for: " + match2);
            var name = "rscript-" + Path.GetFileNameWithoutExtension(script.File) + " ";
            var hash = 0;
            try {
                var proc = new Process();
                proc.Query(
                    Path.Combine(root, Path.Combine("EventListener", "OpenIDE.EventListener.exe")),
                    "",
                    false,
                    _token,
                    (error, s) => {
                        if (s.StartsWith(name)) {
                            _dispatch(s.Substring(name.Length, s.Length - name.Length));
                        }
                        if (s == match1 || s == match2) {
                            var newHas = File.ReadAllText(script.File).GetHashCode();
                            if (newHas != hash) {
                                hash = newHas;
                                Thread.Sleep(200);
                                _dispatch("");
                                _dispatch("Triggering reactive script:");
                                _dispatch("event|"+arguments[1]);
                            }
                        }
                    }
                );
            } catch (Exception ex) {
                Logger.Write(ex);
            }
        }
	}
}