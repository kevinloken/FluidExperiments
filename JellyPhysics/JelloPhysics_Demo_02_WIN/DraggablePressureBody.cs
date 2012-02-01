using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace JelloDemo_02
{
    class DraggablePressureBody : JelloPhysics.PressureBody
    {
        // Variables for dragging point masses in this body.
        private Vector2 dragForce = Vector2.Zero;
        private int dragPoint = -1;

        VertexDeclaration mDecl;
        VertexPositionColor[] mVerts = null;
        int[] mIndices = null;
        List<int> mIndexList;
        Color mColor = Color.White;

        public DraggablePressureBody(JelloPhysics.World w, JelloPhysics.ClosedShape s, float massPerPoint, float gasPressure,
            float shapeSpringK, float shapeSpringDamp, float edgeSpringK, float edgeSpringDamp, Vector2 pos, float angleInRadians, Vector2 scale)
            : base(w, s, massPerPoint, gasPressure, shapeSpringK, shapeSpringDamp, edgeSpringK, edgeSpringDamp, pos, angleInRadians, scale, false)
        {
            mIndexList = new List<int>();
        }

        public void setDragForce(Vector2 force, int pm)
        {
            dragForce = force;
            dragPoint = pm;
        }

        // add an indexed triangle to this primitive.
        public void addTriangle(int A, int B, int C)
        {
            mIndexList.Add(A);
            mIndexList.Add(B);
            mIndexList.Add(C);
        }

        // finalize triangles
        public void finalizeTriangles(Color c)
        {
            mVerts = new VertexPositionColor[mPointMasses.Count];

            mIndices = new int[mIndexList.Count];
            for (int i = 0; i < mIndexList.Count; i++)
                mIndices[i] = mIndexList[i];

            mColor = c;
        }

        // add gravity, and drag force.
        public override void accumulateExternalForces()
        {
            base.accumulateExternalForces();

            // gravity.
            for (int i = 0; i < mPointMasses.Count; i++)
                mPointMasses[i].Force += new Vector2(0.0f, -9.8f * mPointMasses[i].Mass);

            if (dragPoint != -1)
                mPointMasses[dragPoint].Force += dragForce;

            dragPoint = -1;
        }


        public void drawMe(GraphicsDevice device, Effect effect)
        {
            if (mDecl == null)
            {
                mDecl = new VertexDeclaration(device, VertexPositionColor.VertexElements);
            }

            // update vert buffer.
            for (int i = 0; i < mPointMasses.Count; i++)
            {
                mVerts[i].Position = JelloPhysics.VectorTools.vec3FromVec2(mPointMasses[i].Position);

                float dist = (mPointMasses[i].Position - mGlobalShape[i]).Length() * 2.0f;
                if (dist > 1f) { dist = 1f; }

                mVerts[i].Color = mColor;
            }

            device.VertexDeclaration = mDecl;

            // draw me!
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, mVerts, 0, mVerts.Length, mIndices, 0, mIndices.Length / 3);
                pass.End();
            }
            effect.End();
        }
    }
}
