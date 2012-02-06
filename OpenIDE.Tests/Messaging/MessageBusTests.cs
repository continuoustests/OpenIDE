using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Threading;
using OpenIDE.Messaging;

namespace OpenIDE.Tests.Messaging
{
    [TestFixture]
    public class MessageBusTests 
    {
        private IMessageBus _bus;

        [SetUp]
        public void Initialize()
        {
            _bus = new MessageBus();
        }

        [Test]
        public void MessageBus_WhenPublishing_ItShouldDistributeMessagesToConsumers()
        {
            var consumer = new Fake_Consumer();
            _bus.Register(consumer);
            _bus.Publish(new Fake_Message());
            consumer.ShouldConsumeOneMessageWithinOneSecond();
        }
    }
}
