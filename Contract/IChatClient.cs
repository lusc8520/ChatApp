using System.ServiceModel;

namespace de.hsfl.vs.hul.chatApp.contract
{
    [ServiceContract]
    public interface IChatClient
    {
        [OperationContract(IsOneWay = true)]
        void Receive(string s);
    }
}