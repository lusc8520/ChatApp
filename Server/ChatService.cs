using System;
using System.ServiceModel;
using de.hsfl.vs.hul.chatApp.contract;

namespace de.hsfl.vs.hul.chatApp.server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class ChatService : IChatService
    {
        public void Connect()
        {
            Console.WriteLine("Client Connected!");
            var client = OperationContext.Current.GetCallbackChannel<IChatClient>();
            client.Receive("Server Callback!");
        }
    }
}