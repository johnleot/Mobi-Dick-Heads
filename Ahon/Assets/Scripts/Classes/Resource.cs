using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Classes
{
    public class Resource
    {
        public Resource(string _name, string _image)
        {
            Name = _name;
            Image = _image;
        }

        public Resource(string _name, string _image, string _type, int _x, int _y, int _z)
        {
            Name = _name;
            Image = _image;
            Type = _type;
        }

        public Resource(string _name, string _image, int _survivalTime, int _quantity, float _x, float _y, float _z)
        {
            Name = _name;
            Image = _image;
            SurvivalTime = _survivalTime;
            Quantity = _quantity;
            X = _x;
            Y = _y;
            Z = _z;
        }

		public Resource(string _name, string _image, int _survivalTime, 
		                int _maxNoOfOccupants, string _prefabToInstantiate, int _price,
		                int _quantity, float _x, float _y, float _z)
		{
			Name = _name;
			Image = _image;
			SurvivalTime = _survivalTime;
			MaxNoOfOccupants = _maxNoOfOccupants;
			PrefabToInstantiate = _prefabToInstantiate;
			Price = _price;
			Quantity = _quantity;
			X = _x;
			Y = _y;
			Z = _z;
		}

        public string Name { get; set; }
        public string Image { get; set; }
        public string Type { get; set; }
        public int SurvivalTime { get; set; }
		public int MaxNoOfOccupants { get; set; }
		public string PrefabToInstantiate { get; set; }
		public int Price { get; set; }
        public int Quantity { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}
