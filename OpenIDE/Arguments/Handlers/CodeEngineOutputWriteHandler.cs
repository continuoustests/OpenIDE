using System;
using OpenIDE.Arguments;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
    class CodeEngineOutputWriteHandler : ICommandHandler
    {
        private ICodeEngineLocator _codeEngineFactory;
        
        public CommandHandlerParameter Usage {
            get {
                var usage = new CommandHandlerParameter(
                    "All",
                    CommandType.FileCommand,
                    Command,
                    "Writes a message to the output endpoint");
                var publisher = usage.Add("PUBLISHER", "Name of publisher");
                publisher.Add("MESSAGE", "Message to pass on to the endpoint");
                return usage;
            }
        }

        public string Command { get { return "output"; } }
        
        public CodeEngineOutputWriteHandler(ICodeEngineLocator codeEngineFactory)
        {
            _codeEngineFactory = codeEngineFactory;
        }
        
        public void Execute (string[] arguments)
        {
            using (var instance = _codeEngineFactory.GetInstance(Environment.CurrentDirectory)) {
                if (instance == null)
                    return;
                if (arguments.Length != 2)
                    return;
                instance.Send("write-output \"" + arguments[0] + "\" \"" + arguments[1] + "\"");
            }
        }
    }
}
