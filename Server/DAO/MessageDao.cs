using System;
using System.Data.SQLite;
using Dapper;
using Dapper.Contrib.Extensions;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.server.DAO;

[Table("Messages")]
public class MessageDao
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ChatRoomId { get; set; }
    public string Text { get; set; }
    public DateTime DateTime { get; set; }
    
    public bool IsPrivate { get; set; }

    public static MessageDao FromDto(MessageDto message)
    {
        return new MessageDao
        {
            SenderId = message.Sender.Id,
            ChatRoomId = message.ChatId,
            Text = message.Text,
            DateTime = message.DateTime
        };
    }

    public MessageDto ToDto(SQLiteConnection db)
    {
        db.Open();
        var user = db.QuerySingle<UserDao?>("select * from Users where id = @UserId", new {UserId = SenderId});
        db.Close();
        return new MessageDto
        {
            Sender = user?.ToDto(),
            ChatId = ChatRoomId,
            Text = Text,
            DateTime = DateTime
        };
    }
}