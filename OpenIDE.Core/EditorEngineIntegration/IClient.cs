using System;
namespace OpenIDE.Core.EditorEngineIntegration
{
	public interface IClient
	{
		bool IsConnected { get; }
		void Connect(int port, Action<string> onMessage);
		void Disconnect();
		void Send(string message);
		void SendAndWait(string message);
		string Request(string message);
	}
}

