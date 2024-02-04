using System;
using de.hsfl.vs.hul.chatApp.contract;

namespace de.hsfl.vs.hul.chatApp.client
{
    public class ChatClient : IChatClient
    {
        public void Receive(string s)
        {
            Console.WriteLine($"Received Message: {s}");
        }
    }
}