using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSoft.Game.BSP
{
    public class Node
    {
        public Tree tree;
        public Polygon divider;

        public Node right;
        public Node left;

        public List<Polygon> polygons = new List<Polygon>();
    }
}
