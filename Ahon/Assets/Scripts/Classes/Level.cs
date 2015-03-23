using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Classes
{
    public class Level
    {
        public Level(Location _location, List<Resource> _resource, 
            List<Solution> _solution, List<Calamity> _calamity)
        {
			Location = _location;
			Resources = _resource;
			Solutions = _solution;
			Calamities= _calamity;
        }

        public Location Location { get; set; }
        public List<Resource> Resources { get; set; }
        public List<Solution> Solutions { get; set; }
        public List<Calamity> Calamities { get; set; }

        public Level(string _location, string _desc)
        {
            StrLocation = _location;
            StrDescription = _desc;
        }

        public string StrLocation { get; set; }
        public string StrDescription { get; set; }
    }
}
