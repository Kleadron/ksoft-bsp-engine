using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KSoft.Game.BSP
{
    public class Solid
    {
        public List<Surface> surfaces;
        public List<Polygon> polygons;
        
        public Solid()
        {
            surfaces = new List<Surface>();
            polygons = new List<Polygon>();
        }

        public Solid(params Surface[] sides)
        {
            this.surfaces = sides.ToList();
            polygons = new List<Polygon>();
            CreatePolygons();
        }

        void CreatePolygons()
        {
            // invalid shape
            if (surfaces.Count < 4)
                return;

            foreach(Surface surface in surfaces)
            {
                if (surface.nodraw)
                    continue;

                // the plane size argument may need to be configurable or adaptable to map or largest brush size
                Polygon polygon = new Polygon(surface.Plane, 4096);

                Vector3 planeOrigin = surface.Origin;
                Vector3 polyOrigin = polygon.Origin;
                Vector3 diff = planeOrigin - polyOrigin;

                polygon.Shift(diff);

                foreach (Surface intersector in surfaces)
                {
                    if (surface == intersector)
                        continue;

                    Polygon back, front;
                    bool intersected = polygon.Split(intersector.Plane, out back, out front);
                    if (intersected && back != null)
                    {
                        polygon = back;
                    }
                }

                polygons.Add(polygon);
            }

            // Ensure all the faces point outwards
            var origin = polygons.Aggregate(Vector3.Zero, (x, y) => x + y.Origin) / polygons.Count;
            for (var i = 0; i < polygons.Count; i++)
            {
                var face = polygons[i];
                if (face.Plane.OnPlane(origin) >= 0)
                {
                    //polygons[i] = new Polygon(face.vertices.Reverse());
                    polygons[i].vertices.Reverse();
                }
            }
        }
    }
}
