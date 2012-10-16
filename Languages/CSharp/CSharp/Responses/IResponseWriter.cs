using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp.Responses
{
    public interface IResponseWriter
    {
        void Write(string message);
        void Write(string message, params object[] args);
    }

    public class NullResponseWriter : IResponseWriter
    {
        public void Write(string message) {}
        public void Write(string message, params object[] args) {}
    }

    public class ConsoleResponseWriter : IResponseWriter
    {
        public void Write(string message)
        {
            Console.WriteLine(message);
        }

        public void Write(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }
    }
}
