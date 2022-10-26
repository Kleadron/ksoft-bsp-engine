using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSoft.Game.Primitives
{
    // Contains the plane and properties of a surface.
    // Idea: separate properties into SurfaceInfo? this way polygons can create new Surfaces and not duplicate SurfaceInfo
    public class Surface
    {
        //public List<Vector3> vertices = new List<Vector3>();

        // NOTE: XNA is opposite handed, so the default plane class needs the winding swapped.
        // If you are using a CORRECT math library, make sure point2 and point3 are flipped.
        //public Plane Plane => new Plane(vertices[0], vertices[2], vertices[1]); 
        //public Vector3 Origin => vertices.Aggregate(Vector3.Zero, (x, y) => x + y) / vertices.Count;

        // These values are now calculated when the surface is instantiated as surfaces are not meant to be modified.
        public Plane plane;
        public Vector3 origin;

        public bool nodraw = false;
        public bool clip = false;

        public Surface(string line)
        {
            string[] parts = line.Split(' ');

            // ( -16 -16 -64 ) ( -16 -15 -64 ) ( -16 -16 -63 )
            Vector3 p1 = new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
            Vector3 p2 = new Vector3(float.Parse(parts[6]), float.Parse(parts[7]), float.Parse(parts[8]));
            Vector3 p3 = new Vector3(float.Parse(parts[11]), float.Parse(parts[12]), float.Parse(parts[13]));

            //vertices.Add(p1);
            //vertices.Add(p2);
            //vertices.Add(p3);
            UpdatePlane(p1, p2, p3);

            // __TB_empty 0 0 0 1 1
            string texture = parts[15].ToUpper();
            if (texture == "CLIP" || texture == "NULL" || texture == "TRIGGER" || texture == "SKIP")
                nodraw = true;
            if (texture.StartsWith("*"))
                nodraw = true;
        }

        public Surface(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            //vertices.Add(p1);
            //vertices.Add(p2);
            //vertices.Add(p3);
            UpdatePlane(p1, p2, p3);
        }

        public Surface(float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3)
        {
            Vector3 p1 = new Vector3(x1, y1, z1);
            Vector3 p2 = new Vector3(x2, y2, z2);
            Vector3 p3 = new Vector3(x3, y3, z3);
            //vertices.Add(p1);
            //vertices.Add(p2);
            //vertices.Add(p3);
            UpdatePlane(p1, p2, p3);
        }

        public void UpdatePlane(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            // NOTE: XNA is opposite handed, so the default plane class needs the winding swapped.
            // If you are using a CORRECT math library, make sure point2 and point3 are flipped.
            plane = new Plane(p1, p3, p2);
            origin = (p1 + p2 + p3) / 3;
        }
    }
}
