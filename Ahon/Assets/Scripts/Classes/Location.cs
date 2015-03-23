using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Classes
{
    public class Location
    {
        public Location(string _name, string _levelDescription, string _provinceName, string _locationDescription,
            int _population, int _politicalLevel)
        {
            Name = _name;
            LevelDescription = _levelDescription;
            ProvinceName = _provinceName;
            LocationDescription = _locationDescription;
            Population = _population;
            PoliticalLevel = _politicalLevel;
        }

        public string Name { get; set; }
        public string LevelDescription { get; set; }
        public string ProvinceName { get; set; }
        public string LocationDescription { get; set; }
        public int Population { get; set; }
        public int PoliticalLevel { get; set; }
    }
}
