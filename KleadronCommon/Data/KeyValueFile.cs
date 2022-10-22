using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KleadronCommon.Data
{
    public class KeyValueFile
    {
        Stream s;
        StreamReader r;
        int linenum;
        bool closed;

        public KeyValueFile(string path)
        {
            s = File.Open(path, FileMode.Open, FileAccess.Read);
            r = new StreamReader(s);
        }

        // Takes in a dictionary and fills with with keyvalues, returns true if the end of the file was not reached.
        public bool ReadBlock(ref Dictionary<string, string> kvs, bool retain = false)
        {
            if (closed)
                return false;

            if (!retain)
                kvs.Clear();

            string line = r.ReadLine();
            linenum++;

            // eof
            if (line == null)
            {
                return false;
            }
            else
            {
                line = line.Trim();
            }

            // skip
            if (line == "{")
            {
                line = r.ReadLine().Trim();
                linenum++;
            }
            else
            {
                // something has gone extremely wrong
                throw new Exception("Expected opening, got \"" + line + "\" at line" + linenum);
            }

            while (line != null)
            {
                // eat comments or empty lines
                if (line.Length == 0 || line.StartsWith("//"))
                {
                    line = r.ReadLine().Trim();
                    linenum++;
                    continue;
                }

                // end of block
                if (line == "}")
                    break;

                string key = null;
                string value = null;

                int stringStart = line.IndexOf('"') + 1;
                int stringEnd = line.IndexOf('"', stringStart);
                key = line.Substring(stringStart, stringEnd - stringStart);

                if (stringEnd + 2 > line.Length)
                {
                    value = "";
                }
                else
                {
                    stringStart = line.IndexOf('"', stringEnd + 2) + 1;
                    stringEnd = line.IndexOf('"', stringStart);
                    value = line.Substring(stringStart, stringEnd - stringStart);
                }

                kvs[key] = value;

                line = r.ReadLine().Trim();
                linenum++;
            }

            return true;
        }

        public void Close()
        {
            r.Close();
            s.Close();
            closed = true;
        }
    }
}
