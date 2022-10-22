using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace KleadronCommon.Data
{
    public struct WfVertex
    {
        public int positionIndex;
        public int uvIndex;
        public int normalIndex;

        public WfVertex(int positionIndex, int uvIndex, int normalIndex)
        {
            this.positionIndex = positionIndex;
            this.uvIndex = uvIndex;
            this.normalIndex = normalIndex;
        }

        public override string ToString()
        {
            return "P" + positionIndex + " T" + uvIndex + " N" + normalIndex;
        }
    }

    public class WfFace
    {
        // per-face because groups can use multiple materials
        public int materialIndex;

        // associated vertices
        public List<WfVertex> vertices = new List<WfVertex>();
    }

    public class WfObject
    {
        // object name
        public string name;

        // associated materials
        public List<int> materials = new List<int>();

        // associated faces
        public List<WfFace> faces = new List<WfFace>();

        public override string ToString()
        {
            return name;
        }
    }

    // if I had a dollar for every time I wrote an OBJ importer...
    public class WavefrontFile
    {
        // file stuff
        Stream s;
        StreamReader r;
        int linenum;
        public readonly string filename;
        //bool closed;

        // strings
        public List<string> materialNames = new List<string>();

        // vertex data
        public List<Vector3> v_positions = new List<Vector3>() { Vector3.Zero };
        public List<Vector2> v_uvs = new List<Vector2>() { Vector2.Zero };
        public List<Vector3> v_normals = new List<Vector3>() { Vector3.Zero };

        // organization
        //public List<WfGroup> groups = new List<WfGroup>();
        public List<WfObject> objects = new List<WfObject>();

        // state
        WfObject currentObject;
        int currentMatIndex = -1;

        public string mtlPath;

        // constructor
        public WavefrontFile(string path)
        {
            s = File.Open(path, FileMode.Open, FileAccess.Read);
            r = new StreamReader(s);

            filename = path;
            ReadFile();

            r.Close();
            s.Close();
        }

        public WfObject GetObjectByName(string name)
        {
            foreach(WfObject obj in objects)
            {
                if (obj.name == name)
                    return obj;
            }
            return null;
        }

        public override string ToString()
        {
            return filename;
        }

        // processing
        void ReadFile()
        {
            // read to end of file
            while (true)
            {
                string line = r.ReadLine();
                linenum++;

                // eol
                if (line == null)
                    break;

                line = line.Trim();

                // empty line
                if (line.Length == 0)
                    continue;

                // comment
                if (line.StartsWith("#"))
                    continue;

                // get the first word and command content
                int firstSpace = line.IndexOf(' ');

                // command has no content
                if (firstSpace == -1)
                    continue;

                string command = line.Substring(0, firstSpace);
                string content = line.Substring(firstSpace+1);

                ProcessCommand(command, content);
            }
        }

        Vector3 ToV3(string content)
        {
            string[] split = content.Split(' ');

            Vector3 v = new Vector3(
                float.Parse(split[0]),
                float.Parse(split[1]),
                float.Parse(split[2]));

            return v;
        }

        Vector2 ToV2(string content)
        {
            string[] split = content.Split(' ');

            Vector2 v = new Vector2(
                float.Parse(split[0]),
                float.Parse(split[1]));

            return v;
        }

        WfFace MakeFace(string content)
        {
            WfFace face = new WfFace();
            face.materialIndex = currentMatIndex;

            string[] v_contents = content.Split(' ');

            foreach(string vc in v_contents)
            {
                string[] vertContent = vc.Split('/');
                WfVertex vert = new WfVertex();

                vert.positionIndex = int.Parse(vertContent[0]);

                if (vertContent.Length > 1 && vertContent[1].Length > 0)
                    vert.uvIndex = int.Parse(vertContent[1]);

                if (vertContent.Length > 2 && vertContent[2].Length > 0)
                    vert.normalIndex = int.Parse(vertContent[2]);

                face.vertices.Add(vert);
            }

            return face;
        }

        void ProcessCommand(string command, string content)
        {
            switch (command)
            {
                case "v":
                    v_positions.Add(ToV3(content));
                    break;
                case "vt":
                    v_uvs.Add(ToV2(content));
                    break;
                case "vn":
                    v_normals.Add(ToV3(content));
                    break;


                case "f":
                    if (currentObject == null)
                    {
                        currentObject = new WfObject();
                        currentObject.name = "unnamed";
                        objects.Add(currentObject);
                    }
                    currentObject.faces.Add(MakeFace(content));
                    break;


                case "usemtl":
                    // check if the material exists and if not make a new name for it
                    currentMatIndex = materialNames.IndexOf(content);
                    if (currentMatIndex == -1)
                    {
                        materialNames.Add(content);
                        currentMatIndex = materialNames.Count - 1;
                    }
                    if (!currentObject.materials.Contains(currentMatIndex))
                        currentObject.materials.Add(currentMatIndex);
                    break;

                case "o":
                    currentObject = new WfObject();
                    currentObject.name = content;
                    objects.Add(currentObject);
                    break;
            }
        }
    }
}
