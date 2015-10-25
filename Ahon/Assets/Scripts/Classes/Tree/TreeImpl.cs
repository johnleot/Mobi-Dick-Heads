using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Classes.Tree
{
    class TreeImpl: MonoBehaviour, Tree
    {
        private GameObject TreeGraphicalRep;

        public void DestroyTree(Tree tree)
        {
            Debug.Log(" - - Remove Tree - - ");
        }
    }
}
