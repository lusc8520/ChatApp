using System;
using System.ServiceModel;

namespace de.hsfl.vs.hul.chatApp.contract.DTO;

public class LoginResponse
{
    public User User { get; set; }
    public string Text { get; set; }
}
