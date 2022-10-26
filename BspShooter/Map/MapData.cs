using KSoft.Game.Primitives;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSoft.Game.Map
{
    public class MapData
    {
        // Shared vertices between polygons
        public List<Vector3> vertices = new List<Vector3>();

        public List<Polygon> polygons = new List<Polygon>();
    }
}
