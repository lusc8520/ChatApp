using Dapper.Contrib.Extensions;

namespace de.hsfl.vs.hul.chatApp.server.DAO;

[Table("user")]
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    
    public string Password { get; set; }
    
    
}