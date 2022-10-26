using KSoft.Game.Primitives;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSoft.Game.BSP
{
    public class Node
    {
        //public Tree tree; // I don't think this needs a reference to the tree.
        //public Polygon divider; // It'd be more efficient to use a plane here.

        public Plane plane; // The dividing plane between the front and back halves of the node.

        public Node front; // Node that resides in front of the plane.
        public Node back; // Node that resides behind of the plane.

        public List<Polygon> polygons = new List<Polygon>(); // Polygon content of this node.
    }
}
