using System;
namespace OpenIDENet.Arguments
{
    public interface ICommandDispatcher
    {
        void For(string name, string[] arguments);
    }
}
