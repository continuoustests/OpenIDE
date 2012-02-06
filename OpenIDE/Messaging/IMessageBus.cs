using System;
namespace OpenIDE.Messaging
{
	public interface IMessageBus
    {
        void Register(IConsumer consumer);
        void Publish(IMessage message);
    }
}

