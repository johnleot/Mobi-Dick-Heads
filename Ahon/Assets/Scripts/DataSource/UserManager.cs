using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Data.Sqlite;
using UnityEngine;
using System.IO;
using Assets.Scripts.DataSource;
using Assets.Scripts.Classes;

namespace Assets.Scripts.DataSource
{
    public class UserManager
    {
        public IDbConnection dbConn;
        public Connection dsc;
        public UserManager()
        {
            dsc = new Connection();
        }

        public User GetUser()
        {
            Debug.Log("Getting User");
            IDbCommand dbCmd;
            IDataReader reader;

            dbConn = dsc.GetInstance();
            dbConn.Open();
            dbCmd = dbConn.CreateCommand();
            dbCmd.CommandText = "SELECT email AS 'Email', a.name AS 'User', " +
                "b.name AS 'Position', b.money AS 'Money' " +
                "FROM user a JOIN position b ON a.pos_id = b.id";
            reader = dbCmd.ExecuteReader();
            string email = "";
            string name = "";
            string position = "";
            int money = 0;

            while (reader.Read())
            {
                email = reader.GetString(0);
                name = reader.GetString(1);
                position = reader.GetString(2);
                money = reader.GetInt32(3);
            }

            dbConn.Close();
            return new User(email, name, position, money);
        }
    }
}
