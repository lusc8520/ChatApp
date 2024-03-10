using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.ServiceModel;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.contract;

[ServiceKnownType("GetKnownTypes", typeof(Helper))]
[ServiceContract(CallbackContract = typeof(IChatClient))]
public interface IChatService
{
    [OperationContract(IsOneWay = true)]
    void Connect();
    [OperationContract]
    LoginResponse Login(string username, string password);

    [OperationContract]
    LoginResponse Register(string username, string password);

    [OperationContract]
    IEnumerable<ChatRoom> FetchChatRooms();
    
    [OperationContract]
    IEnumerable<UserDto> FetchUsers();

    [OperationContract(IsOneWay = true)]
    void BroadcastMessage(MessageDto messageDto);

    [OperationContract(IsOneWay = true)]
    void SendPrivateMessage(MessageDto messageDto);

    [OperationContract]
    IEnumerable<MessageDto> FetchMessages(int chatRoomId);

    [OperationContract]
    IEnumerable<MessageDto> FetchPrivateMessages(int user1, int user2);

    [OperationContract(IsOneWay = true)]
    void UploadPdf(byte[] bytes);
    
    [OperationContract]
    byte[] FetchPlugins();
}

static class Helper
{
    public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
    {
        return new List<Type>
        {
            typeof(LoginResponse),
            typeof(UserDto),
            typeof(ChatRoom),
            typeof(MessageDto),
            typeof(IPlugin)
        };
    }
}
