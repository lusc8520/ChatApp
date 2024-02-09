using System.ServiceModel;

namespace de.hsfl.vs.hul.chatApp.contract;


public interface IChatClient
{
    [OperationContract(IsOneWay = true)]
    void Receive(string s); // erstmal nur zum testen des server callbacks
}
