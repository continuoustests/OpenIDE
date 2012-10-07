using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenIDE.Core.CodeEngineIntegration;

namespace CSharp.Tests.Crawlers.TypeResolvers.CodeEngine
{
    class Fake_CodeEngineInstance : ICodeEngineInstanceSimple
    {
        public string File { get; set; }
        public int ProcessID { get; set; }
        public string Key { get; set; }
        public int Port { get; set; }

        public void KeepAlive()
        {
        }

        public void Send(string message)
        {
            throw new NotImplementedException();
        }

        private string _queryReturns = "";
        public void OnQueryReturn(string text) { _queryReturns = text; }
        public string Query(string query)
        {
            return _queryReturns;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
