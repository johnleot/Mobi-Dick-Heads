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
    public class SolutionManager : LevelManager
    {
        public List<Solution> GetSolutions(int level)
        {
            Debug.Log("Getting Solutions");
            IDbCommand dbCmd;
            IDataReader reader;

            dbConn = dsc.GetInstance();
            dbConn.Open();
            dbCmd = dbConn.CreateCommand();
            dbCmd.CommandText = "SELECT b.name AS 'SolutionName', b.amount AS 'SolutionAmount', " +
			"b.desc AS 'SolutionDescription', b.img AS 'SolutionUrl', " +
			"b.availability AS 'IsAvailable', b.type AS 'SolutionType' FROM sol_lev a " +
			"JOIN solution b ON a.sol_id = b.id WHERE a.lev_id = " + level ;

            reader = dbCmd.ExecuteReader();
            string name = "";
            int amount = 0;
            string description = "";
            string image = "";
            bool isAvailable = false;
            int type = 0;

            List<Solution> solutions = new List<Solution>();
            while (reader.Read())
            {
                name = reader.GetString(0);
                amount = reader.GetInt16(1);
                description = reader.GetString(2);
                image = reader.GetString(3);
                isAvailable = (reader.GetInt16(4) == 1) ? true : false;
                type = reader.GetInt16(5);

                Solution solution = new Solution(name, amount, description, 
                    image, isAvailable, type);
                solutions.Add(solution);
            }
            dbConn.Close();
            //return new Level(location, description);
            return solutions;
        }
    }
}
