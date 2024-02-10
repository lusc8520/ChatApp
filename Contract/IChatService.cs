using System;
using System.Collections.Generic;
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
}

static class Helper
{
    public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
    {
        return new List<Type>
        {
            typeof(LoginResponse),
            typeof(User)
        };
    }
}
