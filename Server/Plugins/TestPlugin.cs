using System;
using de.hsfl.vs.hul.chatApp.contract;

namespace de.hsfl.vs.hul.chatApp.server.Plugins;

public class TestPlugin //: IPlugin
{
    public void DoSomething()
    {
        Console.WriteLine($"Plugin: {GetType().Name}");
    }
}