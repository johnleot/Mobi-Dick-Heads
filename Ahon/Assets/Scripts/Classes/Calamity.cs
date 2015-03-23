using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Classes
{
    public class Calamity
    {
        public Calamity(string _name, string _forecast, int _duration, int _timeofArrival,
            int _perCasualtyPeople, int _perCasualtyCrops)
        {
            Name = _name;
            Forecast = _forecast;
            Duration = _duration;
            TimeOfArrival = _timeofArrival;
            PerCasualtyPeople = _perCasualtyPeople;
            PerCasualtyCrops = _perCasualtyCrops;
        }

        public string Name { get; set; }
        public string Forecast { get; set; }
        public int Duration { get; set; }
        public int TimeOfArrival { get; set; }
        public int PerCasualtyPeople { get; set; }
        public int PerCasualtyCrops { get; set; }

    }
}
