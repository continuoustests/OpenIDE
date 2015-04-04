using System;
using System.Collections.Generic;
using OpenIDE.Core.Logging;

namespace OpenIDE.Core.Language
{
	public class BaseCommandHandlerParameter
	{
		protected List<BaseCommandHandlerParameter> _parameters;
		
		public string Name { get; private set; }
		private string _description = "";
		public string Description { get { return GetDescription(""); } }
		public bool Required { get; private set; }
		public bool Override { get; private set; }
		public CommandType Type { get; private set; }
		public IEnumerable<BaseCommandHandlerParameter> Parameters { get { return _parameters; } }

		public BaseCommandHandlerParameter(string name, string description)
		{
            Rebrand(name, description);
		}

		public BaseCommandHandlerParameter(string name, string description, CommandType type)
		{
			Rebrand(name, description, type);
		}

        public void Rebrand(string name, string description)
        {
            Rebrand(name, description, CommandType.SubParameter);
        }

        public void Rebrand(string name, string description, CommandType type)
        {
            Name = name.Replace("[", "").Replace("]", "");
			_description = description;
			setNameProperties(name);
			Type = type;
			_parameters = new List<BaseCommandHandlerParameter>();
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

		private void setNameProperties(string rawName)
		{
			setNameProperties(this, rawName);
		}

		private void setNameProperties(BaseCommandHandlerParameter parameter, string rawName)
		{
			Override = false;
			Required = true;
			if (rawName.StartsWith("[[")) {
				Override = true;
				return;
			}
			if (rawName.StartsWith("["))
				Required = false;
		}
	}

	public class CommandHandlerParameter : BaseCommandHandlerParameter
	{
		public string Language { get; private set; }
		

		public CommandHandlerParameter(string language, CommandType type, string name, string description) :
			base(name, description, type)
		{
			Language = language;
		}
	}

	public enum CommandType
	{
		Initialization,
		FileCommand,
		ProjectCommand,
		Run,
		SubParameter,
		LanguageCommand
	}
}
