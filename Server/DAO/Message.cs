using System;
using Dapper.Contrib.Extensions;

namespace de.hsfl.vs.hul.chatApp.server.DAO;

public class Message
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ChatRoomId { get; set; }
    public string Text { get; set; }
    public DateTime DateTime { get; set; }
}