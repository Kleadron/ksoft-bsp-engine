﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KSoft.Game.BSP
{
    public class Polygon
    {
        public List<Vector3> vertices;
        //public Plane plane;
        //public Vector3 origin;

        public Plane Plane => new Plane(vertices[0], vertices[2], vertices[1]);
        public Vector3 Origin => vertices.Aggregate(Vector3.Zero, (x, y) => x + y) / vertices.Count;

        public Polygon(Plane plane, float radius = 1000000)
        {
            // Get aligned up and right axes to the plane
            Vector3 direction = plane.GetClosestAxisToNormal();
            Vector3 tempV = direction == Vector3.UnitZ ? -Vector3.UnitY : -Vector3.UnitZ;
            Vector3 up = tempV.SledgeCross(plane.Normal).SafeNormalise();
            Vector3 right = plane.Normal.SledgeCross(up).SafeNormalise();

            Vector3 planePoint = plane.GetPointOnPlane();

            List<Vector3> verts = new List<Vector3>()
            {
                planePoint + right + up, // Top right
                planePoint - right + up, // Top left
                planePoint - right - up, // Bottom left
                planePoint + right - up, // Bottom right
            };

            var origin = verts.Aggregate(Vector3.Zero, (x, y) => x + y) / verts.Count;
            vertices = verts.Select(x => (x - origin).SafeNormalise() * radius + origin).ToList();
        }

        public void Shift(Vector3 offset)
        {
            for(int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = vertices[i] + offset;
            }
        }

        public Polygon(params Vector3[] verts)
        {
            vertices = verts.ToList();

            if (vertices.Count <= 2)
                throw new Exception("Cannot create a polygon with less than 3 vertices! >:(");



            //plane = new Plane(vertices[0], vertices[1], vertices[2]);

            //CalcOrigin();
        }

        public Polygon(IEnumerable<Vector3> verts)
        {
            //this.p1 = p1;
            //this.p2 = p2;
            //this.p3 = p3;

            vertices = verts.ToList();

            if (vertices.Count <= 2)
                throw new Exception("Cannot create a polygon with less than 3 vertices! >:(");

           

            //plane = new Plane(vertices[0], vertices[1], vertices[2]);

            //CalcOrigin();
        }

        public Polygon(params float[] coords)
        {
            //this.p1 = p1;
            //this.p2 = p2;
            //this.p3 = p3;
            int numverts = coords.Length / 3;

            if (numverts <= 2)
                throw new Exception("Cannot create a polygon with less than 3 vertices! >:(");

            vertices = new List<Vector3>(numverts);

            for(int i = 0; i < numverts; i++)
            {
                int coordI = i * 3;
                vertices.Add(new Vector3(coords[coordI], coords[coordI + 1], coords[coordI + 2]));
            }

            //plane = new Plane(vertices[0], vertices[1], vertices[2]);

            //CalcOrigin();
        }

        //void CalcOrigin()
        //{
        //    int numverts = vertices.Count;
        //    origin = Vector3.Zero;
        //    for (int i = 0; i < numverts; i++)
        //        origin += vertices[i];
        //    origin /= numverts;
        //}

        public PolySide ClassifyPoint(Vector3 point)
        {
            //float dot = Vector3.Dot(point, plane.Normal) - plane.D;
            float dot = Plane.DotCoordinate(point);
            if (dot == 0)
                return PolySide.Coinciding;
            else if (dot < 0)
                return PolySide.Behind;
            else
                return PolySide.Infront;
        }

        public bool PolygonInfront(Polygon target)
        {
            for (int i = 0; i < target.vertices.Count; i++)
            {
                if (ClassifyPoint(target.vertices[i]) != PolySide.Infront)
                    return false;
            }
            return true;
        }

        public static bool IsConvexSet(Polygon[] polygons)
        {
            for(int i = 0; i < polygons.Length; i++)
            {
                for (int j = 0; j < polygons.Length; j++)
                {
                    if (i == j)
                        continue;

                    bool infront = polygons[i].PolygonInfront(polygons[j]);

                    Console.WriteLine("Compare " + i + " to " + j + ": " + infront);

                    // all polygons must face eachother
                    if (!infront)
                        return false;
                }
            }

            return true;
        }

        public PolySide CalculateSide(Polygon target)
        {
            int positives = 0, negatives = 0;

            for (int i = 0; i < target.vertices.Count; i++)
            {
                PolySide side = ClassifyPoint(target.vertices[i]);

                if (side == PolySide.Infront)
                    positives++;
                else if (side == PolySide.Behind)
                    negatives++;
            }

            if (positives > 0 && negatives == 0)
                return PolySide.Infront;
            else if (positives == 0 && negatives > 0)
                return PolySide.Behind;
            else if (positives == 0 && negatives == 0)
                return PolySide.Coinciding;
            else
                return PolySide.Spanning;
        }

        /// <summary>
        /// Splits this polygon by a clipping plane, returning the back and front planes.
        /// The original polygon is not modified.
        /// </summary>
        /// <param name="clip">The clipping plane</param>
        /// <param name="back">The back polygon</param>
        /// <param name="front">The front polygon</param>
        /// <returns>True if the split was successful</returns>
        public bool Split(Plane clip, out Polygon back, out Polygon front)
        {
            return Split(clip, out back, out front, out _, out _);
        }

        /// <summary>
        /// Splits this polygon by a clipping plane, returning the back and front planes.
        /// The original polygon is not modified.
        /// </summary>
        /// <param name="clip">The clipping plane</param>
        /// <param name="back">The back polygon</param>
        /// <param name="front">The front polygon</param>
        /// <param name="coplanarBack">If the polygon rests on the plane and points backward, this will not be null</param>
        /// <param name="coplanarFront">If the polygon rests on the plane and points forward, this will not be null</param>
        /// <returns>True if the split was successful</returns>
        public bool Split(Plane clip, out Polygon back, out Polygon front, out Polygon coplanarBack, out Polygon coplanarFront)
        {
            const float epsilon = Extensions.Epsilon;

            var distances = vertices.Select(clip.DotCoordinate).ToList();

            int cb = 0, cf = 0;
            for (var i = 0; i < distances.Count; i++)
            {
                if (distances[i] < -epsilon)
                    cb++;
                else if (distances[i] > epsilon)
                    cf++;
                else
                    distances[i] = 0;
            }

            // Check non-spanning cases
            if (cb == 0 && cf == 0)
            {
                // Co-planar
                back = front = coplanarBack = coplanarFront = null;
                if (Vector3.Dot(Plane.Normal, clip.Normal) > 0)
                    coplanarFront = this;
                else
                    coplanarBack = this;
                return false;
            }
            else if (cb == 0)
            {
                // All vertices in front
                back = coplanarBack = coplanarFront = null;
                front = this;
                return false;
            }
            else if (cf == 0)
            {
                // All vertices behind
                front = coplanarBack = coplanarFront = null;
                back = this;
                return false;
            }

            // Get the new front and back vertices
            var backVerts = new List<Vector3>();
            var frontVerts = new List<Vector3>();

            for (var i = 0; i < vertices.Count; i++)
            {
                var j = (i + 1) % vertices.Count;

                Vector3 s = vertices[i], e = vertices[j];
                float sd = distances[i], ed = distances[j];

                if (sd <= 0)
                    backVerts.Add(s);
                if (sd >= 0)
                    frontVerts.Add(s);

                if ((sd < 0 && ed > 0) || (ed < 0 && sd > 0))
                {
                    var t = sd / (sd - ed);
                    var intersect = s * (1 - t) + e * t;

                    backVerts.Add(intersect);
                    frontVerts.Add(intersect);
                }
            }

            back = new Polygon(backVerts.Select(x => new Vector3(x.X, x.Y, x.Z)));
            front = new Polygon(frontVerts.Select(x => new Vector3(x.X, x.Y, x.Z)));
            coplanarBack = coplanarFront = null;

            return true;
        }
    }
}