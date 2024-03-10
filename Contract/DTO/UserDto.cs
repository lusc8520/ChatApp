using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace de.hsfl.vs.hul.chatApp.contract.DTO;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
}