using System.ServiceModel;

namespace de.hsfl.vs.hul.chatApp.contract
{
    [ServiceContract(CallbackContract = typeof(IChatClient))]
    public interface IChatService
    {
        [OperationContract(IsOneWay = true)]
        void Connect(); // erstmal nur zum testen des servers
    }
}