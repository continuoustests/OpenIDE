using System;
namespace OpenIDE.Arguments
{
    public interface ICommandDispatcher
    {
        void For(string name, string[] arguments);
    }
}
