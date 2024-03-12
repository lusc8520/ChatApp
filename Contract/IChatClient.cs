using System;
using System.ServiceModel;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.contract;


[ServiceContract]
[ServiceKnownType("GetKnownTypes", typeof(Helper))]
public interface IChatClient
{
    [OperationContract(IsOneWay = true)]
    void Connect();

    [OperationContract(IsOneWay = true)]
    [ServiceKnownType(typeof(TextMessage))]
    void ReceiveBroadcast(IMessageDto textMessage);
    [OperationContract(IsOneWay = true)]
    [ServiceKnownType(typeof(TextMessage))]
    void ReceivePrivateMessage(IMessageDto textMessage);
    [OperationContract(IsOneWay = true)]
    void ReceiveNewUser(UserDto user);
    
    public event Action<IMessageDto> MessageReceiving;
    public event Action<IMessageDto> MessageSending;
}
