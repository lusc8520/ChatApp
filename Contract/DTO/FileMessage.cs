using System;

namespace de.hsfl.vs.hul.chatApp.contract.DTO;

public class FileMessage : IMessageDto
{
    public int Id { get; set; }
    public UserDto Sender { get; set; }
    public string Text { get; set; }
    public DateTime DateTime { get; set; }
    public int ChatId { get; set; }
}