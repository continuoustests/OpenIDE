using System;
namespace OpenIDE.Messaging
{
	public interface IConsumer
    {
        bool CanHandle(IMessage message);
        void Consume(IMessage message);
    }
}
