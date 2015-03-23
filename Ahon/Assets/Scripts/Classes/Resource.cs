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

        public string Name { get; set; }
        public string Image { get; set; }
    }
}
