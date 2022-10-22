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

        public static float DeltaTime(this GameTime gameTime)
        {
            return (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public static float TotalTime(this GameTime gameTime)
        {
            return (float)gameTime.TotalGameTime.TotalSeconds;
        }
    }
}
