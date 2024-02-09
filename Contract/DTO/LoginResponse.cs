using System;
using System.ServiceModel;

namespace de.hsfl.vs.hul.chatApp.contract.DTO;
using System.Runtime.Serialization;

[DataContract]
public class LoginResponse
{
    [DataMember]
    public User User { get; set; }
    [DataMember]
    public string Text { get; set; }
}
