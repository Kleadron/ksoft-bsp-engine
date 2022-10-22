using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSoft.Game.BSP
{
    public class Surface
    {
        public List<Vector3> vertices = new List<Vector3>();

        public Plane Plane => new Plane(vertices[0], vertices[2], vertices[1]);
        public Vector3 Origin => vertices.Aggregate(Vector3.Zero, (x, y) => x + y) / vertices.Count;

        public bool nodraw = false;

        public Surface(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            vertices.Add(p1);
            vertices.Add(p2);
            vertices.Add(p3);
        }

        public Surface(float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3)
        {
            Vector3 p1 = new Vector3(x1, y1, z1);
            Vector3 p2 = new Vector3(x2, y2, z2);
            Vector3 p3 = new Vector3(x3, y3, z3);
            vertices.Add(p1);
            vertices.Add(p2);
            vertices.Add(p3);
        }
    }
}
