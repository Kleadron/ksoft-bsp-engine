using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KSoft.Game.BSP
{
    public static class MapLoader
    {
        //// Game: Generic
        //// Format: Standard
        //// entity 0
        //{
        //"classname" "worldspawn"
        //// brush 0
        //{
        //( -16 -16 -64 ) ( -16 -15 -64 ) ( -16 -16 -63 ) __TB_empty 0 0 0 1 1
        //( -16 -16 -64 ) ( -16 -16 -63 ) ( -15 -16 -64 ) __TB_empty 0 0 0 1 1
        //( -16 -16 -16 ) ( -15 -16 -16 ) ( -16 -15 -16 ) __TB_empty 0 0 0 1 1
        //( 48 64 16 ) ( 48 65 16 ) ( 49 64 16 ) __TB_empty 0 0 0 1 1
        //( 48 16 -48 ) ( 49 16 -48 ) ( 48 16 -47 ) __TB_empty 0 0 0 1 1
        //( 16 0 0 ) ( 0 0 16 ) ( 0 16 0 ) __TB_empty 0 0 0 1 1
        //( 0 16 16 ) ( 16 144 0 ) ( 16 16 0 ) __TB_empty 0 0 0 1 1
        //( 16 64 -48 ) ( 16 64 -47 ) ( 16 65 -48 ) __TB_empty 0 0 0 1 1
        //}
        //}

        enum ReadState
        {
            Nothing,
            Entity,
            Solid
        }

        public static List<DiskEntity> LoadEntities(string mapfile)
        {
            List<DiskEntity> entities = new List<DiskEntity>();

            DiskEntity curEntity = null;

            Stream s = File.OpenRead(mapfile);
            StreamReader sr = new StreamReader(s);

            ReadState state = ReadState.Nothing;

            List<Surface> surfaces = new List<Surface>();

            int linenum = 0;
            int lineIndex = -1;
            while (true)
            {
                ReadState newstate = state;

                string line = sr.ReadLine();

                linenum++;
                lineIndex++;

                if (line == null)
                    break;

                int commentIndex = line.IndexOf("//");
                if (commentIndex != -1)
                    line = line.Remove(commentIndex);

                line = line.Trim();

                if (line.Length == 0)
                    continue;

                switch (state)
                {
                    case ReadState.Nothing:
                        {
                            if (line == "{")
                            {
                                // start reading the entity
                                newstate = ReadState.Entity;
                                curEntity = new DiskEntity();
                            }
                            else
                            {
                                // fuck, idk lol do nothing
                            }
                        }
                        break;
                    case ReadState.Entity:
                        {
                            if (line == "{")
                            {
                                // start reading the solid
                                newstate = ReadState.Solid;
                            }
                            else if (line == "}")
                            {
                                // stop reading the entity
                                newstate = ReadState.Nothing;
                                entities.Add(curEntity);
                                curEntity = null;
                            }
                            else
                            {
                                // do entity stuff
                                string key = null;
                                string value = null;

                                int stringStart = line.IndexOf('"') + 1;
                                int stringEnd = line.IndexOf('"', stringStart);

                                key = line.Substring(stringStart, stringEnd - stringStart);

                                stringStart = line.IndexOf('"', stringEnd + 2) + 1;
                                stringEnd = line.IndexOf('"', stringStart);

                                value = line.Substring(stringStart, stringEnd - stringStart);

                                curEntity.keyvalues[key] = value;
                            }
                        }
                        break;
                    case ReadState.Solid:
                        {
                            if (line == "}")
                            {
                                // stop reading the solid
                                newstate = ReadState.Entity;
                                Solid solid = new Solid(surfaces.ToArray());
                                curEntity.solids.Add(solid);
                                surfaces.Clear();
                            }
                            else
                            {
                                // read surface
                                Surface surf = new Surface(line);
                                surfaces.Add(surf);
                            }
                        }
                        break;
                }

                state = newstate;
            }

            sr.Close();
            s.Close();

            return entities;
        }
    }
}
