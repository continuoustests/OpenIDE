using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using OpenIDE.Messaging;

namespace OpenIDE.Tests.Messaging
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

        public void ShouldConsumeOneMessageWithinOneSecond()
        {
			var then = DateTime.Now.AddSeconds(1);
			while (DateTime.Now < then && _consumedMessages == 0)
				Thread.Sleep(10);
            Assert.AreEqual(1, _consumedMessages);
        }
    }
}
