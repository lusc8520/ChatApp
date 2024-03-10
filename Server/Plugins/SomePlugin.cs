using System;
using de.hsfl.vs.hul.chatApp.contract;

namespace de.hsfl.vs.hul.chatApp.server.Plugins;

public class SomePlugin //: IPlugin
{
    public void Install()
    {
        Console.WriteLine($"Plugin: {GetType().Name}");
    }
    
    public string Execute(string message)
    {
        // Example: Replace all occurrences of "badword" with "*censored*"
        return message.Replace("badword", "*censored*");
    }
}