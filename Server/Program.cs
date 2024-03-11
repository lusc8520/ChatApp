using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.ServiceModel;
using Dapper;
using BC = BCrypt.Net.BCrypt;
using Dapper.Contrib.Extensions;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;
using de.hsfl.vs.hul.chatApp.server.DAO;

namespace de.hsfl.vs.hul.chatApp.server;

internal static class Program
{
    public static void Main()
    {
        // configure chat service with tcp binding
        var binding = new NetTcpBinding
        {
            MaxReceivedMessageSize = 6000000, // = 6mb for pdf upload
            MaxBufferSize = 6000000
        };
        
        using var serviceHost = new ServiceHost(typeof(ChatService), new Uri("net.tcp://localhost:9000/chatApp"));
        serviceHost.AddServiceEndpoint(typeof(IChatService), binding, "");
        
        // prepare sql statements
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
        const string messageSql = """
                                  create table if not exists Messages (
                                    id integer primary key autoincrement,
                                    senderId integer,
                                    chatRoomId integer,
                                    `text` varchar(100),
                                    `datetime` datetime,
                                    isPrivate boolean default 0,
                                    isFile boolean default 0,
                                    foreign key (senderId) references Users(id),
                                    foreign key (chatRoomId) references Chatrooms(id)
                                  )
                                  """;
        
        // initialize database
        var dataBasePath = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName + "/data.db";
        var dataBase = new SQLiteConnection($"Data Source={dataBasePath}");
        dataBase.Open();
        dataBase.Execute("drop table if exists Users");
        dataBase.Execute("drop table if exists Chatrooms");
        dataBase.Execute("drop table if exists Messages");
        dataBase.Execute(userSql);
        dataBase.Execute(chatRoomSql);
        dataBase.Execute(messageSql);
        
        // insert default users for testing
        dataBase.Insert(new UserDao{Username = "user", Password = "user"});
        Enumerable.Range(1, 5).AsList()
            .ForEach(i => dataBase.Insert(new UserDao {Username = $"user{i}", Password = $"user{i}"}));
        dataBase.Insert(new UserDao { Username = "someverylongusername", Password = "user" }); // test long name for ui
        
        // insert default group chats
        new List<string> { "General", "IT", "Media", "Gaming", "Sports", "Food", "Art" }
            .ForEach(s => dataBase.Insert(new ChatRoom {Name = s}));
        dataBase.Insert(new ChatRoom { Name = "someverylongchatroomname" }); // test long name for ui
        
        dataBase.Close();
        serviceHost.Open();
        Console.WriteLine("service started!");
        
        // plugin test
        // foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        // {
        //     if (!typeof(IPlugin).IsAssignableFrom(type)) continue;
        //     var plugin = Activator.CreateInstance(type) as IPlugin;
        // }
        
        Console.ReadLine();
    }
}
