using System;
using System.Collections.Generic;

namespace CSharp.Commands
{
	public class BaseCommandHandlerParameter
	{
		protected List<BaseCommandHandlerParameter> _parameters;
		
		public string Name { get; private set; }
		private string _description = "";
		public string Description { get { return GetDescription(""); } }
		public bool Required { get; private set; }
		public IEnumerable<BaseCommandHandlerParameter> Parameters { get { return _parameters; } }

		public BaseCommandHandlerParameter(string name, string description)
		{
			Name = name;
			_description = description;
			Required = true;
			_parameters = new List<BaseCommandHandlerParameter>();
		}

		public BaseCommandHandlerParameter IsOptional()
		{
			Required = false;
			return this;
		}

		public BaseCommandHandlerParameter Add(string name, string description)
		{
			var parameter = new BaseCommandHandlerParameter(name, description);
			_parameters.Add(parameter);
			return parameter;
		}
		
		public BaseCommandHandlerParameter Add(BaseCommandHandlerParameter parameter)
		{
			_parameters.Add(parameter);
			return parameter;
		}

		public BaseCommandHandlerParameter Insert(string name, string description)
		{
			var parameter = new BaseCommandHandlerParameter(name, description);
			_parameters.Insert(0, parameter);
			return parameter;
		}

		public string GetDescription(string newline)
		{
			return _description.Replace("||newline||", newline);
		}
	}

	public class CommandHandlerParameter : BaseCommandHandlerParameter
	{
		public string Language { get; private set; }
		public CommandType Type { get; private set; }

		public CommandHandlerParameter(string language, CommandType type, string name, string description) :
			base(name, description)
		{
			Language = language;
			Type = type;
		}
	}

	public enum CommandType
	{
		Initialization,
		FileCommand,
		ProjectCommand,
		Run
	}
}
