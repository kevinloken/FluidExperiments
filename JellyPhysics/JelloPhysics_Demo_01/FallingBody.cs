using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace JelloDemo_01_WIN
{
    // simple inherited class, to add gravity force.
    class FallingBody : JelloPhysics.SpringBody
    {
        public FallingBody(JelloPhysics.World w, JelloPhysics.ClosedShape s, float massPerPoint,
            float edgeSpringK, float edgeSpringDamp, Vector2 pos, float angle, Vector2 scale)
            :
            base(w, s, massPerPoint, edgeSpringK, edgeSpringDamp, pos, angle, scale, false)
        {
        }

        public override void accumulateExternalForces()
        {
            base.accumulateExternalForces();

            // gravity!
            for (int i = 0; i < mPointMasses.Count; i++)
                mPointMasses[i].Force += new Vector2(0f, -9.8f * mPointMasses[i].Mass);
        }
    }
}
