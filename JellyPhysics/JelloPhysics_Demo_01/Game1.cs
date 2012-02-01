#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace JelloDemo_01_WIN
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        ContentManager content;

        JelloPhysics.World mWorld;

        Vector3 cursorPos = Vector3.Zero;

        // simple effect for drawing the physics.
        BasicEffect mEffect;

        GamePadState mOldPadState;

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
            // create physics world.
#if XBOX360
            graphics.PreferredBackBufferWidth = this.Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = this.Window.ClientBounds.Height;
            graphics.PreferMultiSampling = true;
#else
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
#endif
            graphics.ApplyChanges();

            // PHYSICS INIT!
            mWorld = new JelloPhysics.World();

            // static ground object.
            JelloPhysics.ClosedShape groundShape = new JelloPhysics.ClosedShape();
            groundShape.begin();
            groundShape.addVertex(new Vector2(-10f, -2f));
            groundShape.addVertex(new Vector2(-10f, 2f));
            groundShape.addVertex(new Vector2(10f, 2f));
            groundShape.addVertex(new Vector2(10f, -2f));
            groundShape.finish();

            // make the body.
            JelloPhysics.Body groundBody = new JelloPhysics.Body(mWorld, groundShape, float.PositiveInfinity, new Vector2(0f, -5f), 0f, Vector2.One, false);


            // visual effect
            mEffect = new BasicEffect(graphics.GraphicsDevice, null);
            mEffect.VertexColorEnabled = true;
            mEffect.LightingEnabled = false;

            base.Initialize();
        }


        /// <summary>
        /// Load your graphics content.  If loadAllContent is true, you should
        /// load content from both ResourceManagementMode pools.  Otherwise, just
        /// load ResourceManagementMode.Manual content.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void  LoadContent()
        {
            // TODO: Load any ResourceManagementMode.Manual content
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            GamePadState newState = GamePad.GetState(PlayerIndex.One);

            // Allows the game to exit
            if (newState.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // UPDATE the physics!
            for (int i = 0; i < 3; i++)
                mWorld.update(1 / 120f);

            // cursor movement.
            cursorPos += new Vector3(newState.ThumbSticks.Left.X, newState.ThumbSticks.Left.Y, 0f) * 0.3f;

            // if the user presses the A button, create a new body at the cursor position.
            if ((newState.Buttons.A == ButtonState.Pressed) && (mOldPadState.Buttons.A == ButtonState.Released))
            {
                JelloPhysics.ClosedShape shape = new JelloPhysics.ClosedShape();
                shape.begin();
                shape.addVertex(new Vector2(-1.0f, 0f));
                shape.addVertex(new Vector2(0f, 1.0f));
                shape.addVertex(new Vector2(1.0f, 0f));
                shape.addVertex(new Vector2(0f, -1.0f));
                shape.finish();

                FallingBody body = new FallingBody(mWorld, shape, 1f, 300f, 10f,
                    new Vector2(cursorPos.X, cursorPos.Y), MathHelper.ToRadians(new Random().Next(360)), Vector2.One);

                body.addInternalSpring(0, 2, 400f, 12f);
                body.addInternalSpring(1, 3, 400f, 12f);
            }


            mOldPadState = GamePad.GetState(PlayerIndex.One);
            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up);
            Matrix proj = Matrix.CreateOrthographic(30f, 22f, -10f, 10f);

            mEffect.View = view;
            mEffect.Projection = proj;
            mEffect.World = Matrix.Identity;

            mWorld.debugDrawAllBodies(graphics.GraphicsDevice, mEffect, false);


            // draw the cursor.
            VertexPositionColor[] pos = new VertexPositionColor[1];
            pos[0].Position = cursorPos;
            pos[0].Color = Color.Red;

            graphics.GraphicsDevice.RenderState.PointSize = 10f;

            mEffect.Begin();
            foreach (EffectPass pass in mEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.PointList, pos, 0, 1);
                pass.End();
            }
            mEffect.End();

            base.Draw(gameTime);
        }
    }
}
