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
    public class LevelManager
    {
        public IDbConnection dbConn;
        public Connection dsc;

        public LevelManager()
        {
            dsc = new Connection();
        }

        public Level GetLevelIntroduction(int level){
            Debug.Log("Getting Level Introduction");
            IDbCommand dbCmd;
            IDataReader reader;

            dbConn = dsc.GetInstance();
            dbConn.Open();
            dbCmd = dbConn.CreateCommand();
            dbCmd.CommandText = "SELECT b.name AS 'Location', a.desc as " +
            "'LevelDescription' FROM level a JOIN area b ON a.area_id = b.id " +
            "WHERE a.id = " + level;

            reader = dbCmd.ExecuteReader();
            string location = "";
            string description = "";

            while (reader.Read())
            {
                location = reader.GetString(0);
                description = reader.GetString(1);
            }

            dbConn.Close();
            return new Level(location, description);
        }

        public Level GetLevel(int level)
        {
            Debug.Log("Getting Level for Gameplay");
            List<Calamity> calamities = new CalamityManager().GetCalamities(level);
            List<Resource> resources = new ResourceManager().GetResources(level);
            List<Solution> solutions = new SolutionManager().GetSolutions(level);
            Location location = new LocationManager().GetLocation(level);

            return new Level(location, resources, solutions, calamities);
        }
    }
}
