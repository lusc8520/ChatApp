using System;
using System.Collections.Generic;
using System.IO;
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
    [OperationContract(IsOneWay = true)]
    public void Logout(int userId);

    [OperationContract]
    LoginResponse Register(string username, string password);

    [OperationContract]
    IEnumerable<ChatRoom> FetchChatRooms();
    
    [OperationContract]
    IEnumerable<UserDto> FetchUsers();

    [OperationContract(IsOneWay = true)]
    void BroadcastMessage(TextMessage textMessage);

    [OperationContract(IsOneWay = true)]
    void SendPrivateMessage(IMessageDto message);

    [OperationContract]
    IEnumerable<IMessageDto> FetchMessages(int chatRoomId);

    [OperationContract]
    IEnumerable<IMessageDto> FetchPrivateMessages(int user1, int user2);

    [OperationContract(IsOneWay = true)]
    void UploadFile(byte[] bytes, string filename, string fileExtension, UserDto sender, int chatId, bool isPrivate);

    [OperationContract]
    byte[] DownloadFile(string filename);

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
            typeof(TextMessage),
            typeof(FileMessage),
            typeof(IMessageDto)
        };
    }
}
