using System;
namespace OpenIDENet.Messaging
{
	public interface IMessageBus
    {
        void Register(IConsumer consumer);
        void Publish(IMessage message);
    }
}

