using System;
using System.Collections.Generic;
using OpenIDE.Core.Logging;

namespace OpenIDE.Core.Requests
{
    class RequestRunner
    {
        private string _keyPath;
        private Func<OpenIDE.Core.EditorEngineIntegration.Instance> _editorFactory;
        private Func<OpenIDE.Core.CodeEngineIntegration.Instance> _codemodelFactory;

        public RequestRunner(string keyPath) {
            _keyPath = keyPath;
            _editorFactory = () => {
                var locator = new OpenIDE.Core.EditorEngineIntegration.EngineLocator(new OpenIDE.Core.FileSystem.FS());
                return locator.GetInstance(_keyPath);
            };
            _codemodelFactory = () => {
                var locator = new OpenIDE.Core.CodeEngineIntegration.CodeEngineDispatcher(new OpenIDE.Core.FileSystem.FS());
                return locator.GetInstance(_keyPath);
            };
        }

        public bool IsRequest(string msg) {
            return msg.StartsWith("request|");
        }

        public List<string> Request(string msg) {
            var response = new List<string>();
            var editorRequest = "request|editor";
            var codeModelRequest = "request|codemodel";
            if (msg.StartsWith(editorRequest))
                response.AddRange(handleEditorRequest(msg.Substring(editorRequest.Length, msg.Length - editorRequest.Length).ToString().Trim()));
            else if (msg.StartsWith(codeModelRequest))
                response.AddRange(handleCodemodelRequest(msg.Substring(codeModelRequest.Length, msg.Length - codeModelRequest.Length).ToString().Trim()));
            response.Add("end-of-conversation");
            return response;
        }

        private List<string> handleEditorRequest(string editorCmd) {
            var response = new List<string>();
            var editor = _editorFactory();
            if (editor == null)
                return response;
            Func<string> request = null;
            if (editorCmd.StartsWith("get-dirty-files"))
                request = () => editor.GetDirtyFiles(editorCmd.Replace("get-dirty-files", "").Trim());
            else if (editorCmd == "get-caret")
                request = () => editor.GetCaret();
            else if (editorCmd == "get-windows")
                request = () => editor.GetWindows();
            if (request != null) {
                var content = request();
                response.Add(content);
            }
            return response;
        }

        private List<string> handleCodemodelRequest(string codemodelCmd) {
            var response = new List<string>();
            var editor = _editorFactory();
            if (editor == null)
                return response;
            var content = _codemodelFactory().Query(codemodelCmd);
            response.Add(content);
            return response;
        }
    }
}