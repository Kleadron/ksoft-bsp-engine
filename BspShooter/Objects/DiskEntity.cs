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
                string value = null;
                keyvalues.TryGetValue("classname", out value);
                return value;
            }
        }

        public int SpawnFlags
        {
            get
            {
                string value = null;
                int flags = 0;

                if (keyvalues.TryGetValue("spawnflags", out value))
                {
                    int.TryParse(value, out flags);
                }
                
                return flags;
            }
        }

        public Vector3 Origin
        {
            get
            {
                string value = null;
                Vector3 v = Vector3.Zero;

                if (keyvalues.TryGetValue("origin", out value))
                {
                    string[] split = value.Split(' ');

                    if (split.Length == 3)
                    {
                        float.TryParse(split[0], out v.X);
                        float.TryParse(split[1], out v.Y);
                        float.TryParse(split[2], out v.Z);
                    }
                }

                return v;
            }
        }

        public Vector3 Angles
        {
            get
            {
                string value = null;
                Vector3 v = Vector3.Zero;

                // X Y Z euler angles
                if (keyvalues.TryGetValue("angles", out value))
                {
                    string[] split = value.Split(' ');

                    if (split.Length == 3)
                    {
                        float.TryParse(split[0], out v.X);
                        float.TryParse(split[1], out v.Y);
                        float.TryParse(split[2], out v.Z);
                    }
                }
                // Z rotation angle
                else if (keyvalues.TryGetValue("angle", out value))
                {
                    float.TryParse(value, out v.Z);
                }

                return v;
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
