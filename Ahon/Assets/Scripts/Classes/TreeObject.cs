using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Classes.Tree;

namespace Assets.Scripts.Classes
{
    class TreeObject : MonoBehaviour
    {
        //TreeClient trees[] = {};
        void OnTriggerEnter(Collider other)
        {
            //Debug.Log("Entered " + other.gameObject.name);
           // Destroy(other.gameObject);
            if(other.gameObject.name != "WaterSurface"){
                /*String line = other.gameObject.name + "\t x " + other.gameObject.transform.position.x + "\t" +
                              "y " + other.gameObject.transform.position.y + "\t" +
                              "z " + other.gameObject.transform.position.z + "\t" +
                              "x " + other.gameObject.transform.localScale.x + "\t" +
                              "y " + other.gameObject.transform.localScale.y + "\t" +
                              "z " + other.gameObject.transform.localScale.z + "\n";
                System.IO.File.AppendAllText(@"C:\Users\Kana Antonio\Desktop\Positions.txt", line);
                */
                Destroy(other.gameObject);
            }
            
        }

        void OnTriggerStay(Collider other)
        {
            //Debug.Log("Within " + other);
            

        }
        void OnTriggerExit(Collider other)
        {
            Debug.Log("Exited");
        }
    }
}
