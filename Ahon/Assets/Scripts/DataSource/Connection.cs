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
            string p = "Ahon.db";
            string filePath = Application.persistentDataPath + "/" + p;

            if (dbConn == null)
            {
                if (!File.Exists(filePath))
                {
                    Debug.Log("Creating Ahon.db");
                    dbConn = (IDbConnection)new SqliteConnection("URI=file:" + filePath);
                    dbConn.Open(); // only open the connection if file is empty

                    TextAsset content = (TextAsset)Resources.Load("AhonScripts", typeof(TextAsset));
                    string sqlFile = content.text;
                    Debug.Log(sqlFile);
                    //string sqlFile = File.ReadAllText(@"Assets/Resources/AhonScripts.txt");
                    IDbCommand dbCmd = dbConn.CreateCommand();
                    dbCmd.CommandText = sqlFile;
                    Debug.Log("Populating Ahon.db");
                    dbCmd.ExecuteReader();
                    dbConn.Close();
                }
                else
                {
                    dbConn = (IDbConnection)new SqliteConnection("URI=file:" + filePath);
                }
            } 
            return dbConn;
        }

        void CloseConnection()
        {
            dbConn.Close();
        }
    }
}
