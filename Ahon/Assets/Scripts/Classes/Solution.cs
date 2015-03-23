using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Classes
{
    public class Solution
    {
        public Solution(string _name, int _amount, string _description, string _image,
            bool _isAvailable, int _type)
        {
            Name = _name;
            Amount = _amount;
            Description = _description;
            Image = _image;
            IsAvailable = _isAvailable;
            Type = _type;
        }

        public string Name { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public bool IsAvailable { get; set; }
        public int Type { get; set; }
    }
}
