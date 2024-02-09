using System.Runtime.Serialization;
using System.ServiceModel;

namespace de.hsfl.vs.hul.chatApp.contract.DTO;

[DataContract]
public class User
{
    [DataMember]
    public int Id { get; set; }
    [DataMember]
    public string Username { get; set; }
}