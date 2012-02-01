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
    /// contains base functionality for all bodies in the JelloPhysics world.  all bodies are
    /// made up of a ClosedShape geometry, and a list of PointMass objects equal to the number of vertices in the
    /// ClosedShape geometry.  The vertices are considered to be connected by lines in order, which creates the collision
    /// volume for this body.  Individual implementations of Body handle forcing the body to keep it's shape through
    /// various methods.
    /// </summary>
    public class Body
    {
        #region PRIVATE VARIABLES
        protected ClosedShape mBaseShape;
        protected Vector2[] mGlobalShape;
        protected List<PointMass> mPointMasses;
        protected Vector2 mScale;
        protected Vector2 mDerivedPos;
        protected Vector2 mDerivedVel;
        protected float mDerivedAngle;
        protected float mDerivedOmega;
        protected float mLastAngle;
        protected AABB mAABB;
        protected int mMaterial;
        protected bool mIsStatic;
        protected bool mKinematic;
        protected object mObjectTag;

        protected float mVelDamping = 0.999f;

        //// debug visualization variables
        VertexDeclaration mVertexDecl = null;
        #endregion

        #region INTERNAL VARIABLES
        internal Bitmask mBitMaskX = new Bitmask();
        internal Bitmask mBitMaskY = new Bitmask();
        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// default constructor.
        /// </summary>
        /// <param name="w">world to add this body to (done automatically)</param>
        public Body( World w )
        {
            mAABB = new AABB();
            mBaseShape = null;
            mGlobalShape = null;
            mPointMasses = new List<PointMass>();
            mScale = Vector2.One;
            mIsStatic = false;
            mKinematic = false;

            mMaterial = 0;

            w.addBody(this);
        }

        /// <summary>
        /// create a body, and set its shape and position immediately
        /// </summary>
        /// <param name="w">world to add this body to (done automatically)</param>
        /// <param name="shape">closed shape for this body</param>
        /// <param name="massPerPoint">mass for each PointMass to be created</param>
        /// <param name="position">global position of the body</param>
        /// <param name="angleInRadians">global angle of the body</param>
        /// <param name="scale">local scale of the body</param>
        /// <param name="kinematic">whether this body is kinematically controlled</param>
        public Body(World w, ClosedShape shape, float massPerPoint, Vector2 position, float angleInRadians, Vector2 scale, bool kinematic)
        {
            mAABB = new AABB();
            mDerivedPos = position;
            mDerivedAngle = angleInRadians;
            mLastAngle = mDerivedAngle;
            mScale = scale;
            mMaterial = 0;
            mIsStatic = float.IsPositiveInfinity(massPerPoint);
            mKinematic = kinematic;

            mPointMasses = new List<PointMass>();
            setShape(shape);
            for (int i = 0; i < mPointMasses.Count; i++)
                mPointMasses[i].Mass = massPerPoint;

            updateAABB(0f, true);

            w.addBody(this);
        }

        /// <summary>
        /// create a body, and set its shape and position immediately - with individual masses for each PointMass.
        /// </summary>
        /// <param name="w">world to add this body to (done automatically)</param>
        /// <param name="shape">closed shape for this body</param>
        /// <param name="pointMasses">list of masses for each PointMass</param>
        /// <param name="position">global position of the body</param>
        /// <param name="angleInRadians">global angle of the body</param>
        /// <param name="scale">local scale of the body</param>
        /// <param name="kinematic">whether this body is kinematically controlled.</param>
        public Body(World w, ClosedShape shape, List<float> pointMasses, Vector2 position, float angleInRadians, Vector2 scale, bool kinematic)
        {
            mAABB = new AABB();
            mDerivedPos = position;
            mDerivedAngle = angleInRadians;
            mLastAngle = mDerivedAngle;
            mScale = scale;
            mMaterial = 0;
            mIsStatic = false;
            mKinematic = kinematic;

            mPointMasses = new List<PointMass>();
            setShape(shape);
            for (int i = 0; i < mPointMasses.Count; i++)
                mPointMasses[i].Mass = pointMasses[i];

            updateAABB(0f, true);

            w.addBody(this);
        }
        #endregion

        #region SETTING SHAPE
        /// <summary>
        /// set the shape of this body to a new ClosedShape object.  This function 
        /// will remove any existing PointMass objects, and replace them with new ones IF
        /// the new shape has a different vertex count than the previous one.  In this case
        /// the mass for each newly added point mass will be set zero.  Otherwise the shape is just
        /// updated, not affecting the existing PointMasses.
        /// </summary>
        /// <param name="shape">new closed shape</param>
        public void setShape(ClosedShape shape)
        {
            mBaseShape = shape;

            if (mBaseShape.Vertices.Count != mPointMasses.Count)
            {
                mPointMasses.Clear();
                mGlobalShape = new Vector2[mBaseShape.Vertices.Count];
                
                mBaseShape.transformVertices(ref mDerivedPos, mDerivedAngle, ref mScale, ref mGlobalShape);

                for (int i = 0; i < mBaseShape.Vertices.Count; i++)
                    mPointMasses.Add(new PointMass(0.0f, mGlobalShape[i]));               
            }
        }
        #endregion

        #region SETTING MASS
        /// <summary>
        /// set the mass for each PointMass in this body.
        /// </summary>
        /// <param name="mass">new mass</param>
        public void setMassAll(float mass)
        {
            for (int i = 0; i < mPointMasses.Count; i++)
                mPointMasses[i].Mass = mass;

            if (float.IsPositiveInfinity(mass)) { mIsStatic = true; }
        }

        /// <summary>
        /// set the mass for each PointMass individually.
        /// </summary>
        /// <param name="index">index of the PointMass</param>
        /// <param name="mass">new mass</param>
        public void setMassIndividual( int index, float mass )
        {
            if ((index >= 0) && (index < mPointMasses.Count))
                mPointMasses[index].Mass = mass;
        }

        /// <summary>
        /// set the mass for all point masses from a list.
        /// </summary>
        /// <param name="masses">list of masses (count MUSE equal PointMasses.Count)</param>
        public void setMassFromList(List<float> masses)
        {
            if (masses.Count == mPointMasses.Count)
            {
                for (int i = 0; i < mPointMasses.Count; i++)
                    mPointMasses[i].Mass = masses[i];
            }
        }
        #endregion

        #region MATERIAL
        /// <summary>
        /// Material for this body.  Used for physical interaction and collision notification.
        /// </summary>
        public int Material
        {
            get { return mMaterial; }
            set { mMaterial = value; }
        }
        #endregion

        #region SETTING POSITION AND ANGLE MANUALLY
        /// <summary>
        /// Set the position and angle of the body manually.
        /// </summary>
        /// <param name="pos">global position</param>
        /// <param name="angleInRadians">global angle</param>
        public virtual void setPositionAngle(Vector2 pos, float angleInRadians, Vector2 scale)
        {
            mBaseShape.transformVertices(ref pos, angleInRadians, ref scale, ref mGlobalShape);
            for (int i = 0; i < mPointMasses.Count; i++)
                mPointMasses[i].Position = mGlobalShape[i];

            mDerivedPos = pos;
            mDerivedAngle = angleInRadians;
        }

        /// <summary>
        /// For moving a body kinematically.  sets the position in global space.  via shape-matching, the
        /// body will eventually move to this location.
        /// </summary>
        /// <param name="pos">position in global space.</param>
        public virtual void setKinematicPosition(ref Vector2 pos)
        {
            mDerivedPos = pos;
        }

        /// <summary>
        /// For moving a body kinematically.  sets the angle in global space.  via shape-matching, the
        /// body will eventually rotate to this angle.
        /// </summary>
        /// <param name="angleInRadians"></param>
        public virtual void setKinematicAngle(float angleInRadians)
        {
            mDerivedAngle = angleInRadians;
        }

        /// <summary>
        /// For changing a body kinematically.  via shape matching, the body will eventually
        /// change to the given scale.
        /// </summary>
        /// <param name="scale"></param>
        public virtual void setKinematicScale(ref Vector2 scale)
        {
            mScale = scale;
        }
        #endregion

        #region DERIVING POSITION AND VELOCITY
        /// <summary>
        /// Derive the global position and angle of this body, based on the average of all the points.
        /// This updates the DerivedPosision, DerivedAngle, and DerivedVelocity properties.
        /// This is called by the World object each Update(), so usually a user does not need to call this.  Instead
        /// you can juse access the DerivedPosition, DerivedAngle, DerivedVelocity, and DerivedOmega properties.
        /// </summary>
        public void derivePositionAndAngle(float elaspsed)
        {
            // no need it this is a static body, or kinematically controlled.
            if (mIsStatic || mKinematic)
                return;

            // find the geometric center.
            Vector2 center = new Vector2();
            center.X = 0;
            center.Y = 0;

            Vector2 vel = new Vector2();
            vel.X = 0f;
            vel.Y = 0f;

            for (int i = 0; i < mPointMasses.Count; i++)
            {
                center.X += mPointMasses[i].Position.X;
                center.Y += mPointMasses[i].Position.Y;

                vel.X += mPointMasses[i].Velocity.X;
                vel.Y += mPointMasses[i].Velocity.Y;
            }

            center.X /= mPointMasses.Count;
            center.Y /= mPointMasses.Count;

            vel.X /= mPointMasses.Count;
            vel.Y /= mPointMasses.Count;

            mDerivedPos = center;
            mDerivedVel = vel;

            // find the average angle of all of the masses.
            float angle = 0;
            int originalSign = 1;
            float originalAngle = 0;
            for (int i = 0; i < mPointMasses.Count; i++)
            {
                Vector2 baseNorm = new Vector2();
                baseNorm.X = mBaseShape.Vertices[i].X;
                baseNorm.Y = mBaseShape.Vertices[i].Y;
                Vector2.Normalize(ref baseNorm, out baseNorm);

                Vector2 curNorm = new Vector2();
                curNorm.X = mPointMasses[i].Position.X - mDerivedPos.X;
                curNorm.Y = mPointMasses[i].Position.Y - mDerivedPos.Y;
                Vector2.Normalize(ref curNorm, out curNorm);

                float dot;
                Vector2.Dot(ref baseNorm, ref curNorm, out dot);
                if (dot > 1.0f) { dot = 1.0f; }
                if (dot < -1.0f) { dot = -1.0f; }

                float thisAngle = (float)Math.Acos(dot);
                if (!JelloPhysics.VectorTools.isCCW(ref baseNorm, ref curNorm)) { thisAngle = -thisAngle; }

                if (i == 0)
                {
                    originalSign = (thisAngle >= 0.0f) ? 1 : -1;
                    originalAngle = thisAngle;
                }
                else
                {
                    float diff = (thisAngle - originalAngle);
                    int thisSign = (thisAngle >= 0.0f) ? 1 : -1;

                    if ((Math.Abs(diff) > Math.PI) && (thisSign != originalSign))
                    {
                        thisAngle = (thisSign == -1) ? ((float)Math.PI + ((float)Math.PI + thisAngle)) : (((float)Math.PI - thisAngle) - (float)Math.PI);
                    }
                }

                angle += thisAngle;
            }

            angle /= mPointMasses.Count;
            mDerivedAngle = angle;

            // now calculate the derived Omega, based on change in angle over time.
            float angleChange = (mDerivedAngle - mLastAngle);
            if (Math.Abs(angleChange) >= Math.PI)
            {
                if (angleChange < 0f)
                    angleChange = angleChange + (float)(Math.PI * 2);
                else
                    angleChange = angleChange - (float)(Math.PI * 2);
            }

            mDerivedOmega = angleChange / elaspsed;

            mLastAngle = mDerivedAngle;
        }

        /// <summary>
        /// Derived position of the body in global space, based on location of all PointMasses.
        /// </summary>
        public Vector2 DerivedPosition
        {
            get { return mDerivedPos; }
        }

        /// <summary>
        /// Derived global angle of the body in global space, based on location of all PointMasses.
        /// </summary>
        public float DerivedAngle
        {
            get { return mDerivedAngle; }
        }

        /// <summary>
        /// Derived global velocity of the body in global space, based on velocity of all PointMasses.
        /// </summary>
        public Vector2 DerivedVelocity
        {
            get { return mDerivedVel; }
        }

        /// <summary>
        /// Derived rotational velocity of the body in global space, based on changes in DerivedAngle.
        /// </summary>
        public float DerivedOmega
        {
            get { return mDerivedOmega; }
        }
        #endregion

        #region ACCUMULATING FORCES - TO BE INHERITED!
        /// <summary>
        /// this function should add all internal forces to the Force member variable of each PointMass in the body.
        /// these should be forces that try to maintain the shape of the body.
        /// </summary>
        public virtual void accumulateInternalForces() {}

        /// <summary>
        /// this function should add all external forces to the Force member variable of each PointMass in the body.
        /// these are external forces acting on the PointMasses, such as gravity, etc.
        /// </summary>
        public virtual void accumulateExternalForces() {}
        #endregion

        #region INTEGRATION
        internal void integrate(float elapsed)
        {
            if (mIsStatic) { return; }

            for (int i = 0; i < mPointMasses.Count; i++)
                mPointMasses[i].integrateForce(elapsed);
        }

        internal void dampenVelocity()
        {
            if (mIsStatic) { return; }

            for (int i = 0; i < mPointMasses.Count; i++)
            {
                mPointMasses[i].Velocity.X *= mVelDamping;
                mPointMasses[i].Velocity.Y *= mVelDamping;
            }
        }
        #endregion

        #region HELPER FUNCTIONS
        /// <summary>
        /// update the AABB for this body, including padding for velocity given a timestep.
        /// This function is called by the World object on Update(), so the user should not need this in most cases.
        /// </summary>
        /// <param name="elapsed">elapsed time in seconds</param>
        public void updateAABB(float elapsed, bool forceUpdate)
        {
            if ((!IsStatic) || (forceUpdate))
            {
                mAABB.clear();
                for (int i = 0; i < mPointMasses.Count; i++)
                {
                    Vector2 p = mPointMasses[i].Position;
                    mAABB.expandToInclude(ref p);

                    // expanding for velocity only makes sense for dynamic objects.
                    if (!IsStatic)
                    {
                        p.X += (mPointMasses[i].Velocity.X * elapsed);
                        p.Y += (mPointMasses[i].Velocity.Y * elapsed);
                        mAABB.expandToInclude(ref p);
                    }
                }
            }
        }

        /// <summary>
        /// get the Axis-aligned bounding box for this body.  used for broad-phase collision checks.
        /// </summary>
        /// <returns>AABB for this body</returns>
        public AABB getAABB()
        {
            return mAABB;
        }

        /// <summary>
        /// collision detection.  detect if a global point is inside this body.
        /// </summary>
        /// <param name="pt">point in global space</param>
        /// <returns>true = point is inside body, false = it is not.</returns>
        public bool contains(ref Vector2 pt)
        {
            // basic idea: draw a line from the point to a point known to be outside the body.  count the number of
            // lines in the polygon it intersects.  if that number is odd, we are inside.  if it's even, we are outside.
            // in this implementation we will always use a line that moves off in the positive X direction from the point
            // to simplify things.
            Vector2 endPt = new Vector2();
            endPt.X = mAABB.Max.X + 0.1f;
            endPt.Y = pt.Y;

            // line we are testing against goes from pt -> endPt.
            bool inside = false;
            Vector2 edgeSt = mPointMasses[0].Position;
            Vector2 edgeEnd = new Vector2();
            int c = mPointMasses.Count;
            for (int i = 0; i < c; i++)
            {
                // the current edge is defined as the line from edgeSt -> edgeEnd.
                if (i < (c - 1))
                    edgeEnd = mPointMasses[i + 1].Position;
                else
                    edgeEnd = mPointMasses[0].Position;

                // perform check now...
                if (((edgeSt.Y <= pt.Y) && (edgeEnd.Y > pt.Y)) || ((edgeSt.Y > pt.Y) && (edgeEnd.Y <= pt.Y)))
                {
                    // this line crosses the test line at some point... does it do so within our test range?
                    float slope = (edgeEnd.X - edgeSt.X) / (edgeEnd.Y - edgeSt.Y);
                    float hitX = edgeSt.X + ((pt.Y - edgeSt.Y) * slope);

                    if ((hitX >= pt.X) && (hitX <= endPt.X))
                        inside = !inside;
                }
                edgeSt = edgeEnd;
            }

            return inside;
        }

        /// <summary>
        /// collision detection - given a global point, find the point on this body that is closest to the global point,
        /// and if it is an edge, information about the edge it resides on.
        /// </summary>
        /// <param name="pt">global point</param>
        /// <param name="hitPt">returned point on the body in global space</param>
        /// <param name="normal">returned normal on the body in global space</param>
        /// <param name="pointA">returned ptA on the edge</param>
        /// <param name="pointB">returned ptB on the edge</param>
        /// <param name="edgeD">scalar distance between ptA and ptB [0,1]</param>
        /// <returns>distance</returns>
        public float getClosestPoint( Vector2 pt, out Vector2 hitPt, out Vector2 normal, out int pointA, out int pointB, out float edgeD )
        {
            hitPt = Vector2.Zero;
            pointA = -1;
            pointB = -1;
            edgeD = 0f;
            normal = Vector2.Zero;

            float closestD = 1000.0f;

            for (int i = 0; i < mPointMasses.Count; i++)
            {
                Vector2 tempHit;
                Vector2 tempNorm;
                float tempEdgeD;

                float dist = getClosestPointOnEdge(pt, i, out tempHit, out tempNorm, out tempEdgeD);
                if (dist < closestD)
                {
                    closestD = dist;
                    pointA = i;
                    if (i < (mPointMasses.Count - 1))
                        pointB = i + 1;
                    else
                        pointB = 0;
                    edgeD = tempEdgeD;
                    normal = tempNorm;
                    hitPt = tempHit;
                }
            }


            // return.
            return closestD;
        }

        /// <summary>
        /// find the distance from a global point in space, to the closest point on a given edge of the body.
        /// </summary>
        /// <param name="pt">global point</param>
        /// <param name="edgeNum">edge to check against.  0 = edge from pt[0] to pt[1], etc.</param>
        /// <param name="hitPt">returned point on edge in global space</param>
        /// <param name="normal">returned normal on edge in global space</param>
        /// <param name="edgeD">returned distance along edge from ptA to ptB [0,1]</param>
        /// <returns>distance</returns>
        public float getClosestPointOnEdge(Vector2 pt, int edgeNum, out Vector2 hitPt, out Vector2 normal, out float edgeD)
        {
            hitPt = new Vector2();
            hitPt.X = 0f;
            hitPt.Y = 0f;

            normal = new Vector2();
            normal.X = 0f;
            normal.Y = 0f;

            edgeD = 0f;
            float dist = 0f;

            Vector2 ptA = mPointMasses[edgeNum].Position;
            Vector2 ptB = new Vector2();

            if (edgeNum < (mPointMasses.Count - 1))
                ptB = mPointMasses[edgeNum + 1].Position;
            else
                ptB = mPointMasses[0].Position;

            Vector2 toP = new Vector2();
            toP.X = pt.X - ptA.X;
            toP.Y = pt.Y - ptA.Y;

            Vector2 E = new Vector2();
            E.X = ptB.X - ptA.X;
            E.Y = ptB.Y - ptA.Y;

            // get the length of the edge, and use that to normalize the vector.
            float edgeLength = (float)Math.Sqrt((E.X * E.X) + (E.Y * E.Y));
            if (edgeLength > 0.00001f)
            {
                E.X /= edgeLength;
                E.Y /= edgeLength;
            }

            // normal
            Vector2 n = new Vector2();
            VectorTools.getPerpendicular(ref E, ref n);

            // calculate the distance!
            float x;
            Vector2.Dot(ref toP, ref E, out x);
            if (x <= 0.0f)
            {
                // x is outside the line segment, distance is from pt to ptA.
                //dist = (pt - ptA).Length();
                Vector2.Distance(ref pt, ref ptA, out dist);
                hitPt = ptA;
                edgeD = 0f;
                normal = n;
            }
            else if (x >= edgeLength)
            {
                // x is outside of the line segment, distance is from pt to ptB.
                //dist = (pt - ptB).Length();
                Vector2.Distance(ref pt, ref ptB, out dist);
                hitPt = ptB;
                edgeD = 1f;
                normal = n;
            }
            else
            {
                // point lies somewhere on the line segment.
                Vector3 toP3 = new Vector3();
                toP3.X = toP.X;
                toP3.Y = toP.Y;

                Vector3 E3 = new Vector3();
                E3.X = E.X;
                E3.Y = E.Y;

                //dist = Math.Abs(Vector3.Cross(toP3, E3).Z);
                Vector3.Cross(ref toP3, ref E3, out E3);
                dist = Math.Abs(E3.Z);
                hitPt.X = ptA.X + (E.X * x);
                hitPt.Y = ptA.Y + (E.Y * x);
                edgeD = x / edgeLength;
                normal = n;
            }

            return dist;
        }

        /// <summary>
        /// find the squared distance from a global point in space, to the closest point on a given edge of the body.
        /// </summary>
        /// <param name="pt">global point</param>
        /// <param name="edgeNum">edge to check against.  0 = edge from pt[0] to pt[1], etc.</param>
        /// <param name="hitPt">returned point on edge in global space</param>
        /// <param name="normal">returned normal on edge in global space</param>
        /// <param name="edgeD">returned distance along edge from ptA to ptB [0,1]</param>
        /// <returns>distance</returns>
        public float getClosestPointOnEdgeSquared(Vector2 pt, int edgeNum, out Vector2 hitPt, out Vector2 normal, out float edgeD)
        {
            hitPt = new Vector2();
            hitPt.X = 0f;
            hitPt.Y = 0f;

            normal = new Vector2();
            normal.X = 0f;
            normal.Y = 0f;

            edgeD = 0f;
            float dist = 0f;

            Vector2 ptA = mPointMasses[edgeNum].Position;
            Vector2 ptB = new Vector2();

            if (edgeNum < (mPointMasses.Count - 1))
                ptB = mPointMasses[edgeNum + 1].Position;
            else
                ptB = mPointMasses[0].Position;

            Vector2 toP = new Vector2();
            toP.X = pt.X - ptA.X;
            toP.Y = pt.Y - ptA.Y;

            Vector2 E = new Vector2();
            E.X = ptB.X - ptA.X;
            E.Y = ptB.Y - ptA.Y;

            // get the length of the edge, and use that to normalize the vector.
            float edgeLength = (float)Math.Sqrt((E.X * E.X) + (E.Y * E.Y));
            if (edgeLength > 0.00001f)
            {
                E.X /= edgeLength;
                E.Y /= edgeLength;
            }

            // normal
            Vector2 n = new Vector2();
            VectorTools.getPerpendicular(ref E, ref n);

            // calculate the distance!
            float x;
            Vector2.Dot(ref toP, ref E, out x);
            if (x <= 0.0f)
            {
                // x is outside the line segment, distance is from pt to ptA.
                //dist = (pt - ptA).Length();
                Vector2.DistanceSquared(ref pt, ref ptA, out dist);
                hitPt = ptA;
                edgeD = 0f;
                normal = n;
            }
            else if (x >= edgeLength)
            {
                // x is outside of the line segment, distance is from pt to ptB.
                //dist = (pt - ptB).Length();
                Vector2.DistanceSquared(ref pt, ref ptB, out dist);
                hitPt = ptB;
                edgeD = 1f;
                normal = n;
            }
            else
            {
                // point lies somewhere on the line segment.
                Vector3 toP3 = new Vector3();
                toP3.X = toP.X;
                toP3.Y = toP.Y;

                Vector3 E3 = new Vector3();
                E3.X = E.X;
                E3.Y = E.Y;

                //dist = Math.Abs(Vector3.Cross(toP3, E3).Z);
                Vector3.Cross(ref toP3, ref E3, out E3);
                dist = Math.Abs(E3.Z * E3.Z);
                hitPt.X = ptA.X + (E.X * x);
                hitPt.Y = ptA.Y + (E.Y * x);
                edgeD = x / edgeLength;
                normal = n;
            }

            return dist;
        }


        /// <summary>
        /// Find the closest PointMass in this body, givena global point.
        /// </summary>
        /// <param name="pos">global point</param>
        /// <param name="dist">returned dist</param>
        /// <returns>index of the PointMass</returns>
        public int getClosestPointMass(Vector2 pos, out float dist)
        {
            float closestSQD = 100000.0f;
            int closest = -1;

            for (int i = 0; i < mPointMasses.Count; i++)
            {
                float thisD = (pos - mPointMasses[i].Position).LengthSquared();
                if (thisD < closestSQD)
                {
                    closestSQD = thisD;
                    closest = i;
                }
            }

            dist = (float)Math.Sqrt(closestSQD);
            return closest;
        }

        /// <summary>
        /// Number of PointMasses in the body
        /// </summary>
        public int PointMassCount
        {
            get { return mPointMasses.Count; }
        }

        /// <summary>
        /// Get a specific PointMass from this body.
        /// </summary>
        /// <param name="index">index</param>
        /// <returns>PointMass</returns>
        public PointMass getPointMass(int index)
        {
            return mPointMasses[index];
        }

        /// <summary>
        /// Helper function to add a global force acting on this body as a whole.
        /// </summary>
        /// <param name="pt">location of force, in global space</param>
        /// <param name="force">direction and intensity of force, in global space</param>
        public void addGlobalForce(ref Vector2 pt, ref Vector2 force)
        {
            Vector2 R = (mDerivedPos - pt);
            
            float torqueF = Vector3.Cross(JelloPhysics.VectorTools.vec3FromVec2(R), JelloPhysics.VectorTools.vec3FromVec2(force)).Z;

            for (int i = 0; i < mPointMasses.Count; i++)
            {
                Vector2 toPt = (mPointMasses[i].Position - mDerivedPos);
                Vector2 torque = JelloPhysics.VectorTools.rotateVector(toPt, -(float)(Math.PI) / 2f);

                mPointMasses[i].Force += torque * torqueF;

                mPointMasses[i].Force += force;
            }
        }
        #endregion

        #region DEBUG VISUALIZATION
        /// <summary>
        /// This function draws the points to the screen as lines, showing several things:
        /// WHITE - actual PointMasses, connected by lines
        /// GREY - baseshape, at the derived position and angle.
        /// </summary>
        /// <param name="device">graphics device</param>
        /// <param name="effect">effect to use (MUST implement VertexColors)</param>
        public virtual void debugDrawMe(GraphicsDevice device, Effect effect)
        {
            if (mVertexDecl == null)
            {
                mVertexDecl = new VertexDeclaration(device, VertexPositionColor.VertexElements);
            }


            ////////////////////////////////////////////////////////////////////////////
            // fill the debug verts with derived positions.
            mBaseShape.transformVertices(ref mDerivedPos, mDerivedAngle, ref mScale, ref mGlobalShape);

            VertexPositionColor[] debugVerts = new VertexPositionColor[mGlobalShape.Length + 1];
            for (int i = 0; i < mGlobalShape.Length; i++)
            {
                debugVerts[i].Position = VectorTools.vec3FromVec2(mGlobalShape[i]);
                debugVerts[i].Color = Color.Gray;
            }

            debugVerts[debugVerts.Length - 1].Position = VectorTools.vec3FromVec2(mGlobalShape[0]);
            debugVerts[debugVerts.Length - 1].Color = Color.Gray;

            device.VertexDeclaration = mVertexDecl;
            device.RenderState.PointSize = 5.0f;

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, debugVerts, 0, mGlobalShape.Length);
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.PointList, debugVerts, 0, mGlobalShape.Length);
                pass.End();
            }
            effect.End();

            ////////////////////////////////////////////////////////////////////////////
            // fill the debug verts with global positions.
            for (int i = 0; i < mPointMasses.Count; i++)
            {
                debugVerts[i].Position = VectorTools.vec3FromVec2(mPointMasses[i].Position);
                debugVerts[i].Color = Color.White;
            }

            debugVerts[debugVerts.Length - 1].Position = VectorTools.vec3FromVec2(mPointMasses[0].Position);
            debugVerts[debugVerts.Length - 1].Color = Color.White;

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, debugVerts, 0, mPointMasses.Count);
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.PointList, debugVerts, 0, mPointMasses.Count);

                // derived center.
                VertexPositionColor[] center = new VertexPositionColor[1];
                center[0].Position = VectorTools.vec3FromVec2(mDerivedPos);
                center[0].Color = Color.IndianRed;
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.PointList, center, 0, 1);

                // UP and LEFT vectors.
                VertexPositionColor[] axis = new VertexPositionColor[4];
                axis[0].Position = VectorTools.vec3FromVec2(mDerivedPos);
                axis[0].Color = Color.OrangeRed;
                axis[1].Position = VectorTools.vec3FromVec2(mDerivedPos + (VectorTools.rotateVector(Vector2.UnitY, mDerivedAngle)));
                axis[1].Color = Color.OrangeRed;

                axis[2].Position = VectorTools.vec3FromVec2(mDerivedPos);
                axis[2].Color = Color.YellowGreen;
                axis[3].Position = VectorTools.vec3FromVec2(mDerivedPos + (VectorTools.rotateVector(Vector2.UnitX, mDerivedAngle)));
                axis[3].Color = Color.YellowGreen;

                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, axis, 0, 2);
                pass.End();
            }
            effect.End();
            
            
        }

        /// <summary>
        /// Draw the AABB for this body, for debug purposes.
        /// </summary>
        /// <param name="device">graphics device</param>
        /// <param name="effect">effect to use (MUST implement VertexColors)</param>
        public void debugDrawAABB(GraphicsDevice device, Effect effect)
        {
            AABB box = getAABB();

            if (mVertexDecl == null)
            {
                mVertexDecl = new VertexDeclaration(device, VertexPositionColor.VertexElements);
            }

            // draw the world limits.
            VertexPositionColor[] limits = new VertexPositionColor[5];

            limits[0].Position = new Vector3(box.Min.X, box.Max.Y, 0);
            limits[0].Color = Color.SlateGray;

            limits[1].Position = new Vector3(box.Max.X, box.Max.Y, 0);
            limits[1].Color = Color.SlateGray;

            limits[2].Position = new Vector3(box.Max.X, box.Min.Y, 0);
            limits[2].Color = Color.SlateGray;

            limits[3].Position = new Vector3(box.Min.X, box.Min.Y, 0);
            limits[3].Color = Color.SlateGray;

            limits[4].Position = new Vector3(box.Min.X, box.Max.Y, 0);
            limits[4].Color = Color.SlateGray;

            device.VertexDeclaration = mVertexDecl;
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, limits, 0, 4);
                pass.End();
            }
            effect.End();
        }
        #endregion

        #region PUBLIC PROPERTIES
        /// <summary>
        /// Gets / Sets whether this is a static body.  setting static greatly improves performance on static bodies.
        /// </summary>
        public bool IsStatic
        {
            get { return mIsStatic; }
            set { mIsStatic = value; }
        }

        /// <summary>
        /// Sets whether this body is kinematically controlled.  kinematic control requires shape-matching forces to work properly.
        /// </summary>
        public bool IsKinematic
        {
            get { return mKinematic; }
            set { mKinematic = value; }
        }

        public float VelocityDamping
        {
            get { return mVelDamping; }
            set { mVelDamping = value; }
        }

        public object ObjectTag
        {
            get { return mObjectTag; }
            set { mObjectTag = value; }
        }
        #endregion
    }
}
