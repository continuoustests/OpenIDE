using System;
namespace OpenIDENet.Messaging
{
	public interface IConsumer
    {
        bool CanHandle(IMessage message);
        void Consume(IMessage message);
    }
}

