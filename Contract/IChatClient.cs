using System;
using System.ServiceModel;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.contract;

[ServiceKnownType("GetKnownTypes", typeof(Helper))]
[ServiceContract]
public interface IChatClient
{
    [OperationContract(IsOneWay = true)]
    void Connect();

    [OperationContract(IsOneWay = true)]
    [ServiceKnownType(typeof(MessageDto))]
    void ReceiveBroadcast(MessageDto messageDto);
    
    [OperationContract(IsOneWay = true)]
    [ServiceKnownType(typeof(MessageDto))]
    void ReceivePrivateMessage(MessageDto messageDto);
    
    [OperationContract(IsOneWay = true)]
    void ReceiveNewUser(UserDto user);
    
    public event Action<MessageDto> MessageReceiving;
}
