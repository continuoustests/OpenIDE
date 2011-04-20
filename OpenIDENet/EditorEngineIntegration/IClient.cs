using System;
namespace OpenIDENet
{
	public interface IClient
	{
		bool IsConnected { get; }
		string RecievedMessage { get; }
		void Connect(int port);
		void Disconnect();
		void Send(string message);
		void SendAndWait(string message);
	}
}

