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
    class CalamityManager : LevelManager
    {
        public List<Calamity> GetCalamities(int level)
        {
            Debug.Log("Getting Calamities");
            IDbCommand dbCmd;
            IDataReader reader;

            dbConn = dsc.GetInstance();
            dbConn.Open();
            dbCmd = dbConn.CreateCommand();
            dbCmd.CommandText = "SELECT b.name AS 'CalamityName', a.forecast AS 'Forecast', " + 
                "a.duration AS 'CalamityDuration', a.time AS 'CalamityTimeOfArrival', " +
                "a.casualty_people AS 'CasualtyPeople', a.casualty_crops " + 
                "AS 'CasualtyCrops' FROM lev_cal a JOIN calamity b ON a.cal_id = b.id " +
                "WHERE a.lev_id =" + level;

            reader = dbCmd.ExecuteReader();
            string name = "";
            string forecast = "";
            int duration = 0;
            int time = 0;
            int casualtyPeople = 0;
            int casualtyCrops = 0;

            List<Calamity> calamities = new List<Calamity>();
            while (reader.Read())
            {
                name = reader.GetString(0);
                forecast = reader.GetString(1);
                duration = reader.GetInt16(2);
                time = reader.GetInt16(3);
                casualtyPeople = reader.GetInt16(4);
                casualtyCrops = reader.GetInt16(5);

                Calamity calamity = new Calamity(name, forecast, duration, time, casualtyPeople, casualtyCrops);
                calamities.Add(calamity);
            }
            dbConn.Close();
            //return new Level(location, description);
            return calamities;
        }

    }
}
