using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.Caching;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
    class CodeEnginePublishHandler : ICommandHandler
    {
        private ICodeEngineLocator _codeEngineFactory;
        
        public CommandHandlerParameter Usage {
            get {
                var usage = new CommandHandlerParameter(
                    "All",
                    CommandType.FileCommand,
                    Command,
                    "Publishes command or event to the code engine endpoint");
                usage.Add("MESSAGE", "Message to pass on to the endpoint");
                return usage;
            }
        }

        public string Command { get { return "publish"; } }
        
        public CodeEnginePublishHandler(ICodeEngineLocator codeEngineFactory)
        {
            _codeEngineFactory = codeEngineFactory;
        }
        
        public void Execute (string[] arguments)
        {
            var instance = _codeEngineFactory.GetInstance(Environment.CurrentDirectory);
            if (instance == null)
                return;
            if (arguments.Length == 0)
                return;
            instance.Send(arguments[0]);
        }
    }
}