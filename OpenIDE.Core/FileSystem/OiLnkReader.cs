using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenIDE.Core.FileSystem
{
	public class OiLnkReader
	{
		public class Handler
		{
			public string[] Arguments { get; private set; }
			public string[] Responses { get; private set; }

			public Handler(string[] arguments, string[] responses) {
				Arguments = arguments;
				Responses = responses;
			}

			public bool Matches(string[] args) {
				if (Arguments.Length != args.Length)
					return false;
				for (int i = 0; i < Arguments.Length; i++) {
					if (Arguments[i] != "|ANY|" && Arguments[i] != args[i])
						return false;
				}
				return true;
			}

			public void WriteResponses(Action<string> lineWriter) {
				foreach (var line in Responses)
					lineWriter(line);
			}
		}

		public static OiLnkReader Read(string json) {
			try {
				var data = JObject.Parse(json);
				var handlers = getHandlers(data);
				string command = null;
				string parameters = null;
				var link = data["link"];
				if (link != null) {
					command = link["executable"].ToString();
					parameters = link["params"].ToString();
					if (Environment.OSVersion.Platform != PlatformID.MacOSX && Environment.OSVersion.Platform != PlatformID.Unix) {
						command = command.Replace("/", "\\");
					}
				}
				return new OiLnkReader(
					handlers.ToArray(),
					command,
					parameters);
			} catch {
				return null;
			}
		}

		private static Handler[] getHandlers(JObject data) {
			var handlers = new List<Handler>();
			if (data["handlers"] != null) {
				foreach (var handler in data["handlers"].Children()) {
					var arguments = handler["handler"]["arguments"].ToArray().Select(y => y.ToString()).ToArray();
					var responses = handler["handler"]["responses"].ToArray().Select(y => y.ToString()).ToArray();
					handlers.Add(new Handler(arguments, responses));
				}
			}
			return handlers.ToArray();
		}

		public OiLnkReader(Handler[] handlers, string executable, string parameters) {
			Handlers = handlers;
			LinkCommand = executable;
			LinkArguments = parameters;
		}

		public Handler[] Handlers { get; private set; }
		public string LinkCommand { get; private set; }
		public string LinkArguments { get; private set; }
	}
}