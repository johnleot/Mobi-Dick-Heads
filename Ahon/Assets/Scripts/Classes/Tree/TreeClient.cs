using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Classes.Tree
{
    class TreeClient
    {
        private Tree tree = TreeFactory.GetTree();

        public void DestroyTree(Tree _tree)
        {
            tree.DestroyTree(_tree);
        }
    }
}
