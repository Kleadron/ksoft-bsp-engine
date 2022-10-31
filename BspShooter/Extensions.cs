using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSoft.Game
{
    public static class Extensions
    {
        public const float Epsilon = 0.0001f;
        public const float MapVertexRound = 0.0625f;

        /// <summary>
        /// Gets the axis closest to the normal of this plane
        /// </summary>
        /// <returns>Vector3.UnitX, Vector3.UnitY, or Vector3.UnitZ depending on the plane's normal</returns>
        public static Vector3 GetClosestAxisToNormal(this Plane plane)
        {
            // VHE prioritises the axes in order of X, Y, Z.
            Vector3 norm = plane.Normal.Absolute();

            if (norm.X >= norm.Y && norm.X >= norm.Z)
                return Vector3.UnitX;
            if (norm.Y >= norm.Z)
                return Vector3.UnitY;
            return Vector3.UnitZ;
        }

        public static Vector3 Absolute(this Vector3 v)
        {
            return new Vector3(Math.Abs(v.X), Math.Abs(v.Y), Math.Abs(v.Z));
        }

        public static Vector3 GetPointOnPlane(this Plane plane)
        {
            return plane.Normal * plane.D;
        }

        // Left handed version of Vector3.Cross (I think)
        public static Vector3 SledgeCross(this Vector3 v, Vector3 that)
        {
            var xv = v.Y * that.Z - v.Z * that.Y;
            var yv = v.Z * that.X - v.X * that.Z;
            var zv = v.X * that.Y - v.Y * that.X;
            return new Vector3(xv, yv, zv);
        }

        public static Vector3 SafeNormalise(this Vector3 v)
        {
            float len = v.Length();
            return Math.Abs(len) < Epsilon ? new Vector3(0, 0, 0) : new Vector3(v.X / len, v.Y / len, v.Z / len);
        }

        public static bool EquivalentTo(this Vector3 v, Vector3 test, float delta = 0.001f)
        {
            var xd = Math.Abs(v.X - test.X);
            var yd = Math.Abs(v.Y - test.Y);
            var zd = Math.Abs(v.Z - test.Z);
            return (xd < delta) && (yd < delta) && (zd < delta);
        }

        ///  <summary>Finds if the given point is above, below, or on the plane.</summary>
        ///  <param name="co">The Vector3 to test</param>
        /// <param name="epsilon">Tolerance value</param>
        /// <returns>
        ///  value == -1 if Vector3 is below the plane<br />
        ///  value == 1 if Vector3 is above the plane<br />
        ///  value == 0 if Vector3 is on the plane.
        /// </returns>
        public static int OnPlane(this Plane p, Vector3 co, double epsilon = 0.0001d)
        {
            //eval (s = Ax + By + Cz + D) at point (x,y,z)
            //if s > 0 then point is "above" the plane (same side as normal)
            //if s < 0 then it lies on the opposite side
            //if s = 0 then the point (x,y,z) lies on the plane
            var res = p.DotCoordinate(co);
            if (Math.Abs(res) < epsilon)
                return 0;
            if (res < 0)
                return -1;
            return 1;
        }

        // needs to be a return function
        public static Vector3 RoundToStep(this Vector3 v, float step)
        {
            v.X = (float)Math.Round(v.X / step) * step;
            v.Y = (float)Math.Round(v.Y / step) * step;
            v.Z = (float)Math.Round(v.Z / step) * step;

            return v;
        }

        public static float DeltaTime(this GameTime gameTime)
        {
            return (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public static float TotalTime(this GameTime gameTime)
        {
            return (float)gameTime.TotalGameTime.TotalSeconds;
        }

        public static void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
    }
}
