using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Arguments;
using System.Text;
using OpenIDE.Core.Language;

namespace OpenIDE.CommandBuilding
{
	public class CommandBuilder
	{
		private List<BaseCommandHandlerParameter> _parameters;
        
		public string[] AvailableCommands { get; private set; }

		public CommandBuilder(IEnumerable<BaseCommandHandlerParameter> parameters)
		{
			_parameters = parameters.ToList();
            NavigateTo("");
		}

        public void NavigateTo(string path)
        {
            AvailableCommands = new string[] {};
            var paths = getPaths()
                .Select(x => new KeyValuePair<string, int>(x, matchWith(x, path)));
            if (paths.Count() > 0)
            {
                var max = paths
                    .Max(x => x.Value);
                AvailableCommands = paths
                    .Where(x => x.Value.Equals(max))
                    .Select(x => x.Key).ToArray();
            }
        }

        public string Describe(string path)
        {
            var chunks = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (chunks.Count() == 0)
                return "";
            var builder = new StringBuilder();
            var available = _parameters;
            int level = 0;
            foreach (var chunk in chunks) {
                if (available == null)
                    return "";
                var command = available.FirstOrDefault(x => x.Name.Equals(chunk));
                if (command == null)
                    return "";
                if (builder.Length == 0)
                {
                    builder.AppendLine(command.Description);
                    builder.AppendLine(command.Name);
                }
                else
                    builder.AppendLine("".PadLeft(level * 4, ' ') + command.Name + " : " + command.Description);
                available = new List<BaseCommandHandlerParameter>(command.Parameters);
                level++;
            }
            return builder.ToString();
        }

        private IEnumerable<string> getPaths()
        {
            var paths = new List<string>();
            _parameters
                .ForEach(x => paths.AddRange(getPaths(x, "/" + x.Name)));
            return paths;
        }

        private IEnumerable<string> getPaths(BaseCommandHandlerParameter x, string path)
        {
            if (x.Parameters.Count() == 0)
                return new string[] { path };
            var paths = new List<string>();
            x.Parameters.ToList()
                .ForEach(y => paths.AddRange(getPaths(y, path + "/" + y.Name)));
            return paths;
        }

        private int matchWith(string x, string path)
        {
            int length = (new int[] { x.Length, path.Length }).Min();
            int matchLength = 0;
            for (int i = 0; i < length; i++)
            {
                if (!x[i].Equals(path[i]))
                    break;
                matchLength++;
            }
            return matchLength;
        }
	}
}
