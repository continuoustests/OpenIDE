using System;
using System.Linq;
using OpenIDE.UI;
using OpenIDE.CommandBuilding;
using System.Collections.Generic;
using System.IO;
using OpenIDE.Bootstrapping;
using OpenIDE.Core.Language;
using System.Windows.Forms;

namespace OpenIDE.Arguments.Handlers
{
	class RunCommandHandler : ICommandHandler
	{
		private Func<IEnumerable<ICommandHandler>> _commandHandlerFactory;
        private ICommandHandler[] _commandHandlers;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"Launches the command execution window");
				return usage;
			}
		}

		public string Command { get { return "run"; } }
		
        public RunCommandHandler(Func<IEnumerable<ICommandHandler>> handlers)
        {
        	_commandHandlerFactory = handlers;
        }

		public void Execute (string[] arguments)
		{
			var form = new RunCommandForm(
				Directory.GetCurrentDirectory(),
				Bootstrapper.Settings.DefaultLanguage,
				new CommandBuilder(getHandlerParameters().Cast<BaseCommandHandlerParameter>()));
			form.ShowDialog();
		}

        private IEnumerable<CommandHandlerParameter> getHandlerParameters()
        {
        	if (_commandHandlers == null)
        		_commandHandlers = _commandHandlerFactory().ToArray();
            var parameters = new List<CommandHandlerParameter>();
            _commandHandlers.ToList()
                .ForEach(x => 
                    {
                        var usage = x.Usage;
                        if (usage != null)
                            parameters.Add(usage);
                    });
            return parameters;
        }
	}
}
