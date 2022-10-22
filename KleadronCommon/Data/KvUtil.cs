using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KleadronCommon.Data
{
    public static class KvUtil
    {
        public static Vector3 ValToVec3(string value)
        {
            string[] split = value.Split(' ');
            return new Vector3(
                float.Parse(split[0]),
                float.Parse(split[1]),
                float.Parse(split[2])); 
        }

        public static string Vec3ToVal(Vector3 vector)
        {
            return vector.X + " " + vector.Y + " " + vector.Z;
        }
    }
}
