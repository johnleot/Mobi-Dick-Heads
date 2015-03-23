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
    public class LocationManager : LevelManager
    {
        public Location GetLocation(int level)
        {
            Debug.Log("Getting Location");
            IDbCommand dbCmd;
            IDataReader reader;

            dbConn = dsc.GetInstance();
            dbConn.Open();
            dbCmd = dbConn.CreateCommand();
            dbCmd.CommandText = "SELECT b.name AS 'Location', a.desc as " + 
                "'LevelDescription', c.name AS 'Province', b.desc AS " +
                "'LocationDescription', b.population AS 'Population', "+
                "b.political_level AS 'PoliticalLevel' FROM level a " +
		        "JOIN area b ON a.area_id = b.id " +
		        "JOIN province c ON b.prov_id = c.id " +
		        "WHERE a.id = " + level;

            reader = dbCmd.ExecuteReader();
            string name = "";
            string levelDescription = "";
            string province = "";
            string locationDescription = "";
            int population = 0;
            int politicalLevel = 0;
           
            while (reader.Read())
            {
                name = reader.GetString(0);
                levelDescription = reader.GetString(1);
                province = reader.GetString(2);
                locationDescription = reader.GetString(3);
                population = reader.GetInt16(4);
                politicalLevel = reader.GetInt16(5);

            }
            dbConn.Close();

            return new Location(name, levelDescription, province, locationDescription, 
                population, politicalLevel);

        }
    }
}
