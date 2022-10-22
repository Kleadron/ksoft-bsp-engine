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
            foreach(Surface surface in surfaces)
            {
                if (surface.nodraw)
                    continue;

                Polygon polygon = new Polygon(surface.Plane, 2048);

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
        }
    }
}
