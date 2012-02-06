using System;
namespace OpenIDE.Messaging
{
	public class ThreadInfo
    {
        public IConsumer Consumer { get; private set; }
        public IMessage Message { get; private set; }

        public ThreadInfo(IConsumer consumer, IMessage message)
        {
            Consumer = consumer;
            Message = message;
        }
	}
}

