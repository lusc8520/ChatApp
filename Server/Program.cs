using System;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.ServiceModel;
using Dapper;
using BC = BCrypt.Net.BCrypt;
using Dapper.Contrib.Extensions;
using de.hsfl.vs.hul.chatApp.server.DAO;

namespace de.hsfl.vs.hul.chatApp.server
{
    internal static class Program
    {
        public static void Main()
        {
            var dbPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/data.db";
            Console.WriteLine(dbPath);
            using var serviceHost = new ServiceHost(typeof(ChatService));
            serviceHost.Opened += (_, _) =>
            {
                Console.WriteLine("Service Opened!");
                Console.ReadLine();
            };
            // test sqlite
            var db = new SQLiteConnection($"Data Source={dbPath}");
            db.Execute("drop table if exists user");
            
            // create user table
            const string sql1 = """
                                create table if not exists user (
                                    id integer primary key autoincrement,
                                    username text,
                                    password text
                                )
                                """;
            db.Execute(sql1);
            db.DeleteAll<User>();
            
            // test user insert
            db.Insert(new User { Username = "Timo" });
            db.Insert(new User { Username = "Tom" });
            
            // test user query
            db.GetAll<User>().ToList().ForEach(user =>
            {
                Console.WriteLine(user.Username);
                Console.WriteLine(user.Id);
            });
           
            serviceHost.Open();
        }
    }
}