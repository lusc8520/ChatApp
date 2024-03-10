using System;
using Dapper.Contrib.Extensions;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.server.DAO;

[Table("Users")]
public class UserDao
{
    public int Id { get; set; }
    public string Username { get; set; }
    
    public string Password { get; set; }

    public UserDto ToDto()
    {
        return new UserDto
        {
            Id = Id,
            Username = Username
        };
    }
    
}