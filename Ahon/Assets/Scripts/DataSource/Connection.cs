using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Data.Sqlite;
using UnityEngine;
using System.IO;


namespace Assets.Scripts.DataSource
{
    public class Connection
    {
        public IDbConnection dbConn { get; set; }     
        public Connection() { }

        public IDbConnection GetInstance()
        {
            if (dbConn == null)
            {
                Debug.Log("Accessing Ahon.s3db");
                if (!File.Exists("Ahon.s3db"))
                {
                    Debug.Log("Creating Ahon.s3db");
                    dbConn = (IDbConnection)new SqliteConnection("Data Source=Ahon.s3db;Version=3");
                    dbConn.Open(); // only open the connection if file is empty
                    string sqlFile = File.ReadAllText(@"AhonScripts.sqlite");
                    IDbCommand dbCmd = dbConn.CreateCommand();
                    dbCmd.CommandText = sqlFile;
                    Debug.Log("Populating Ahon.s3db");
                    dbCmd.ExecuteReader();

                    dbConn.Close();
                }
                else
                {
                    dbConn = (IDbConnection)new SqliteConnection("Data Source=Ahon.s3db;Version=3");
                }
            } 
            return dbConn;
        }

        void CloseConnection()
        {
            dbConn.Close();
        }
        
        /*bool isTableExists(IDbConnection dbConn, String tableName)
        {
            IDbCommand dbCmd = dbConn.CreateCommand();
            string sqlQuery = "SELECT 1 FROM sqlite_master " + 
                "WHERE type='table' and name=" + tableName+ ";";
            dbCmd.CommandText = sqlQuery;
            IDataReader reader = dbCmd.ExecuteReader();
            int ctr = 0;

            while (reader.Read())
            {
                ctr = reader.GetInt32(0);
                Debug.Log("Count: " + ctr);
            }
            if(ctr ==  0){
                return false;
            }
            return false;
        }*/
    }
}
