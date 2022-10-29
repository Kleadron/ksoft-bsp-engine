using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSoft.Game.BSP;
using KSoft.Game.Primitives;
using Microsoft.Xna.Framework;

namespace KSoft.Game.Map
{
    // Holds graphics geometry of an entity brush model but lets you render it using your own effect pass.
    // This will be useful for the material system.

    // Idea: Allow map model to use BSP data or create new PartitionedMapModel object
    // for now, I will only use the map model for the resulting split map
    public class MapModel
    {
        public readonly BoundingBox bb;
        public readonly Vector3 size;

        VertexPositionColor[] renderverts;
        int[] renderindices;

        //BasicEffect effect;
        GraphicsDevice device;

        // Takes a list of polygons and automatically creates renderable geometry
        public MapModel(GraphicsDevice device, List<Polygon> polygons)
        {
            this.device = device;
            //effect = new BasicEffect(device);
            //effect.VertexColorEnabled = true;

            List<VertexPositionColor> vlist = new List<VertexPositionColor>();
            List<int> ilist = new List<int>();

            CreateRenderData(vlist, ilist, polygons);

            renderverts = vlist.ToArray();
            renderindices = ilist.ToArray();

            Vector3 min = Vector3.One * 4096;
            Vector3 max = Vector3.One * -4096;

            for (int i = 0; i < renderverts.Length; i++)
            {
                Vector3 check = renderverts[i].Position;

                if (min.X > check.X)
                    min.X = check.X;
                if (min.Y > check.Y)
                    min.Y = check.Y;
                if (min.Z > check.Z)
                    min.Z = check.Z;

                if (max.X < check.X)
                    max.X = check.X;
                if (max.Y < check.Y)
                    max.Y = check.Y;
                if (max.Z < check.Z)
                    max.Z = check.Z;
            }

            bb = new BoundingBox(min, max);

            size = max - min;
        }

        void CreateRenderData(List<VertexPositionColor> verts, List<int> indices, List<Polygon> polygons)
        {
            if (polygons.Count == 0)
                return;

            bool randomColorPerSurface = false;

            // dedupe
            void AddVert(Vector3 pos, Color c)
            {
                const bool dedupe = false;
                //const bool round = true;

                // rounds vertex positions to nearest 8th
                //if (round)
                //    pos = pos.RoundToStep(Extensions.MapVertexRound);

                // uses existing vertex instead of creating a new one, slow
                if (dedupe)
                {
                    for (int i = 0; i < verts.Count; i++)
                    {
                        if (verts[i].Position.EquivalentTo(pos, Extensions.MapVertexRound))
                        {
                            indices.Add(i);
                            return;
                        }
                    }
                }

                verts.Add(new VertexPositionColor(pos, c));
                indices.Add(verts.Count - 1);
            }

            Color c1 = Color.White;

            if (!randomColorPerSurface)
                c1 = Game1.RandomColor();

            foreach (Polygon poly in polygons)
            {
                if (randomColorPerSurface)
                    c1 = Game1.RandomColor();

                Color c = c1;
                for (int j = 2; j < poly.vertices.Count; j++)
                {
                    AddVert(poly.vertices[0], c);
                    AddVert(poly.vertices[j - 1], c);
                    AddVert(poly.vertices[j], c);

                    // fade color to reveal triangles and winding order
                    c *= 0.9f;
                    c.A = 255;
                }
            }
        }

        public void Draw(EffectPass pass)
        {
            if (renderverts.Length > 0 && renderindices.Length > 0)
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, renderverts, 0, renderverts.Length, renderindices, 0, renderindices.Length / 3);
            }
        }
    }
}
