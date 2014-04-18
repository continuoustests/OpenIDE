using System;
using System.Text;
using OpenIDE.Core.Logging;
using OpenIDE.Core.Requests;

namespace OpenIDE.Core.Integration
{
    public class ResponseDispatcher
    {
        private string _token;
        private bool _dispatchErrors;
        private string _eventPrefix;
        private Action<string> _dispatcher;
        private Action<string> _dispatchResponse;

        private bool _onlyCommands;
        private RequestRunner _requestRunner;
        private OpenIDE.Core.EditorEngineIntegration.Instance _editor;
        private Func<OpenIDE.Core.EditorEngineIntegration.Instance> _editorFactory;

        public ResponseDispatcher(string token, bool dispatchErrors, string eventPrefix, Action<string> dispatcher, Action<string> dispatchResponse) {
            _token = token;
            _dispatchErrors = dispatchErrors;
            _eventPrefix = eventPrefix;
            _dispatcher = dispatcher;
            _dispatchResponse = dispatchResponse;
            _requestRunner = new RequestRunner(_token);
            _editorFactory = () => {
                if (_editor == null) {
                    var locator = new OpenIDE.Core.EditorEngineIntegration.EngineLocator(new OpenIDE.Core.FileSystem.FS());
                    _editor = locator.GetInstance(_token);
                }
                return _editor;
            };
        }

        public void OnlyCommands() {
            _onlyCommands = true;
        }

        public void Handle(bool error, string m)
        {
            if (m == null)
                return;

            var cmdText = "command|";
            var eventText = "event|";
            if (error) {
                if (_dispatchErrors)
                    _dispatcher(_eventPrefix + "error|" + m);
            } else {
                if (m.StartsWith(cmdText)) {
                    var toDispatch = m.Substring(cmdText.Length, m.Length - cmdText.Length);
                    var args = toDispatch.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                    if (args.Length > 1 && args[0] == "editor") {
                        var _editor = _editorFactory();
                        if (_editor == null)
                            return;
                        _editor.Send(toDispatch.Substring(6, toDispatch.Length - 6).Trim());
                    } else {
                        _dispatcher(toDispatch);
                    }
                } else if (m.StartsWith(eventText)) {
                    if (_onlyCommands)
                        return;
                    _dispatcher(m.Substring(eventText.Length, m.Length - eventText.Length));
                } else if (_requestRunner.IsRequest(m)) {
                    if (_onlyCommands)
                        return;
                    var response = _requestRunner.Request(m);
                    foreach (var content in response)
                        _dispatchResponse(content);
                } else {
                    if (_onlyCommands)
                        return;
                    _dispatcher(_eventPrefix + m);
                }
            }
        }
    }
}