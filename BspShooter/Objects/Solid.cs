using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSoft.Game.Primitives;
using Microsoft.Xna.Framework;

namespace KSoft.Game.Objects
{
    // A solid represents a convex hull defined by plane surfaces.
    public class Solid
    {
        public List<Surface> surfaces; // Raw surface information used to generate the polygons.

        //public List<Vector3> vertices;
        public List<Polygon> polygons; // 3D polygons.
        
        public Solid()
        {
            surfaces = new List<Surface>();
            //vertices = new List<Vector3>();
            polygons = new List<Polygon>();
        }

        public Solid(List<Surface> sides)
        {
            surfaces = sides;
            //vertices = new List<Vector3>();
            polygons = new List<Polygon>();
            BuildPolygons();
        }

        public Solid(params Surface[] sides)
        {
            this.surfaces = sides.ToList();
            polygons = new List<Polygon>();
            BuildPolygons();
        }

        public void BuildPolygons()
        {
            // invalid shape
            if (surfaces.Count < 4)
                return;

            polygons.Clear();

            foreach(Surface surface in surfaces)
            {
                //if (surface.nodraw)
                //    continue;

                // the plane size argument may need to be configurable or adaptable to map or largest brush size
                Polygon polygon = new Polygon(surface, 4096);

                Vector3 planeOrigin = surface.origin;
                Vector3 polyOrigin = polygon.origin;
                Vector3 diff = planeOrigin - polyOrigin;

                polygon.Shift(diff);

                foreach (Surface intersector in surfaces)
                {
                    if (surface == intersector)
                        continue;

                    Polygon back, front;
                    bool intersected = polygon.Split(intersector.plane, out back, out front);
                    if (intersected && back != null)
                    {
                        polygon = back;
                    }
                }

                // snap vertices to nice cordinates (this may be a bad idea)
                polygon.RoundVertices();
                polygons.Add(polygon);
            }

            // Ensure all the faces point outwards (I don't know if this is necessary but this has never fired, and probably never will)
            //var origin = polygons.Aggregate(Vector3.Zero, (x, y) => x + y.origin) / polygons.Count;
            //for (var i = 0; i < polygons.Count; i++)
            //{
            //    var face = polygons[i];
            //    if (face.surface.plane.OnPlane(origin) >= 0)
            //    {
            //        //polygons[i] = new Polygon(face.vertices.Reverse());
            //        polygons[i].vertices.Reverse();
            //    }
            //}
        }
    }
}
