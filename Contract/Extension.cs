using System;

namespace de.hsfl.vs.hul.chatApp.contract;

public static class Extension
{
    public static void Print(this object obj)
    {
        Console.WriteLine(obj);
    }
}