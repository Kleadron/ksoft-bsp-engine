using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using KleadronCommon.Data;
using KSoft.Game.BSP;
using KleadronCommon;
using System.IO;

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

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            Console.WriteLine("Engine Created");
            IsMouseVisible = true;

            float axisSize = 256;

            axisverts = new VertexPositionColor[]
            {
                new VertexPositionColor(new Vector3(0, 0, 0), Color.Red),
                new VertexPositionColor(new Vector3(axisSize, 0, 0), Color.Red),

                new VertexPositionColor(new Vector3(0, 0, 0), Color.Lime),
                new VertexPositionColor(new Vector3(0, axisSize, 0), Color.Lime),

                new VertexPositionColor(new Vector3(0, 0, 0), Color.Blue),
                new VertexPositionColor(new Vector3(0, 0, axisSize), Color.Blue),

                new VertexPositionColor(new Vector3(0, 0, 0), Color.DarkRed),
                new VertexPositionColor(new Vector3(-axisSize, 0, 0), Color.DarkRed),

                new VertexPositionColor(new Vector3(0, 0, 0), Color.Green),
                new VertexPositionColor(new Vector3(0, -axisSize, 0), Color.Green),

                new VertexPositionColor(new Vector3(0, 0, 0), Color.DarkBlue),
                new VertexPositionColor(new Vector3(0, 0, -axisSize), Color.DarkBlue),
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

            string mapname = "industrial.map";

            if (!File.Exists(mapname))
            {
                throw new Exception("Map file does not exist. Make sure you are using the correct map name and have the file placed next to the EXE.");
            }

            List<DiskEntity> mapEntities = MapLoader.LoadEntities(mapname);

            Console.WriteLine("Loaded Entities");

            foreach(DiskEntity ent in mapEntities)
            {
                Console.WriteLine("\t" + ent.Classname);
            }

            List<Solid> solids = mapEntities[0].solids;

            //int numrenderverts = blocksolid.polygons.Count * 3;
            List<VertexPositionColor> vlist = new List<VertexPositionColor>();
            //renderverts = new VertexPositionColor[numrenderverts];

            foreach (Solid solid in solids)
            {
                foreach (Polygon poly in solid.polygons)
                {
                    Color c = RandomColor();
                    for (int j = 2; j < poly.vertices.Count; j++)
                    {
                        vlist.Add(new VertexPositionColor(poly.vertices[0], c));
                        vlist.Add(new VertexPositionColor(poly.vertices[j - 1], c));
                        vlist.Add(new VertexPositionColor(poly.vertices[j], c));

                        // fade color to reveal triangles and winding order
                        c *= 0.9f;
                        c.A = 255;
                    }
                }
            }

            renderverts = vlist.ToArray();

            //bool convex = Polygon.IsConvexSet(blocksolid);

            //Vector3 a = new Vector3(0, 0, 10);

            //BSPTreePolygon poly = new BSPTreePolygon(Vector3.Zero+a, Vector3.UnitY+a, Vector3.UnitX+a);

            //BSPPolygonSide side = poly.ClassifyPoint(new Vector3(0, 0, -1));
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
            wireframeState.CullMode = CullMode.CullCounterClockwiseFace;
            wireframeState.FillMode = FillMode.WireFrame;

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


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(64, 64, 64));

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Vector3 cameraForward = Vector3.UnitX;
            Vector3 cameraUp = Vector3.UnitZ;

            Matrix camTrans = Matrix.CreateRotationY(MathHelper.ToRadians(cameraPitch)) * Matrix.CreateRotationZ(MathHelper.ToRadians(cameraYaw));

            cameraForward = Vector3.TransformNormal(cameraForward, camTrans);
            cameraUp = Vector3.TransformNormal(cameraUp, camTrans);

            world = Matrix.Identity;
            view = Matrix.CreateLookAt(cameraOrigin, cameraOrigin + cameraForward, cameraUp);
            proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height, 1f, 3000f);
            //world = Matrix.CreateRotationZ((float)gameTime.TotalGameTime.TotalSeconds * 0.25f);


            effect.View = view;
            effect.Projection = proj;
            effect.World = world;

            effect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, axisverts, 0, axisverts.Length / 2);
            effect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, renderverts, 0, renderverts.Length / 3);

            base.Draw(gameTime);
        }
    }
}
