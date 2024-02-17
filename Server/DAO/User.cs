using System;
using Dapper.Contrib.Extensions;
using DTO = de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.server.DAO;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    
    public string Password { get; set; }

    public DTO.User ToDto()
    {
        return new DTO.User
        {
            Id = Id,
            Username = Username
        };
    }
    
}