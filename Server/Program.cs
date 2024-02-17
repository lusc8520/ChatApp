using System;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.ServiceModel;
using Dapper;
using BC = BCrypt.Net.BCrypt;
using Dapper.Contrib.Extensions;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;
using User = de.hsfl.vs.hul.chatApp.server.DAO.User;

namespace de.hsfl.vs.hul.chatApp.server;

internal static class Program
{
    public static void Main()
    {
        var dbPath = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName + "/data.db";
        // configure service
        using var serviceHost = new ServiceHost(typeof(ChatService), new Uri("net.tcp://localhost:9000/chatApp"));
        serviceHost.AddServiceEndpoint(typeof(IChatService), new NetTcpBinding(), "");
        
        const string userSql = """
                            create table if not exists Users (
                                id integer primary key autoincrement,
                                username varchar(20),
                                password varchar(20)
                            )
                            """;
        const string chatRoomSql = """
                                create table if not exists Chatrooms (
                                    id integer primary key autoincrement,
                                    name varchar(30)
                                )
                                """;
        
        // initialize database
        var db = new SQLiteConnection($"Data Source={dbPath}");
        db.Open();
        db.Execute("drop table if exists Users");
        db.Execute("drop table if exists Chatrooms");
        db.Execute(userSql); // create user table
        db.Execute(chatRoomSql); // create chatroom table
        db.Insert(new User { Username = "user", Password = "user"}); // insert default users for testing
        db.Insert(new User { Username = "user2", Password = "user2"});
        db.Insert(new ChatRoom { Name = "General" }); // insert default group chats
        db.Insert(new ChatRoom { Name = "IT" });
        db.Close();
        
        serviceHost.Open();
        Console.WriteLine("service started!");
        Console.ReadLine();
    }
}
