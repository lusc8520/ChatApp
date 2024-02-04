using System;
using System.ServiceModel;

namespace de.hsfl.vs.hul.chatApp.server
{
    internal static class Program
    {
        public static void Main()
        {
            
            using var serviceHost = new ServiceHost(typeof(ChatService));
            serviceHost.Opened += (_, _) =>
            {
                Console.WriteLine("Service Opened!");
                Console.ReadLine();
            };
            var s = new ServiceHost( new ChatService());
            
            serviceHost.Open();
        }
    }
}