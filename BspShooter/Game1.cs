using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using KleadronCommon.Data;
using KSoft.Game.Map;
using KSoft.Game.Objects;
using KSoft.Game.Primitives;
using KleadronCommon;
using System.IO;
using QoiSharp;
using System.Diagnostics;

namespace KSoft.Game
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        VertexPositionColor[] renderverts;
        int[] renderindices;

        VertexPositionColor[] axisverts;
        BasicEffect effect;

        Matrix world;
        Matrix view;
        Matrix proj;

        RasterizerState wireframeState;

        //SpriteFont font;

        Random r = new Random();

        Vector3 cameraOrigin = new Vector3(128, 0, 32);
        float cameraPitch = 20;
        float cameraYaw = 180;

        Color RandomColor()
        {
            return new Color(r.Next(255), r.Next(255), r.Next(255));
        }

        InputSystem input;

        int axisSize = 64;
        bool screenshotQueued = false;

        enum WireframeMode
        {
            Off,
            Surfaces,
            XRaySurfaces,
            XRay,
            NUM_MODES
        }
        WireframeMode wireframeMode = WireframeMode.Surfaces;

        protected override void OnExiting(object sender, EventArgs args)
        {
            screenshotQueued = true;
            try
            {
                if (BeginDraw())
                {
                    Draw(new GameTime());
                    EndDraw();
                }
            }
            catch
            {
                Console.WriteLine("Exit screenshot failed but don't care");
            }
            
            base.OnExiting(sender, args);
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

            Console.WriteLine("Engine Created");
            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            float lineSize = axisSize / 2;

            axisverts = new VertexPositionColor[]
            {
                new VertexPositionColor(new Vector3(0, 0, 0), Color.Red),
                new VertexPositionColor(new Vector3(lineSize, 0, 0), Color.Red),

                new VertexPositionColor(new Vector3(0, 0, 0), Color.Lime),
                new VertexPositionColor(new Vector3(0, lineSize, 0), Color.Lime),

                new VertexPositionColor(new Vector3(0, 0, 0), Color.Blue),
                new VertexPositionColor(new Vector3(0, 0, lineSize), Color.Blue),

                //new VertexPositionColor(new Vector3(0, 0, 0), Color.DarkRed),
                //new VertexPositionColor(new Vector3(-lineSize, 0, 0), Color.DarkRed),

                //new VertexPositionColor(new Vector3(0, 0, 0), Color.Green),
                //new VertexPositionColor(new Vector3(0, -lineSize, 0), Color.Green),

                //new VertexPositionColor(new Vector3(0, 0, 0), Color.DarkBlue),
                //new VertexPositionColor(new Vector3(0, 0, -lineSize), Color.DarkBlue),
            };

            /*
               ( -16 -16 -64 ) ( -16 -15 -64 ) ( -16 -16 -63 ) __TB_empty 0 0 0 1 1
               ( -16 -16 -64 ) ( -16 -16 -63 ) ( -15 -16 -64 ) __TB_empty 0 0 0 1 1
               ( -16 -16 -16 ) ( -15 -16 -16 ) ( -16 -15 -16 ) __TB_empty 0 0 0 1 1

               ( 48 64 16 ) ( 48 65 16 ) ( 49 64 16 ) __TB_empty 0 0 0 1 1
               ( 48 16 -48 ) ( 49 16 -48 ) ( 48 16 -47 ) __TB_empty 0 0 0 1 1
               ( 16 64 -48 ) ( 16 64 -47 ) ( 16 65 -48 ) __TB_empty 0 0 0 1 1
             */

            //Polygon[] blocksolid = new Polygon[]
            //{
            //    new Polygon(-16, -16, -64,       -16, -15, -64,      -16, -16, -63),
            //    new Polygon(-16, -16, -64,       -16, -16, -63,      -15, -16, -64),
            //    new Polygon(-16, -16, -16,       -15, -16, -16,      -16, -15, -16),

            //    new Polygon(48, 64, 16,          48, 65, 16,         49, 64, 16),
            //    new Polygon(48, 16, -48,         49, 16, -48,        48, 16, -47),
            //    new Polygon(16, 64, -48,         16, 64, -47,        16, 65, -48),
            //};

            //Solid blocksolid = new Solid(
            //    new Surface(-16, -16, -64, -16, -15, -64, -16, -16, -63),
            //    new Surface(-16, -16, -64, -16, -16, -63, -15, -16, -64),
            //    new Surface(-16, -16, -16, -15, -16, -16, -16, -15, -16),

            //    new Surface(48, 64, 16, 48, 65, 16, 49, 64, 16),
            //    new Surface(48, 16, -48, 49, 16, -48, 48, 16, -47),

            //    new Surface(16, 0, 0, 0, 0, 16, 0, 16, 0), //(16 0 0)(0 0 16)(0 16 0) __TB_empty 0 0 0 1 1
            //    new Surface(0, 16, 16, 16, 144, 0, 16, 16, 0),

            //    new Surface(16, 64, -48, 16, 64, -47, 16, 65, -48));

            //List<Solid> solids = MapLoader.GetSolids("Content/industrial.map");

            // favorite maps: dm3 and e3m5

            string mapname = "qmaps/e4m1.map";

            if (!File.Exists(mapname))
            {
                throw new Exception("Map file does not exist. Make sure you are using the correct map name and have the file placed next to the EXE.");
            }

            List<DiskEntity> mapEntities = MapLoader.LoadEntities(mapname);

            Console.WriteLine("Loaded Entities");

            foreach(DiskEntity ent in mapEntities)
            {
                Console.WriteLine("\t" + ent.ClassName);
            }

            //List<Solid> solids = mapEntities[0].solids;

            //int numrenderverts = blocksolid.polygons.Count * 3;
            List<VertexPositionColor> vlist = new List<VertexPositionColor>();
            List<int> ilist = new List<int>();
            //renderverts = new VertexPositionColor[numrenderverts];

            bool worldspawnOnly = true;

            if (worldspawnOnly)
            {
                //BuildEntitySolids(vlist, ilist, mapEntities[0]);
                //BuildStaticGeo(vlist, ilist, mapEntities[0]);
                BuildSplitGeometry(vlist, ilist, mapEntities[0]);
            }
            else
            {
                foreach(DiskEntity entity in mapEntities)
                    BuildEntitySolids(vlist, ilist, entity);
            }

            renderverts = vlist.ToArray();
            renderindices = ilist.ToArray();

            Vector3 min = Vector3.One * 4096;
            Vector3 max = Vector3.One * -4096;

            for(int i = 0; i < renderverts.Length; i++)
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

            Vector3 size = max - min;

            Console.WriteLine("Map Dimensions: " + min + " " + max);
            Console.WriteLine("Size: " + size);

            //bool convex = Polygon.IsConvexSet(blocksolid);

            //Vector3 a = new Vector3(0, 0, 10);

            //BSPTreePolygon poly = new BSPTreePolygon(Vector3.Zero+a, Vector3.UnitY+a, Vector3.UnitX+a);

            //BSPPolygonSide side = poly.ClassifyPoint(new Vector3(0, 0, -1));
        }

        //void BuildStaticGeo(List<VertexPositionColor> verts, List<int> indices, DiskEntity entity)
        //{
        //    const float MinVertexSplitDistance = 3;
        //    const float MapRoundEpsilon = 2;

        //    void AddVert(Vector3 pos, Color c)
        //    {
        //        verts.Add(new VertexPositionColor(pos, c));
        //        indices.Add(verts.Count - 1);
        //    }

        //    List<Polygon> polygons = new List<Polygon>();
        //    foreach (Solid solid in entity.solids)
        //        polygons.AddRange(solid.polygons);

        //    Console.WriteLine("starting polys: " + polygons.Count);

        //    List<BoundingBox> bboxes = new List<BoundingBox>();
        //    foreach (Polygon poly in polygons)
        //        bboxes.Add(BoundingBox.CreateFromPoints(poly.vertices));

        //    //List<Polygon> newPolygons = new List<Polygon>();

        //    int polyPrintInterval = 500;
        //    int polysProcessed = 0;

        //    for (int i = 0; i+1 < polygons.Count; i++)
        //    {
        //        Polygon poly1 = polygons[i];

        //        // j should have been i + 1 the whole time, whoops
        //        for(int j = i+1; j < polygons.Count; j++)
        //        {
        //            if (i == j)
        //                continue;

        //            BoundingBox poly1BB = bboxes[i];
                    
        //            poly1BB.Min -= Vector3.One * MapRoundEpsilon;
        //            poly1BB.Max += Vector3.One * MapRoundEpsilon;

        //            if (poly1BB.Min.X > poly1BB.Max.X)
        //                poly1BB.Min.X = poly1BB.Max.X = (poly1BB.Min.X + poly1BB.Max.X) * 0.5f;
        //            if (poly1BB.Min.Y > poly1BB.Max.Y)
        //                poly1BB.Min.Y = poly1BB.Max.Y = (poly1BB.Min.Y + poly1BB.Max.Y) * 0.5f;
        //            if (poly1BB.Min.Z > poly1BB.Max.Z)
        //                poly1BB.Min.Z = poly1BB.Max.Z = (poly1BB.Min.Z + poly1BB.Max.Z) * 0.5f;

        //            if (!poly1BB.Intersects(bboxes[j]))
        //                continue;

        //            Polygon poly2 = polygons[j];

        //            if (poly1.ClassifyAgainstPlane(poly2.surface.plane, MapRoundEpsilon) == PlaneClassification.Spanning)
        //            {
        //                Polygon back, front;
        //                bool didSplit = poly1.Split(poly2.surface.plane, out back, out front, MapRoundEpsilon); 

        //                if (didSplit && back != null && front != null &&
        //                    back.IsValid(MapRoundEpsilon, MinVertexSplitDistance) &&
        //                    front.IsValid(MapRoundEpsilon, MinVertexSplitDistance))
        //                {
        //                    polygons[i] = front;
        //                    polygons.Add(back);

        //                    bboxes[i] = BoundingBox.CreateFromPoints(front.vertices);
        //                    bboxes.Add(BoundingBox.CreateFromPoints(back.vertices));
        //                }
        //            }

        //            if (poly2.ClassifyAgainstPlane(poly1.surface.plane, MapRoundEpsilon) == PlaneClassification.Spanning)
        //            {
        //                Polygon back, front;
        //                bool didSplit = poly2.Split(poly1.surface.plane, out back, out front, MapRoundEpsilon);

        //                if (didSplit && back != null && front != null &&
        //                    back.IsValid(MapRoundEpsilon, MinVertexSplitDistance) &&
        //                    front.IsValid(MapRoundEpsilon, MinVertexSplitDistance))
        //                {
        //                    polygons[j] = front;
        //                    polygons.Add(back);

        //                    bboxes[j] = BoundingBox.CreateFromPoints(front.vertices);
        //                    bboxes.Add(BoundingBox.CreateFromPoints(back.vertices));
        //                }
        //            }
        //        }

        //        polysProcessed++;

        //        if (polysProcessed % polyPrintInterval == 0)
        //        {
        //            Console.WriteLine("Processed " + polysProcessed + "/" + polygons.Count);
        //        }
        //    }



        //    //int numPolys = 0;
        //    //for(int firstPoly = 0; firstPoly < polygons.Count; firstPoly++)
        //    //{

        //    //}


        //    Console.WriteLine("ending polys: " + polygons.Count);


        //    foreach (Polygon poly in polygons)
        //    {
        //        Color c = RandomColor();
        //        for (int j = 2; j < poly.vertices.Count; j++)
        //        {
        //            //verts.Add(new VertexPositionColor(poly.vertices[0], c));
        //            //verts.Add(new VertexPositionColor(poly.vertices[j - 1], c));
        //            //verts.Add(new VertexPositionColor(poly.vertices[j], c));

        //            AddVert(poly.vertices[0], c);
        //            AddVert(poly.vertices[j - 1], c);
        //            AddVert(poly.vertices[j], c);

        //            // fade color to reveal triangles and winding order
        //            c *= 0.9f;
        //            c.A = 255;
        //        }
        //    }
        //}

        void BuildSplitGeometry(List<VertexPositionColor> verts, List<int> indices, DiskEntity entity)
        {
            const float MinVertexSplitDistance = Extensions.MapVertexRound;

            // utility functions
            void AddVert(Vector3 pos, Color c)
            {
                verts.Add(new VertexPositionColor(pos, c));
                indices.Add(verts.Count - 1);
            }

            int polyPrintInterval = 500;
            int polysProcessed = 0;

            // collect polygons from the entity's solids
            List<Polygon> polygons = entity.CollectSolidPolygons();

            Console.WriteLine("starting polys: " + polygons.Count);

            // create bounding boxes
            List<BoundingBox> bboxes = new List<BoundingBox>();
            foreach (Polygon poly in polygons)
                bboxes.Add(BoundingBox.CreateFromPoints(poly.vertices));


            // start splitting intersecting polygons

            // poly1
            for (int i = 0; i + 1 < polygons.Count; i++)
            {
                Polygon poly1 = polygons[i];

                // poly2
                for (int j = i + 1; j < polygons.Count; j++)
                {
                    BoundingBox poly1BB = bboxes[i];

                    // inflate bounding box a tiny bit
                    poly1BB.Max += Vector3.One * Extensions.MapVertexRound;
                    poly1BB.Min -= Vector3.One * Extensions.MapVertexRound;

                    BoundingBox poly2BB = bboxes[j];

                    // only try splitting polygons that are vaguely within the same area
                    if (!poly1BB.Intersects(poly2BB))
                        continue;

                    Polygon poly2 = polygons[j];

                    Polygon back, front;

                    bool split = poly1.Split(poly2.surface.plane, out back, out front);

                    // poly1 was successfully split by poly2
                    if (split &&
                        back.IsValid(MinVertexSplitDistance) && front.IsValid(MinVertexSplitDistance))
                    {
                        // replace existing polygon and add a new one
                        polygons[i] = front;
                        polygons.Add(back);

                        bboxes[i] = BoundingBox.CreateFromPoints(front.vertices);
                        bboxes.Add(BoundingBox.CreateFromPoints(back.vertices));
                    }

                    // update reference (I think I forgot to do this originally)
                    poly1 = polygons[i];

                    // check the other one
                    split = poly2.Split(poly1.surface.plane, out back, out front);

                    // poly2 was successfully split by poly1
                    if (split &&
                        back.IsValid(MinVertexSplitDistance) && front.IsValid(MinVertexSplitDistance))
                    {
                        // replace existing polygon and add a new one
                        polygons[j] = front;
                        polygons.Add(back);

                        bboxes[j] = BoundingBox.CreateFromPoints(front.vertices);
                        bboxes.Add(BoundingBox.CreateFromPoints(back.vertices));
                    }

                    // update status
                    if (j % polyPrintInterval == 0)
                    {
                        Console.WriteLine(i + "/" + polygons.Count);
                    }
                }
            }


            // try and merge polygons 
#if false
            int numPolys = 0;

            for(int firstPoly = 0; firstPoly < polygons.Count; firstPoly += numPolys)
            {
                int poly1Index = 0;
                numPolys = 1;

                for(int i = firstPoly + numPolys; i < polygons.Count; i++)
                {
                    PlaneClassification planeClass = polygons[i].ClassifyAgainstPlane(polygons[firstPoly].surface.plane);

                    if (planeClass == PlaneClassification.OnPlane)
                    {
                        Extensions.Swap(polygons, firstPoly + numPolys, i);
                        numPolys++;
                    }
                }

                for(int i = firstPoly; i < firstPoly + numPolys; i++)
                {
                    for(int j = firstPoly; j < firstPoly + numPolys; j++)
                    {
                        Polygon p1 = polygons[i];
                        Polygon p2 = polygons[j];

                        //if (p1.surface != p2.surface)
                        //    continue;

                        Polygon newPoly = null;
                    }
                }
            }
#endif


            // DONE SPLITTING
            Console.WriteLine("ending polys: " + polygons.Count);

            // create graphics
            foreach (Polygon poly in polygons)
            {
                Color c = RandomColor();
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

        void BuildEntitySolids(List<VertexPositionColor> verts, List<int> indices, DiskEntity entity)
        {
            if (entity.solids.Count == 0)
                return;

            int numSolids = entity.solids.Count;
            int solidPrintInterval = 100;
            int solidsProcessed = 0;

            bool randomColorPerSurface = false;

            Console.WriteLine("Building " + numSolids + " solids...");

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
                indices.Add(verts.Count-1);
            }

            Color c1 = Color.White;

            foreach (Solid solid in entity.solids)
            {
                if (!randomColorPerSurface)
                    c1 = RandomColor();

                foreach (Polygon poly in solid.polygons)
                {
                    if (randomColorPerSurface)
                        c1 = RandomColor();

                    Color c = c1;
                    for (int j = 2; j < poly.vertices.Count; j++)
                    {
                        //verts.Add(new VertexPositionColor(poly.vertices[0], c));
                        //verts.Add(new VertexPositionColor(poly.vertices[j - 1], c));
                        //verts.Add(new VertexPositionColor(poly.vertices[j], c));

                        AddVert(poly.vertices[0], c);
                        AddVert(poly.vertices[j - 1], c);
                        AddVert(poly.vertices[j], c);

                        // fade color to reveal triangles and winding order
                        c *= 0.9f;
                        c.A = 255;
                    }
                }

                solidsProcessed++;

                if (solidsProcessed % solidPrintInterval == 0)
                {
                    Console.WriteLine("Processed " + solidsProcessed + "/" + numSolids);
                }
            }

            Console.WriteLine("Built entity solids " + numSolids);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            input = new InputSystem(this);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //WavefrontFile model = new WavefrontFile("Content/bsptest.obj");
            effect = new BasicEffect(GraphicsDevice);
            //effect.EnableDefaultLighting();
            effect.VertexColorEnabled = true;

            world = Matrix.Identity;
            view = Matrix.CreateLookAt(new Vector3(32, 32, 32)*3, Vector3.Zero, Vector3.UnitZ);

            wireframeState = new RasterizerState();
            wireframeState.CullMode = CullMode.None;
            wireframeState.FillMode = FillMode.WireFrame;
            //wireframeState.DepthBias = -0.0001f;

            //font = Content.Load<SpriteFont>("font");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            Vector3 forward = Vector3.UnitX;
            Vector3 right = -Vector3.UnitY;
            Vector3 up = Vector3.UnitZ;

            float total = gameTime.TotalTime();
            float delta = gameTime.DeltaTime();

            input.Update(delta, total);

            if (input.KeyHeld(Keys.LeftShift))
                delta *= 4;

            if (input.KeyHeld(Keys.Left))
                cameraYaw += 90f * delta;
            if (input.KeyHeld(Keys.Right))
                cameraYaw -= 90f * delta;

            if (input.KeyHeld(Keys.Up))
                cameraPitch -= 90f * delta;
            if (input.KeyHeld(Keys.Down))
                cameraPitch += 90f * delta;

            if (input.KeyPressed(Keys.Escape))
                IsMouseVisible = !IsMouseVisible;

            if (!IsActive)
                IsMouseVisible = true;

            if (!IsMouseVisible)
            {
                Point center = new Point(GraphicsDevice.Viewport.Width/2, GraphicsDevice.Viewport.Height/2);

                cameraYaw += input.mouseDelta.X * 0.2f;
                cameraPitch -= input.mouseDelta.Y * 0.2f;

                input.SetMousePosition(center.X, center.Y);
            }

            cameraPitch = MathHelper.Clamp(cameraPitch, -90f, 90f);

            Matrix moveRot = Matrix.CreateRotationZ(MathHelper.ToRadians(cameraYaw));
            forward = Vector3.TransformNormal(forward, moveRot);
            right = Vector3.TransformNormal(right, moveRot);

            Vector3 move = Vector3.Zero;

            if (input.KeyHeld(Keys.W))
                move += forward;
            if (input.KeyHeld(Keys.S))
                move -= forward;
            if (input.KeyHeld(Keys.A))
                move -= right;
            if (input.KeyHeld(Keys.D))
                move += right;
            if (input.KeyHeld(Keys.Q))
                move -= up;
            if (input.KeyHeld(Keys.E))
                move += up;

            move = move.SafeNormalise();
            cameraOrigin += move * 256f * delta;

            if (input.KeyPressed(Keys.F12))
                screenshotQueued = true;

            if (input.KeyPressed(Keys.F1))
            {
                Process.Start("explorer.exe", Directory.GetCurrentDirectory());
            }

            if (input.KeyPressed(Keys.F2))
            {
                wireframeMode++;
                wireframeMode = (WireframeMode)((int)wireframeMode % (int)WireframeMode.NUM_MODES);
            }

            base.Update(gameTime);
        }

        void DrawMap(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Vector3 cameraForward = Vector3.UnitX;
            Vector3 cameraUp = Vector3.UnitZ;

            Matrix camTrans = Matrix.CreateRotationY(MathHelper.ToRadians(cameraPitch)) * Matrix.CreateRotationZ(MathHelper.ToRadians(cameraYaw));

            cameraForward = Vector3.TransformNormal(cameraForward, camTrans);
            cameraUp = Vector3.TransformNormal(cameraUp, camTrans);

            world = Matrix.Identity;
            view = Matrix.CreateLookAt(cameraOrigin, cameraOrigin + cameraForward, cameraUp);
            proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height, 1f, 8192);
            //world = Matrix.CreateRotationZ((float)gameTime.TotalGameTime.TotalSeconds * 0.25f);


            effect.View = view;
            effect.Projection = proj;
            effect.World = world;

            if (wireframeMode != WireframeMode.XRay)
            {
                effect.VertexColorEnabled = true;
                effect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, renderverts, 0, renderverts.Length, renderindices, 0, renderindices.Length / 3);
            }

            if (wireframeMode != WireframeMode.Off)
            {
                effect.VertexColorEnabled = wireframeMode == WireframeMode.XRay;
                GraphicsDevice.RasterizerState = wireframeState;

                if (wireframeMode == WireframeMode.XRaySurfaces || wireframeMode == WireframeMode.XRay)
                    GraphicsDevice.DepthStencilState = DepthStencilState.None;

                effect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, renderverts, 0, renderverts.Length, renderindices, 0, renderindices.Length / 3);
            }

            effect.VertexColorEnabled = true;
            effect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, axisverts, 0, axisverts.Length / 2);
        }

        void DrawAxisGizmo(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            Viewport oldViewport = GraphicsDevice.Viewport;
            Viewport newViewport = oldViewport;

            newViewport.X = oldViewport.Width - axisSize - 10;
            newViewport.Y = 10;
            newViewport.Width = axisSize;
            newViewport.Height = axisSize;

            GraphicsDevice.Viewport = newViewport;

            Vector3 cameraForward = Vector3.UnitX;
            Vector3 cameraUp = Vector3.UnitZ;

            Matrix camTrans = Matrix.CreateRotationY(MathHelper.ToRadians(cameraPitch)) * Matrix.CreateRotationZ(MathHelper.ToRadians(cameraYaw));

            cameraForward = Vector3.TransformNormal(cameraForward, camTrans);
            cameraUp = Vector3.TransformNormal(cameraUp, camTrans);

            int viewSize = axisSize / 2;

            world = Matrix.Identity;
            view = Matrix.CreateLookAt(cameraForward * viewSize, Vector3.Zero, cameraUp);
            //proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height, 1f, 5000f);
            
            proj = Matrix.CreateOrthographicOffCenter(viewSize, -viewSize, -viewSize, viewSize, -axisSize, axisSize);
            //world = Matrix.CreateRotationZ((float)gameTime.TotalGameTime.TotalSeconds * 0.25f);

            effect.View = view;
            effect.Projection = proj;
            effect.World = world;

            effect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, axisverts, 0, axisverts.Length / 2);

            GraphicsDevice.Viewport = oldViewport;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(64, 64, 64));

            DrawMap(gameTime);
            DrawAxisGizmo(gameTime);

            // screenshot code
            if (screenshotQueued)
            {
                int width = GraphicsDevice.Viewport.Width;
                int height = GraphicsDevice.Viewport.Height;

                // get RGBA backbuffer
                byte[] bbdata = new byte[width * height * 4];
                GraphicsDevice.GetBackBufferData(bbdata);

                // convert to RGB
                byte[] noalpha = new byte[width * height * 3];
                for(int i = 0; i < width * height; i++)
                {
                    int bbdataI = i * 4;
                    int noalphaI = i * 3;

                    noalpha[noalphaI] = bbdata[bbdataI];        // RED
                    noalpha[noalphaI+1] = bbdata[bbdataI+1];    // GREEN
                    noalpha[noalphaI+2] = bbdata[bbdataI+2];    // BLUE
                }

                QoiImage img = new QoiImage(noalpha, width, height, QoiSharp.Codec.Channels.Rgb);
                byte[] encoded = QoiEncoder.Encode(img);

                string dir = "screenshots";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                int id = 0;
                while(true)
                {
                    string filename = dir + "/" + id + ".qoi";
                    if (!File.Exists(filename))
                    {
                        File.WriteAllBytes(filename, encoded);
                        Console.WriteLine("Created " + filename);
                        break;
                    }
                    else
                    {
                        id++;
                    }
                }

                screenshotQueued = false;
            }

            base.Draw(gameTime);
        }
    }
}
