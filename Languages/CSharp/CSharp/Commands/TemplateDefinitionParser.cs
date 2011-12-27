using System;
using System.Linq;

namespace CSharp.Commands
{
	public class TemplateDefinitionParser
	{
		public BaseCommandHandlerParameter Parse(string name, string definition)
		{
			try
			{
				if (!definition.Contains("=>"))
					return new BaseCommandHandlerParameter(name, definition);
				var chunks = definition.Split(new string[] { "=>" }, StringSplitOptions.None);
				var parameters = new BaseCommandHandlerParameter(name, chunks[0]);
				BaseCommandHandlerParameter parameter = parameters;
				chunks[1]
					.Split(new string[] { "||" }, StringSplitOptions.None).ToList()
					.ForEach(x =>
						{
							var y =x.Split(new string[] { "|" }, StringSplitOptions.None);
							if (y[0].StartsWith("["))
								parameter = parameter.Add(y[0].Replace("[", "").Replace("]", ""), y[1]).IsOptional();
							else
								parameter = parameter.Add(y[0], y[1]);
						});
				return parameters;
			}
			catch
			{
				return new BaseCommandHandlerParameter(
					"Usage string is not in the right format: " +
					"Description string=>Parameter1|Parameter1 description||" +
					"Parameter2|Parameter2 description",
					definition);
			}
		}
	}
}
