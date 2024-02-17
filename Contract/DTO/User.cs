using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace de.hsfl.vs.hul.chatApp.contract.DTO;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }

    public override bool Equals(object obj)
    {
        return obj is User user && Id == user.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}