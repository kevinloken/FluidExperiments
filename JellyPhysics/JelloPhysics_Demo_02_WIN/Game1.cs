#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using JelloPhysics;
#endregion

namespace JelloDemo_02
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        ContentManager content;
        Vector3 cameraPos;
        Vector3 cameraLookVector;
        BasicEffect lineEffect;

        float screenWidth;
        float screenHeight;

        // PHTSICS VARIABLES
        JelloPhysics.World mWorld;
        List<DraggableSpringBody> mSpringBodies;
        List<DraggablePressureBody> mPressureBodies;
        List<Body> mStaticBodies;

        Vector2 mCursorPos;

        Body dragBody = null;
        int dragPoint = -1;

        bool showDebug = false;
        bool showVelocities = false;

        GamePadState oldPadState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(Services);
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
#if XBOX360
            graphics.PreferredBackBufferWidth = this.Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = this.Window.ClientBounds.Height;
            graphics.PreferMultiSampling = true;
#else
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
#endif
            graphics.ApplyChanges();


            screenWidth = graphics.GraphicsDevice.Viewport.Width;
            screenHeight = graphics.GraphicsDevice.Viewport.Height;

            // setup camera
            cameraPos = new Vector3(-2f, -15f, 25f);
            cameraLookVector = Vector3.Forward;

            // basic effect used for drawing all objects, based on vertexcolor.
            lineEffect = new BasicEffect(graphics.GraphicsDevice, null);
            lineEffect.LightingEnabled = false;
            lineEffect.VertexColorEnabled = true;
            lineEffect.Alpha = 1.0f;

            // initialize physics world
            mWorld = new World();
 
            // lists to keep track of bodies.
            mSpringBodies = new List<DraggableSpringBody>();
            mPressureBodies = new List<DraggablePressureBody>();
            mStaticBodies = new List<Body>();

            // STATIC COLLISION SHAPE
            // all Closed Shape objects are assumed to be a list of lines going from point to point, with the last point
            // connecting back to the first point to close the shape.  This is a simple rectangle to represent the ground for this demo.
            // ClosedShape objects are automatically "centered" when you call finish(), and that center becomes the center when
            // setting the position of the object.
            ClosedShape stat = new ClosedShape();
            stat.begin();
            stat.addVertex(new Vector2(-20f,-1f));
            stat.addVertex(new Vector2(-20f, 1f));
            stat.addVertex(new Vector2(20f, 1f));
            stat.addVertex(new Vector2(20f, -1f));
            stat.finish();

            // creating the body.  Since this is a static body, we can use the base class Body.
            // setting the mass per point to PositiveInfinity makes it immobile / static.
            Body b = new Body(mWorld, stat, float.PositiveInfinity, new Vector2(0f, -19f), 0, Vector2.One, false);
            mStaticBodies.Add(b);


            // this is a more complex body, in the shape of a capital "I", and connected with many internal springs.
            ClosedShape shape = new ClosedShape();
            shape.begin();
            shape.addVertex(new Vector2(-1.5f, 2.0f));
            shape.addVertex(new Vector2(-0.5f, 2.0f));
            shape.addVertex(new Vector2(0.5f, 2.0f));
            shape.addVertex(new Vector2(1.5f, 2.0f));
            shape.addVertex(new Vector2(1.5f, 1.0f));
            shape.addVertex(new Vector2(0.5f, 1.0f));
            shape.addVertex(new Vector2(0.5f, -1.0f));
            shape.addVertex(new Vector2(1.5f, -1.0f));
            shape.addVertex(new Vector2(1.5f, -2.0f));
            shape.addVertex(new Vector2(0.5f, -2.0f));
            shape.addVertex(new Vector2(-0.5f, -2.0f));
            shape.addVertex(new Vector2(-1.5f, -2.0f));
            shape.addVertex(new Vector2(-1.5f, -1.0f));
            shape.addVertex(new Vector2(-0.5f, -1.0f));
            shape.addVertex(new Vector2(-0.5f, 1.0f));
            shape.addVertex(new Vector2(-1.5f, 1.0f));
            shape.finish();

            // draggablespringbody is an inherited version of SpringBody that includes polygons for visualization, and the
            // ability to drag the body around the screen with the cursor.
            for (int x = -8; x <= 8; x += 4)
            {
                DraggableSpringBody body = new DraggableSpringBody(mWorld, shape, 1f, 150.0f, 5.0f, 300.0f, 15.0f, new Vector2(x, 0), 0.0f, Vector2.One);
                body.addInternalSpring(0, 14, 300.0f, 10.0f);
                body.addInternalSpring(1, 14, 300.0f, 10.0f);
                body.addInternalSpring(1, 15, 300.0f, 10.0f);
                body.addInternalSpring(1, 5, 300.0f, 10.0f);
                body.addInternalSpring(2, 14, 300.0f, 10.0f);
                body.addInternalSpring(2, 5, 300.0f, 10.0f);
                body.addInternalSpring(1, 5, 300.0f, 10.0f);
                body.addInternalSpring(14, 5, 300.0f, 10.0f);
                body.addInternalSpring(2, 4, 300.0f, 10.0f);
                body.addInternalSpring(3, 5, 300.0f, 10.0f);
                body.addInternalSpring(14, 6, 300.0f, 10.0f);
                body.addInternalSpring(5, 13, 300.0f, 10.0f);
                body.addInternalSpring(13, 6, 300.0f, 10.0f);
                body.addInternalSpring(12, 10, 300.0f, 10.0f);
                body.addInternalSpring(13, 11, 300.0f, 10.0f);
                body.addInternalSpring(13, 10, 300.0f, 10.0f);
                body.addInternalSpring(13, 9, 300.0f, 10.0f);
                body.addInternalSpring(6, 10, 300.0f, 10.0f);
                body.addInternalSpring(6, 9, 300.0f, 10.0f);
                body.addInternalSpring(6, 8, 300.0f, 10.0f);
                body.addInternalSpring(7, 9, 300.0f, 10.0f);

                // polygons!
                body.addTriangle(0, 15, 1);
                body.addTriangle(1, 15, 14);
                body.addTriangle(1, 14, 5);
                body.addTriangle(1, 5, 2);
                body.addTriangle(2, 5, 4);
                body.addTriangle(2, 4, 3);
                body.addTriangle(14, 13, 6);
                body.addTriangle(14, 6, 5);
                body.addTriangle(12, 11, 10);
                body.addTriangle(12, 10, 13);
                body.addTriangle(13, 10, 9);
                body.addTriangle(13, 9, 6);
                body.addTriangle(6, 9, 8);
                body.addTriangle(6, 8, 7);
                body.finalizeTriangles(Color.SpringGreen, Color.Navy);

                mSpringBodies.Add(body);
            }


            // pressure body. similar to a SpringBody, but with internal pressurized gas to help maintain shape.
            JelloPhysics.ClosedShape ball = new ClosedShape();
            ball.begin();
            for (int i = 0; i < 360; i += 20)
            {
                ball.addVertex(new Vector2((float)Math.Cos(MathHelper.ToRadians((float)-i)), (float)Math.Sin(MathHelper.ToRadians((float)-i))));
            }
            ball.finish();

            // make many of these.
            for (int x = -10; x <= 10; x+=5)
            {
                DraggablePressureBody pb = new DraggablePressureBody(mWorld, ball, 1.0f, 40.0f, 10.0f, 1.0f, 300.0f, 20.0f, new Vector2(x, -12), 0, Vector2.One);
                pb.addTriangle(0, 10, 9);
                pb.addTriangle(0, 9, 1);
                pb.addTriangle(1, 9, 8);
                pb.addTriangle(1, 8, 2);
                pb.addTriangle(2, 8, 7);
                pb.addTriangle(2, 7, 3);
                pb.addTriangle(3, 7, 6);
                pb.addTriangle(3, 6, 4);
                pb.addTriangle(4, 6, 5);
                pb.addTriangle(17, 10, 0);
                pb.addTriangle(17, 11, 10);
                pb.addTriangle(16, 11, 17);
                pb.addTriangle(16, 12, 11);
                pb.addTriangle(15, 12, 16);
                pb.addTriangle(15, 13, 12);
                pb.addTriangle(14, 12, 15);
                pb.addTriangle(14, 13, 12);
                pb.finalizeTriangles((x==-10) ? Color.Teal : Color.Maroon);
                mPressureBodies.Add(pb);
            }

            // default cursor position, etc.
            mCursorPos = new Vector2(cameraPos.X, cameraPos.Y);

            base.Initialize();
        }


        /// <summary>
        /// Load your graphics content.  If loadAllContent is true, you should
        /// load content from both ResourceManagementMode pools.  Otherwise, just
        /// load ResourceManagementMode.Manual content.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadContent()
        {
            base.LoadContent();
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //////////////////////////////////////////////////////////////////////
            GamePadState gs = GamePad.GetState(PlayerIndex.One);

            // perform several updates per frame, with a smal timestep.
            for (int i = 0; i < 5; i++)
            //if (gs.Buttons.Y == ButtonState.Pressed)
            {
                mWorld.update(1f / 200f);

                // dragging!
                if (gs.Buttons.A == ButtonState.Pressed)
                {
                    if (dragBody != null)
                    {
                        PointMass pm = dragBody.getPointMass(dragPoint);
                        if (dragBody.GetType().Name == "DraggableSpringBody")
                            ((DraggableSpringBody)dragBody).setDragForce(JelloPhysics.VectorTools.calculateSpringForce(pm.Position, pm.Velocity, mCursorPos, Vector2.Zero, 0.0f, 100.0f, 10.0f), dragPoint);
                        else if (dragBody.GetType().Name == "DraggablePressureBody")
                            ((DraggablePressureBody)dragBody).setDragForce(JelloPhysics.VectorTools.calculateSpringForce(pm.Position, pm.Velocity, mCursorPos, Vector2.Zero, 0.0f, 100.0f, 10.0f), dragPoint);

                    }
                }
                else
                {
                    dragBody = null;
                    dragPoint = -1;
                }

            }

            // QUITTING
            if (gs.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // CURSOR MOVEMENT - 360 PAD
            mCursorPos.X += (gs.ThumbSticks.Left.X * gs.ThumbSticks.Left.X) * 0.5f * ((gs.ThumbSticks.Left.X > 0) ? 1 : -1);
            mCursorPos.Y += (gs.ThumbSticks.Left.Y * gs.ThumbSticks.Left.Y) * 0.5f * ((gs.ThumbSticks.Left.Y > 0) ? 1 : -1);

            // CAMERA UPDATE
            Vector2 stray = (mCursorPos - new Vector2(cameraPos.X, cameraPos.Y));
            float maxStrayX = cameraPos.Z * 0.35f;
            float maxStrayY = (screenHeight * maxStrayX) / screenWidth;

            if (Math.Abs(stray.X) > maxStrayX)
                cameraPos.X += (stray.X > 0) ? -(maxStrayX - stray.X) : (maxStrayX + stray.X);

            if (Math.Abs(stray.Y) > maxStrayY)
                cameraPos.Y += (stray.Y > 0) ? -(maxStrayY - stray.Y) : (maxStrayY + stray.Y);

            cameraPos.Z += (gs.Triggers.Left - gs.Triggers.Right);
            if (cameraPos.Z < 5.0f) { cameraPos.Z = 5.0f; }

            // DRAGGING!
            if ((gs.Buttons.A == ButtonState.Pressed) && (oldPadState.Buttons.A == ButtonState.Released))
            {
                if (dragBody == null)
                {
                    int body;
                    mWorld.getClosestPointMass(mCursorPos, out body, out dragPoint);
                    dragBody = mWorld.getBody(body);
                }
            }

            // PRESSURE CHANGE!
            mPressureBodies[0].GasPressure += (gs.ThumbSticks.Right.Y) * 3.0f;



            // DEBUG VISUALIZATION
            if ((gs.Buttons.LeftShoulder == ButtonState.Pressed) && (oldPadState.Buttons.LeftShoulder == ButtonState.Released))
                showDebug = !showDebug;

            if ((gs.Buttons.RightShoulder == ButtonState.Pressed) && (oldPadState.Buttons.RightShoulder == ButtonState.Released))
                showVelocities = !showVelocities;

            oldPadState = gs;

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            Matrix view = Matrix.CreateLookAt(cameraPos, cameraPos + (cameraLookVector * 10.0f), Vector3.Up);
            Matrix proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                screenWidth / screenHeight,
                0.1f, 1000.0f);

            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;

            lineEffect.World = Matrix.Identity;
            lineEffect.Projection = proj;
            lineEffect.View = view;

            // PHYSICS DRAWING
            if (!showDebug)
            {
                for (int i = 0; i < mSpringBodies.Count; i++)
                    mSpringBodies[i].drawMe(graphics.GraphicsDevice, lineEffect);

                for (int i = 0; i < mPressureBodies.Count; i++)
                    mPressureBodies[i].drawMe(graphics.GraphicsDevice, lineEffect);

                for (int i = 0; i < mStaticBodies.Count; i++)
                    mStaticBodies[i].debugDrawMe(graphics.GraphicsDevice, lineEffect);
            }
            else
            {
                // draw all the bodies in debug mode, to confirm physics.
                mWorld.debugDrawMe(graphics.GraphicsDevice, lineEffect);
                mWorld.debugDrawAllBodies(graphics.GraphicsDevice, lineEffect, true);
            }

            if (showVelocities)
                mWorld.debugDrawPointVelocities(graphics.GraphicsDevice, lineEffect);            


            ///////////////////////////////////////////////////////////////////////
            // now draw the CURSOR and line to show dragging.
            graphics.GraphicsDevice.RenderState.PointSize = 10.0f;

            VertexPositionColor[] cursor = new VertexPositionColor[1];
            cursor[0].Position = JelloPhysics.VectorTools.vec3FromVec2(mCursorPos);
            cursor[0].Color = Color.Red;

            VertexPositionColor[] dragLine = new VertexPositionColor[2];
            if (dragBody != null)
            {
                dragLine[0].Position = JelloPhysics.VectorTools.vec3FromVec2(dragBody.getPointMass(dragPoint).Position);
                dragLine[0].Color = Color.Tan;

                dragLine[1].Position = JelloPhysics.VectorTools.vec3FromVec2(mCursorPos);
                dragLine[1].Color = Color.Wheat;
            }
                       
            lineEffect.Begin();
            foreach (EffectPass pass in lineEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.PointList, cursor, 0, 1);

                if (dragBody != null)
                    graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, dragLine, 0, 1);

                pass.End();
            }
            lineEffect.End();

            base.Draw(gameTime);
        }
    }
}
