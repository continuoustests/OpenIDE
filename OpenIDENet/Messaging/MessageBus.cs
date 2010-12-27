using System;
using System.Collections.Generic;
using System.Threading;
namespace OpenIDENet.Messaging
{
	public class MessageBus : IMessageBus
    {
        private List<IConsumer> _consumers = new List<IConsumer>();

        public void Register(IConsumer consumer)
        {
            _consumers.Add(consumer);
        }

        public void Publish(IMessage message)
        {
            foreach (var consumer in _consumers)
                consume(consumer, message);
        }

        private static void consume(IConsumer consumer, IMessage message)
        {
            if (consumer.CanHandle(message))
                ThreadPool.QueueUserWorkItem(consume, new ThreadInfo(consumer, message));
        }

        private static void consume(object info)
        {
            var threadInfo = (ThreadInfo) info;
            threadInfo.Consumer.Consume(threadInfo.Message);
        }
    }
}

