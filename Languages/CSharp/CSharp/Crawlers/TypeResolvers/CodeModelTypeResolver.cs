using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.FileSystem;

namespace CSharp.Crawlers.TypeResolvers
{
    public class CodeModelTypeResolver
    {
        private string _currentDir;
        private Instance _codeModel;

        public CodeModelTypeResolver(string currentPath) {
            _currentDir = Environment.CurrentDirectory;
        }

        public string MatchTypeName(string typeName, IEnumerable<string> usings) {
            if (_codeModel == null) {
                _codeModel = new CodeEngineDispatcher(new FS()).GetInstance(_currentDir);
                if (_codeModel == null)
                    return null;
                _codeModel.KeepAlive();
            }
            var refs = new CodeModelResultParser()
                .ParseRefs( _codeModel.GetCodeRefs("language=C#,name=" + typeName));
            if (refs.Count == 0)
                return null;
            foreach (var usng in usings) {
                var match = refs.FirstOrDefault(x => x.Namespace + "." + x.Name == usng + "." + typeName);
                if (match != null)
                    return match.Namespace + "." + match.Name;
            }
            return null;
        }
    }
}
