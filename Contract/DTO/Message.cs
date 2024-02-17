using System;

namespace de.hsfl.vs.hul.chatApp.contract.DTO;

public class Message
{
    public User Sender { get; set; }
    public string Text { get; set; }
    public DateTime DateTime { get; set; }
    public ChatRoom ChatRoom { get; set; } // only used to check id atm
}