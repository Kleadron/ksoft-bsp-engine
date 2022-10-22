using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Threading;
#if WINDOWS
using Microsoft.CSharp;
using System.Windows.Forms;
#endif

namespace KleadronCommon
{
    public static class Misc
    {
        private const int smoothing = 20;
        private static Vector2[] memoryUsageOverTime = new Vector2[smoothing];
        private static int mUOT_iteration = 0;
        private static float lastMemory;
        private static float updateSpeed = 0.3f;
        private static float updateTimer = 0;
        private static float displayedNumber = 0f;
        private static string clipboard = "";

        public static void SetClipboardText(string text)
        {
#if WINDOWS
            clipboard = text;
            Thread t = new Thread(setClipboard);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            while (t.IsAlive)
            {

            }
#endif
        }

        public static String GetClipboardText()
        {
#if WINDOWS
            Thread t = new Thread(getClipboard);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            while (t.IsAlive)
            {
            }
            return clipboard;
#else
            return "";
#endif
        }

        // do this trash because ?.Invoke non null invocation doesnt exist before c# 6.0
        public static T1 SafeInvoke<T1>(this Func<T1> func)
        {
            if (func != null) return func();

            return default(T1);
        }

        // do this trash because ?.Invoke non null invocation doesnt exist before c# 6.0
        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 arg0, T2 arg1)
        {
            if (action != null) action(arg0, arg1);
        }

#if WINDOWS
        [STAThread]
        private static void setClipboard()
        {
            if (clipboard != null)
            {
                Clipboard.SetText(clipboard, TextDataFormat.Text);
            }
        }

        [STAThread]
        private static void getClipboard()
        {
            if (Clipboard.ContainsText())
            {
                clipboard = Clipboard.GetText(TextDataFormat.Text);
            }
        }
#endif

#if WINDOWS
        private static void Throw(CompilerError e)
        {
            throw new Exception("Compilation failed: " + e.ErrorText + " L" + e.Line + "C" + e.Column + " #" + e.ErrorNumber);
        }

        // COMPILES C# ON THE GO!!!!
        public static object Compile(string source, string method, string path)
        {
            Dictionary<string, string> providerOptions = new Dictionary<string, string>() { {"CompilerVersion", "v3.5"} };

            CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions);
            CompilerParameters compilerParams = new CompilerParameters() { GenerateInMemory = true, GenerateExecutable = false };
            CompilerResults results = provider.CompileAssemblyFromSource(compilerParams, source);

            results.Errors.Cast<CompilerError>().ToList().ForEach(Throw);

            object o = results.CompiledAssembly.CreateInstance(path);
            dynamic d = Convert.ChangeType(o, o.GetType());

            object ret = o.GetType().GetMethod(method).Invoke(o, null);
            return ret;
        }
#endif

        public static void DrawShadowedString(this SpriteBatch sb, SpriteFont spriteFont, string text, Vector2 position, Color color, Vector2 shadowOffset, Color shadowColor)
        {
            sb.DrawString(spriteFont, text, position + shadowOffset, shadowColor);
            sb.DrawString(spriteFont, text, position, color);
        }

        public static string Multiply(this string str, int i) {
            return String.Concat(Enumerable.Repeat(str, i));
        }

        public static string RemoveAt(this string str, int i)
        {
            return str.Remove(i, 1);
        }

        public static void Ensure<T1, T2>(ref Dictionary<T1, T2> d, T1 k) where T2 : new()
        {
            if (!d.ContainsKey(k))
            {
                d.Add(k, new T2());
            }
        }

        public static Stack<T> Flip<T>(this Stack<T> st)
        {
            return new Stack<T>(st);
        }

        public static Stack<T> ToStack<T>(this T[] arr)
        {
            Stack<T> stack = new Stack<T>();
            foreach (T t in arr)
            {
                stack.Push(t);
            }

            return stack;
        }

        public static Stack<T> ToStack<T>(this List<T> list)
        {
            Stack<T> stack = new Stack<T>();
            foreach(T t in list) {
                stack.Push(t);
            }

            return stack;
        }

        public static bool IsEven(this int i)
        {
            if (i % 2 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static float GetAverageMemoryPerSecond(GameTime time)
        {
            float memory = (GC.GetTotalMemory(false) / 1024);

            

            if (mUOT_iteration < smoothing)
            {
                memoryUsageOverTime[mUOT_iteration] = new Vector2(memory - lastMemory, (float)time.ElapsedGameTime.TotalSeconds);
                mUOT_iteration++;
            }
            else
            {
                mUOT_iteration = 0;
            }

            float total = 0f;
            foreach (Vector2 v in memoryUsageOverTime)
            {
                total += v.X;
            }
            total /= smoothing;

            lastMemory = memory;

            if (updateTimer <= 0)
            {
                updateTimer = updateSpeed;
                displayedNumber = total * 60;
            }
            else
            {
                updateTimer -= 1 * (float)time.ElapsedGameTime.TotalSeconds;
            }

            return displayedNumber;
        }

        // this one is simpler and actually works
        public static List<int> AllIndexesOf(this string str, char substring)
        {
            List<int> indexes = new List<int>();

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == substring)
                {
                    indexes.Add(i);
                }
            }

            return indexes;
        }

        // gets the angle from the source and look locations
        public static float YawDegreesFromLocations(Vector3 lookingLocation, Vector3 lookAtLocation)
        {
            float degrees = -MathHelper.ToDegrees((float)Math.Atan2(lookAtLocation.Z - lookingLocation.Z, lookAtLocation.X - lookingLocation.X));

            degrees -= 90f; // correction

            //MathHelper.wr
            degrees %= 360f; // wrap angle

            return degrees;
        }

        // untested
        public static float YawRadiansFromLocations(Vector3 lookingLocation, Vector3 lookAtLocation)
        {
            float angle = -((float)Math.Atan2(lookAtLocation.Z - lookingLocation.Z, lookAtLocation.X - lookingLocation.X));
            angle -= MathHelper.PiOver4; // correction
            //angle = MathHelper.WrapAngle(angle);
            angle %= MathHelper.Pi; // wrap angle

            return angle;
        }
    }
}
