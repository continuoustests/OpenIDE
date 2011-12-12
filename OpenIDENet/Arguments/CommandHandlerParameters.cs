using System;
using System.Collections.Generic;

namespace OpenIDENet.Arguments
{
	public class CommandHandlerParameters
	{
		private List<CommandHandlerParameter> _parameters;

		public CommandHandlerParameter[] Parameters { get { return _parameters; } }

		public CommandHandlerParameters()
		{
			_parameters = new List<CommandHandlerParameter>();
		}

		public CommandHandlerParameters Add(string name, string description)
		{
			_parameters.Add(new CommandHandlerParameter()
				{
					Name = name,
					Description = description,
					Required = true,
					Parameters = null
				});
			return this;
		}

		public CommandHandlerParameters Add(string name, string description, CommandHandlerParameters parameters)
		{
			_parameters.Add(new CommandHandlerParameter()
				{
					Name = name,
					Description = description,
					Required = true,
					Parameters = parameters
				});
			return this;
		}

		public CommandHandlerParameters AddOptional(string name, string description)
		{
			_parameters.Add(new CommandHandlerParameter()
				{
					Name = name,
					Description = description,
					Required = false,
					Parameters = null
				});
			return this;
		}
	}

	public class CommandHandlerParameter
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public bool Required { get; set; }
		public CommandHandlerParameters Parameters { get; set; }
	}
}
