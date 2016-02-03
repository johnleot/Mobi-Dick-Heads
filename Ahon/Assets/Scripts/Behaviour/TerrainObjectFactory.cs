using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.DataSource;
using Assets.Scripts.Classes;

namespace Assets.Scripts.Behaviour
{
    class TerrainObjectFactory : MonoBehaviour
    {
        GameObject baranggayHall;
        public ResourceManager rm;
        public Resource r;
        public Transform BaranggayHall;
        public Transform BlueHouse;
        public Transform Church;
        public Transform CityHall;
        public Transform NipaHut;
        public Transform OrangeHouse;
        public Transform Pabahay;
        public Transform School;

        public Transform Palay;
        public Transform Danggit;
        public Transform Coconut;
        public Transform Tree;

        void Start()
        {

            rm = new ResourceManager();
            List<Resource> list = rm.GetPabahayPoints(1);

            for(int i = 0; i < list.Count; i++){
                float x = list[i].X;
                float y = list[i].Y;
                float z = list[i].Z;
                
                string name = list[i].Name;
                string prefab = list[i].Image;
                int numOcc = list[i].MaxNoOfOccupants;
                
                switch (name){
                    case "Blue House":
                        Instantiate(BlueHouse, new Vector3(x, y, z), Quaternion.identity).name = name + "," + numOcc;
                        break;
                    case "Nipahut":
                        Instantiate(NipaHut, new Vector3(x, y, z), Quaternion.identity).name = name+","+ numOcc;
                        break;
                    case "Pabahay":
                        Instantiate(Pabahay, new Vector3(x, y, z), Quaternion.identity).name = name + "," + numOcc;
                        break;
                    case "Orange House":
                        Instantiate(OrangeHouse, new Vector3(x, y, z), Quaternion.identity).name = name + "," + numOcc;
                        break;
                }
            }

            //baranggayHall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //baranggayHall.gameObject.tag = "Building";
            //baranggayHall.gameObject.layer = 11;
            //Debug.Log("Layet");
            //baranggayHall.AddComponent<BoxCollider>();
            //baranggayHall.AddComponent<Rigidbody>();
            //baranggayHall.GetComponent<BoxCollider>().size = new Vector3(1.38f, 1.81f, 1.41f);
            //baranggayHall.GetComponent<BoxCollider>().center = new Vector3(0.3f, 0.13f, 0.1f);
            //baranggayHall.transform.position = new Vector3(68.82f, 7.75f, 235.76f);
            //baranggayHall.transform.localScale = new Vector3(3.449825f, 3.449823f, 3.449823f);
                
        }
    }
}
