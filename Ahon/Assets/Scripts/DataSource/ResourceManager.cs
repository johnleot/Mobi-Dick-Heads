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
                "'ResourceImage', c.survivalTime AS 'SurvivalTime', " + 
				" c.maxNoOfOccupants AS 'MaxNoOfOccupants', " +
				" c.prefabtoInstantiate AS 'PrefabtoInstantiate', c.price AS 'Price', " + 
				" b.quantity AS 'Quantity'," +
				"d.position_x, d.position_y, d.position_z " +
				"FROM level a JOIN lev_res b ON a.id = b.lev_id " + 
		        "JOIN resource c ON b.res_id = c.id " +
				"JOIN _points d ON b.res_id = d.ref_asset " +
		        "WHERE (ResourceName <> 'Tree' AND ResourceName <> 'Coconut') AND a.id= " + level;

            reader = dbCmd.ExecuteReader();
            string name = "";
            string image = "";
            int survivalTime = 0;
			int maxNoOfOccupants = 0;
			string prefabToInstantiate = "";
			int price = 0;
            int quantity = 0;
            float position_x = 0;
            float position_y = 0;
            float position_z = 0;

            List<Resource> resources = new List<Resource>();
            while (reader.Read())
            {
                name = reader.GetString(0);
                image = reader.GetString(1);
                survivalTime = reader.GetInt16(2);
				maxNoOfOccupants = reader.GetInt16(3);
				prefabToInstantiate = reader.GetString(4);
				price = reader.GetInt16(5);
                quantity = reader.GetInt16(6);
                position_x = reader.GetFloat(7);
                position_y = reader.GetFloat(8);
                position_z = reader.GetFloat(9);
				/*
                Resource resource = new Resource(name, image, survivalTime,
                    quantity, position_x, position_y, position_z);
                resources.Add(resource);
                */
				Resource resource = new Resource(name, image, survivalTime, maxNoOfOccupants, prefabToInstantiate, price, 
				                                 quantity, position_x, position_y, position_z);
				resources.Add(resource);
            }
            dbConn.Close();

            return resources;
        }
    }
}
