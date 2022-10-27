using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSoft.Game.Primitives;
using Microsoft.Xna.Framework;

namespace KSoft.Game.Objects
{
    // Represents an entity's keyvalues and solids
    public class DiskEntity
    {
        public Dictionary<string, string> keyvalues = new Dictionary<string, string>();
        public List<Solid> solids = new List<Solid>();

        public string ClassName
        {
            get
            {
                if (keyvalues.ContainsKey("classname"))
                    return keyvalues["classname"];
                else
                    return null;
            }
        }

        public int SpawnFlags
        {
            get
            {
                int flags = 0;

                if (keyvalues.ContainsKey("spawnflags"))
                {
                    int.TryParse(keyvalues["spawnflags"], out flags);
                }
                
                return flags;
            }
        }

        public List<Polygon> CollectSolidPolygons()
        {
            List<Polygon> polygons = new List<Polygon>();

            foreach (Solid solid in solids)
                polygons.AddRange(solid.polygons);

            return polygons;
        }
    }
}
