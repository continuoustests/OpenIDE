using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenIDE.Core.Logging;

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
            Logger.Write("Response: " + message);
            Console.WriteLine(message);
        }

        public void Write(string message, params object[] args)
        {
            Logger.Write("Response: " + message, args);
            Console.WriteLine(message, args);
        }
    }
}
