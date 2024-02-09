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
using de.hsfl.vs.hul.chatApp.server.DAO;

namespace de.hsfl.vs.hul.chatApp.server;

internal static class Program
{
    public static void Main()
    {
        var dbPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/data.db";
        using var serviceHost = new ServiceHost(typeof(ChatService), new Uri("net.tcp://localhost:9000/chatApp"));
        serviceHost.AddServiceEndpoint(typeof(IChatService), new NetTcpBinding(), "");
        
        const string sql = """
                            create table if not exists user (
                                id integer primary key autoincrement,
                                username text,
                                password text
                            )
                            """;
        
        // initialize database
        var db = new SQLiteConnection($"Data Source={dbPath}");
        db.Open();
        db.Execute("drop table if exists user");
        // create user table
        db.Execute(sql);
        // insert default user for testing
        db.Insert(new User { Username = "user", Password = "user"});
        db.Close();
        
        serviceHost.Open();
        Console.WriteLine("service started");
        Console.ReadLine();
    }
}
