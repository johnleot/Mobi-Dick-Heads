using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Classes.Tree
{
    class TreeFactory
    {
        private static Tree TREE;
        private static List<Tree> TREES;
        public static Tree GetTree()
        {
            if(TREE == null){
                TREE = new TreeImpl();
            }

            return TREE;
        }

        public static List<Tree> GetTrees()
        {
            if (TREES == null)
            {
            }
            return TREES;
        }
    }
}
