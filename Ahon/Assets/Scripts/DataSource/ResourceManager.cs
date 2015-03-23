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
    public class ResourceManager : LevelManager
    {
        public List<Resource> GetResources(int level)
        {
            Debug.Log("Getting Resources");
            IDbCommand dbCmd;
            IDataReader reader;

            dbConn = dsc.GetInstance();
            dbConn.Open();
            dbCmd = dbConn.CreateCommand();
            dbCmd.CommandText = "SELECT c.name AS 'ResourceName', c.img AS " +
                "'ResourceImage' FROM level a JOIN lev_res b ON a.id = b.lev_id " +
		        "JOIN resource c ON b.res_id = c.id " +
		        "WHERE a.id =" + level;

            reader = dbCmd.ExecuteReader();
            string name = "";
            string image = "";
            List<Resource> resources = new List<Resource>();
            while (reader.Read())
            {
                name = reader.GetString(0);
                image = reader.GetString(1);

                Resource resource = new Resource(name, image);
                resources.Add(resource);
            }
            dbConn.Close();

            return resources;
        }
    }
}
