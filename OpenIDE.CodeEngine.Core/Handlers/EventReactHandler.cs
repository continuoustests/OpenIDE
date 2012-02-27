using System;
using OpenIDE.CodeEngine.Core.Commands;

namespace OpenIDE.CodeEngine.Core.Handlers
{
	public class EventReactHandler : IHandler
	{
		public bool Handles(CommandMessage message)
		{
			return true;
		}

		public void Handle(Guid clientID, CommandMessage message)
		{
			var sb = new System.Text.StringBuilder();
			sb.Append(message.Command + " ");
			foreach (var arg in message.Arguments)
				sb.Append(arg + " ");
			Logging.Logger.Write(sb.ToString());
		}
	}
}
