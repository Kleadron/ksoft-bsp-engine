using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSoft.Common
{
    public static class KMath
    {
        // wraps a floating point number between -180 and 180
        public static void WrapDegrees180Signed(ref float num)
        {
            // works negative to positive
            num = num % 180;
        }

        // wraps a floating point number between 0 and 360
        public static void WrapDegrees360(ref float num)
        {
            // may result in a negative number
            num = num % 360;

            // make positive
            if (num < 0)
                num += 360;
        }
    }
}
