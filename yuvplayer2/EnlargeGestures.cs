using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;
namespace Gestureslib
{
    class EnlargeGestures
    {
        private const double HandRaiseUpThreadhold = 0.05f;
        private const double DistanceThreadhold=0.01;
        private const double ScaleUnit = 0.05f;
        private GestureTracker _gesturetracker;
        private const double MaxScale = 10.0;
        public delegate void EnlargeEventHandler(object sender, EnlargeEventArgs e);
        public event EnlargeEventHandler EnlargeGestureDetected;
        
        public class EnlargeEventArgs:EventArgs
        {
            public readonly double scaleindex;
            public EnlargeEventArgs(double scale)
            {
                this.scaleindex = scale;
            }
        }

        public void Update(Body body)
        {
            if(body!=null&&body.IsTracked)
            {         
                TrackEnlarge(body, ref _gesturetracker);
            }
            else
            {
                _gesturetracker.Reset();
            }
        }

        private void TrackEnlarge(Body body,ref GestureTracker gesturetracker)
        {
            if(body.TrackingId!=gesturetracker.trackid)
            {
                gesturetracker.Reset();
            }
            Joint handleft = body.Joints[JointType.HandLeft];
            Joint handright = body.Joints[JointType.HandRight];
            Joint Elbowright = body.Joints[JointType.ElbowRight];
            Joint Elbowleft = body.Joints[JointType.ElbowLeft];
            bool IsRightHandRaise = (handright.Position.Y - Elbowright.Position.Y) > HandRaiseUpThreadhold;
            bool IsLeftHandRasie = (handleft.Position.Y - Elbowleft.Position.Y) > HandRaiseUpThreadhold;

            if(IsRightHandRaise&&IsRightHandRaise&&body.HandLeftState==HandState.Closed&&body.HandRightState==HandState.Closed)
            {
                double dis = Math.Abs(handright.Position.X - handleft.Position.X);
                if(gesturetracker.gesturestate==GestureState.NONE)
                {
                    gesturetracker.UpdateState(GestureState.SUCCED, dis,body.TrackingId);
                    if(EnlargeGestureDetected!=null)
                         EnlargeGestureDetected(this, new EnlargeEventArgs(gesturetracker.scaleindex));
                }
                else
                {
                    if(Math.Abs(dis-gesturetracker.predistance)>=DistanceThreadhold)
                    {
                        double scaleindex = 1.0 + (dis - gesturetracker.initialdistance) / ScaleUnit;
                        scaleindex = scaleindex < 1 + 1e-6 ? 1.0 : scaleindex;
                        scaleindex = scaleindex > MaxScale ? 10.0 : scaleindex;

                        gesturetracker.UpdateDisAndScale(dis, scaleindex);
                        if (EnlargeGestureDetected != null)
                            EnlargeGestureDetected(this, new EnlargeEventArgs(gesturetracker.scaleindex));
                    }
                }
            }
            else
            {
                gesturetracker.Reset();
            }
        }
    }

    enum GestureState
    {
        NONE,
        SUCCED
    }

    struct GestureTracker
    {
        public GestureState gesturestate;
        public double initialdistance;
        public double predistance;
        public ulong trackid;
        public double scaleindex;
        public void UpdateDisAndScale(double dis,double scale)
        {
            predistance = dis;
            this.scaleindex = scale;

        }
        public void UpdateState(GestureState state,double dis,ulong trackid)
        {
            gesturestate=state;
            predistance = dis;
            initialdistance = dis;
            scaleindex = 1.0;
            this.trackid = trackid;
        }

        public void Reset()
        {
            gesturestate=GestureState.NONE;
            initialdistance = 0.0;
            predistance = 0.0;
            trackid = 0;
            scaleindex = 0;
        }
    }
}
