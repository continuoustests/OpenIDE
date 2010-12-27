using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenIDENet.Messaging;

namespace OpenIDENet.Tests.Messaging
{
    class Fake_Consumer : IConsumer
    {
        private int _consumedMessages = 0;

        public bool CanHandle(IMessage message)
        {
            return message.GetType().Equals(typeof(Fake_Message));
        }

        public void Consume(IMessage message)
        {
            _consumedMessages++;
        }

        public void ShouldConsumeOneMessage()
        {
            Assert.AreEqual(1, _consumedMessages);
        }
    }
}
