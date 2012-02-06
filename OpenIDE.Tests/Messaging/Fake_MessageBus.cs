using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenIDE.Messaging;

namespace OpenIDE.Tests.Messaging
{
    class Fake_MessageBus : IMessageBus
    {
        private List<Type> _message = new List<Type>();

        public void DidNotPublish<T>()
        {
            Assert.IsFalse(_message.Contains(typeof(T)), "Message was published when it was not supposed to");
        }

        public void Published<T>()
        {
            Assert.IsTrue(_message.Contains(typeof(T)), "Message was not published when it was supposed to");
        }

        public void Register(IConsumer consumer)
        {
            throw new NotImplementedException();
        }

        public void Publish(IMessage message)
        {
            _message.Add(message.GetType());
        }
    }
}
