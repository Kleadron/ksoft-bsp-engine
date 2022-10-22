using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KleadronCommon.Data;

namespace KleadronCommon.Graphics
{
    // KleadronCommon Model
    // This should be fine for most if not all use cases. If you need more advanced model configuration or generation, use the Supercombiner.
    public class KcModel
    {
        public BasicEffect[] effects;

        // basically a draw call
        class RenderGroup
        {
            public int effectIndex;

            public int startVertex;  // base vertex index
            //public int minVertIndex; // 0
            public int numVerts;    // number of vertices used
            public int startIndex;
            public int primitives;
        }

        GraphicsDevice device;

        //BasicEffect effect;

        VertexBuffer vb;
        IndexBuffer ib;

        //public KcMesh[] meshes;
        RenderGroup[] groups;

        static List<VertexPositionNormalTexture> cVerts = new List<VertexPositionNormalTexture>();
        static List<int> cIndexes = new List<int>();

        public KcModel(GraphicsDevice device, string path)
        {
            this.device = device;

            WavefrontFile mdata = new WavefrontFile(path);

            Build(mdata, null);
        }

        public KcModel(GraphicsDevice device, string path, string objName)
        {
            this.device = device;

            WavefrontFile mdata = new WavefrontFile(path);

            WfObject buildObject = mdata.GetObjectByName(objName);

            if (buildObject == null)
                throw new Exception("object \"" + objName + "\"" + "could not be found in \"" + mdata.filename + "\"");

            Build(mdata, buildObject);
        }

        public KcModel(GraphicsDevice device, WavefrontFile mdata, string objName)
        {
            this.device = device;

            WfObject buildObject = mdata.GetObjectByName(objName);

            if (buildObject == null)
                throw new Exception("object \"" + objName + "\"" + "could not be found in \"" + mdata.filename + "\"");

            Build(mdata, buildObject);
        }

        void BuildFace(WavefrontFile mdata, WfFace face)
        {
            int curCvert = cVerts.Count;
            int vertCount = face.vertices.Count;

            // make vertices
            for (int i = 0; i < vertCount; i++)
            {
                WfVertex v = face.vertices[i];
                VertexPositionNormalTexture cv = new VertexPositionNormalTexture(
                    mdata.v_positions[v.positionIndex],
                    mdata.v_normals[v.normalIndex],
                    mdata.v_uvs[v.uvIndex]);

                cVerts.Add(cv);
            }

            // make indices
            for (int i = 2; i < vertCount; i++)
            {
                cIndexes.Add(curCvert);
                cIndexes.Add(curCvert + (i - 1));
                cIndexes.Add(curCvert + i);
            }
        }

        // Sorts and builds object faces and materials into renderable groups
        void Build(WavefrontFile mdata, WfObject objToBuild)
        {
            cVerts.Clear();
            cIndexes.Clear();


            // Sort faces into their respective materials
            groups = new RenderGroup[mdata.materialNames.Count];
            List<WfFace>[] materialFaces = new List<WfFace>[groups.Length];
            for(int i = 0; i < materialFaces.Length; i++)
            {
                materialFaces[i] = new List<WfFace>();
            }

            if (objToBuild == null)
            {
                foreach (WfObject obj in mdata.objects)
                {
                    foreach (WfFace face in obj.faces)
                    {
                        materialFaces[face.materialIndex].Add(face);
                    }
                }
            }
            else
            {
                foreach (WfFace face in objToBuild.faces)
                {
                    materialFaces[face.materialIndex].Add(face);
                }
            }

            // Create effects
            effects = new BasicEffect[groups.Length];
            for(int i = 0; i < effects.Length; i++)
            {
                // skip empty groups
                //if (materialFaces[i].Count == 0)
                //    continue;

                BasicEffect effect = new BasicEffect(device);

                effect.Tag = mdata.materialNames[i];
                effect.EnableDefaultLighting();
                effect.DiffuseColor = GetGroupDebugColor(i).ToVector3();

                effects[i] = effect;
            }

            // Build the faces
            for(int i = 0; i < groups.Length; i++)
            {
                List<WfFace> groupFaces = materialFaces[i];

                RenderGroup group = new RenderGroup();

                group.effectIndex = i;
                group.startVertex = cVerts.Count;
                group.startIndex = cIndexes.Count;

                foreach(WfFace face in groupFaces)
                {
                    BuildFace(mdata, face);
                }

                group.numVerts = cVerts.Count - group.startVertex;
                group.primitives = (cIndexes.Count - group.startIndex) / 3;

                groups[i] = group;
            }


            vb = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, cVerts.Count, BufferUsage.WriteOnly);
            ib = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, cIndexes.Count, BufferUsage.WriteOnly);

            vb.SetData(cVerts.ToArray());
            ib.SetData(cIndexes.ToArray());

        }

        Color GetGroupDebugColor(int i)
        {
            Color meshColor = Color.White;

            if (i == 1)
                meshColor = Color.Red;
            if (i == 2)
                meshColor = Color.Lime;
            if (i == 3)
                meshColor = Color.Blue;
            if (i == 4)
                meshColor = Color.Cyan;
            if (i == 5)
                meshColor = Color.Magenta;
            if (i == 6)
                meshColor = Color.Yellow;

            return meshColor;
        }

        // draws the model vertex data using a single effect
        public void DrawAll(EffectPass pass)
        {
            pass.Apply();
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vb.VertexCount, 0, ib.IndexCount / 3);
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            device.SetVertexBuffer(vb);
            device.Indices = ib;

            foreach(RenderGroup group in groups)
            {
                if (group.primitives == 0)
                    continue;

                BasicEffect effect = effects[group.effectIndex];

                effect.World = world;
                effect.View = view;
                effect.Projection = projection;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, group.startVertex, group.numVerts, group.startIndex, group.primitives);
                }
            }
            
        }
    }
}
