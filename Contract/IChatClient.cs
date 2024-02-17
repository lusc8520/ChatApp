using System.ServiceModel;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.contract;


[ServiceContract]
public interface IChatClient
{
    [OperationContract(IsOneWay = true)]
    void Connect();

    [OperationContract(IsOneWay = true)]
    [ServiceKnownType(typeof(Message))]
    void ReceiveMessage(Message message);
}
