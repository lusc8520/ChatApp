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
    public bool isFile { get; set; }

    public static MessageDao FromDto(IMessageDto textMessage)
    {
        return new MessageDao
        {
            SenderId = textMessage.Sender.Id,
            ChatRoomId = textMessage.ChatId,
            Text = textMessage.Text,
            DateTime = textMessage.DateTime
        };
    }

    public IMessageDto ToDto(SQLiteConnection db)
    {
        db.Open();
        var user = db.QuerySingle<UserDao?>("select * from Users where id = @UserId", new {UserId = SenderId})?.ToDto();
        db.Close();
        IMessageDto message;
        if (isFile)
        {
            message = new FileMessage();
        }
        else
        {
            message = new TextMessage();
        }
        message.Sender = user;
        message.Text = Text;
        message.ChatId = ChatRoomId;
        message.DateTime = DateTime;
        return message;
    }
}