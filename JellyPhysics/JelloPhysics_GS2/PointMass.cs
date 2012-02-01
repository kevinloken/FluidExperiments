/*
Copyright (c) 2007 Walaber

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace JelloPhysics
{

    /// <summary>
    /// the most important class in JelloPhysics, all bodies in the world are made up of PointMasses, connected to form
    /// shapes.  Each PointMass can have its own mass, allowing for objects with different center-of-gravity.
    /// </summary>
    public class PointMass
    {
        #region PRIVATE VARIABLES
        ////////////////////////////////////////////////////////////////
        /// <summary>
        /// Mass of thie PointMass.
        /// </summary>
        public float Mass;

        /// <summary>
        /// Global position of the PointMass.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Global velocity of the PointMass.
        /// </summary>
        public Vector2 Velocity;

        /// <summary>
        /// Force accumulation variable.  reset to Zero after each call to integrate().
        /// </summary>
        public Vector2 Force;
        #endregion

        #region CONSTRUCTORS
        ////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        public PointMass()
        {
            Mass = 0;
            Position = Velocity = Force = Vector2.Zero;
        }

        public PointMass(float mass, Vector2 pos)
        {
            Mass = mass;
            Position = pos;
            Velocity = Force = Vector2.Zero;
        }
        #endregion

        #region INTEGRATION
        ////////////////////////////////////////////////////////////////
        /// <summary>
        /// integrate Force >> Velocity >> Position, and reset force to zero.
        /// this is usually called by the World.update() method, the user should not need to call it directly.
        /// </summary>
        /// <param name="elapsed">time elapsed in seconds</param>
        public void integrateForce(float elapsed)
        {
            if (Mass != float.PositiveInfinity)
            {
                float elapMass = elapsed / Mass;

                Velocity.X += (Force.X * elapMass);
                Velocity.Y += (Force.Y * elapMass);

                Position.X += (Velocity.X * elapsed);
                Position.Y += (Velocity.Y * elapsed);
            }

            Force.X = 0f;
            Force.Y = 0f;
        }
        #endregion
    }
}
