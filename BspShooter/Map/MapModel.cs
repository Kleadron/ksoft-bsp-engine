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

        VertexPositionColor[] polyVerts;
        int[] polyIndices;

        VertexPositionColor[] lineVerts;
        int[] lineIndices;

        Color color;

        //BasicEffect effect;
        GraphicsDevice device;

        // Takes a list of polygons and automatically creates renderable geometry
        public MapModel(GraphicsDevice device, List<Polygon> polygons, Color? color = null)
        {
            this.device = device;
            //effect = new BasicEffect(device);
            //effect.VertexColorEnabled = true;

            if (color == null)
                this.color = Game1.RandomColor();
            else
                this.color = color.GetValueOrDefault();

            List<VertexPositionColor> vlist = new List<VertexPositionColor>();
            List<int> ilist = new List<int>();

            CreateRenderData(vlist, ilist, polygons, false);

            polyVerts = vlist.ToArray();
            polyIndices = ilist.ToArray();

            vlist.Clear();
            ilist.Clear();

            CreateRenderData(vlist, ilist, polygons, true);

            lineVerts = vlist.ToArray();
            lineIndices = ilist.ToArray();


            Vector3 min = Vector3.One * 4096;
            Vector3 max = Vector3.One * -4096;

            for (int i = 0; i < polyVerts.Length; i++)
            {
                Vector3 check = polyVerts[i].Position;

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

        void CreateRenderData(List<VertexPositionColor> verts, List<int> indices, List<Polygon> polygons, bool lines)
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
                c1 = color;

            foreach (Polygon poly in polygons)
            {
                if (poly.surface.nodraw)
                    continue;

                if (randomColorPerSurface)
                    c1 = Game1.RandomColor();

                Color c = c1;

                if (lines)
                {
                    const float fadeStrength = 0.95f;

                    for (int j = 1; j < poly.vertices.Count; j++)
                    {
                        AddVert(poly.vertices[j-1], c);
                        c *= fadeStrength;
                        c.A = 255;
                        AddVert(poly.vertices[j], c);
                        c *= fadeStrength;
                        c.A = 255;
                    }

                    AddVert(poly.vertices[poly.vertices.Count-1], c);
                    c *= fadeStrength;
                    c.A = 255;
                    AddVert(poly.vertices[0], c);
                }
                else
                {
                    for (int j = 2; j < poly.vertices.Count; j++)
                    {
                        const float fadeStrength = 0.95f;

                        AddVert(poly.vertices[0], c);
                        c *= fadeStrength;
                        c.A = 255;
                        AddVert(poly.vertices[j - 1], c);
                        c *= fadeStrength;
                        c.A = 255;
                        AddVert(poly.vertices[j], c);
                        c *= fadeStrength;
                        c.A = 255;

                        // fade color to reveal triangles and winding order
                        //c *= 0.9f;
                        //c.A = 255;
                    }
                }
                
            }
        }

        public void DrawPolygons(EffectPass pass)
        {
            if (polyVerts.Length > 0 && polyIndices.Length > 0)
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, polyVerts, 0, polyVerts.Length, polyIndices, 0, polyIndices.Length / 3);
            }
        }

        public void DrawLines(EffectPass pass)
        {
            if (lineVerts.Length > 0 && lineIndices.Length > 0)
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives(PrimitiveType.LineList, lineVerts, 0, lineVerts.Length, lineIndices, 0, lineIndices.Length / 2);
            }
        }
    }
}
