using System;
namespace OpenIDE.CodeEngine.Core.Endpoints.Tcp
{
	public class MessageArgs : EventArgs
	{
		public Guid ClientID { get; private set; }
		public string Message { get; private set; }
		
		public MessageArgs(Guid clientID, string message)
		{
			ClientID = clientID;
			Message = message;
		}
	}
	
	public interface ITcpServer
	{
		event EventHandler ClientConnected;
		event EventHandler<MessageArgs> IncomingMessage;
		
		int Port { get; }
		
		void Start();
		void Send(string message);
		void Send(string message, Guid clientID);
	}
}

