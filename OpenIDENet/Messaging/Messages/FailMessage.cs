using System;
namespace OpenIDENet.Messaging.Messages
{
	public class FailMessage : IMessage
	{
		public string Failure { get; set; }
		public FailMessage(string failure)
		{
			Failure = failure;
		}
	}
}

