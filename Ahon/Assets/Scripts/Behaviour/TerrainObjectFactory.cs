using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Behaviour
{
    class TerrainObjectFactory : MonoBehaviour
    {
        //GameObject baranggayHall;
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

        private float[] x = new float[172];
        private float[] y = new float[172];
        private float[] z = new float[172];

        void Start()
        {


            x[0] = 68.82f;
            y[0] = 7.75f;
            z[0] = 235.76f;

            x[1] = 140.38f;
            y[1] = 11.45f;
            z[1] = 169.32f;

            for(int i = 0; i < 2; i++){
                //Debug.Log("- -");
                //Instantiate(baranggayHalls[i].BaranggayHallPrefab, new Vector3(x[i], y[i], z[i]), Quaternion.identity);
            }
            //Instantiate(baranggayHall_2, new Vector3(140.38f, 11.45f, 169.32f), Quaternion.identity);
            /*baranggayHall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baranggayHall.gameObject.tag = "Building";
            //baranggayHall.gameObject.layer = 11;
            //Debug.Log("Layet");
            baranggayHall.AddComponent<BoxCollider>();
            baranggayHall.AddComponent<Rigidbody>();
            baranggayHall.GetComponent<BoxCollider>().size = new Vector3(1.38f, 1.81f, 1.41f);
            baranggayHall.GetComponent<BoxCollider>().center = new Vector3(0.3f, 0.13f, 0.1f);
            baranggayHall.transform.position = new Vector3(68.82f, 7.75f, 235.76f);
            baranggayHall.transform.localScale = new Vector3(3.449825f, 3.449823f, 3.449823f);
                */
        }
    }
}
