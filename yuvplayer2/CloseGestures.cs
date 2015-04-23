using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;
namespace Gestureslib
{
    class CloseGestures:IGesture
    {
        public delegate void CloseEventHandler(object sender, CloseEventArs e);
        public static event CloseEventHandler GestureDetected;
        private CloseGestureTracker gestureTracker;
        private const double HandStrechThreadholdX = 0.30f;
        private const double HandStrechThreadholdY = 0.30f;
        private const double HandStrechThreadholdZ = 0.30f;
        private const int CLOSE_THREADHOLD = 3000;

        public class CloseEventArs : EventArgs
        {
            public readonly CloseGestureState gesturestate;
            public readonly long timeleft;
            public CloseEventArs(CloseGestureState state, long time)
            {
                gesturestate = state;
                timeleft = time;
            }
        }
        public void Update(Body body, long timestamp)
        {
            if (body != null && body.IsTracked)
            {
                TrackCloseGesture(body, timestamp);
            }
            else
            {
                gestureTracker.Reset();
            }
        }
        private void TrackCloseGesture(Body body, long timestamp)
        {
            if (body.TrackingId != gestureTracker.trackid)
            {
                gestureTracker.Reset();
            }
            Joint handright = body.Joints[JointType.HandRight];
            Joint Shoulderright = body.Joints[JointType.ShoulderRight];
            bool IsHandExit = false;
            if (handright.TrackingState == TrackingState.Tracked && Shoulderright.TrackingState == TrackingState.Tracked)
            {
                IsHandExit = (handright.Position.X - Shoulderright.Position.X) > HandStrechThreadholdX
                && Math.Abs(handright.Position.Y - Shoulderright.Position.Y) < HandStrechThreadholdY
                && Math.Abs(handright.Position.Z - Shoulderright.Position.Z) < HandStrechThreadholdZ;

                if (IsHandExit)
                {
                    if (gestureTracker.gesturestate == CloseGestureState.INPROGRESS && gestureTracker.timestamp + CLOSE_THREADHOLD < timestamp)
                    {
                        gestureTracker.UpdateState(CloseGestureState.SUCCEED, body.TrackingId, timestamp);
                        if (GestureDetected != null)
                            GestureDetected(this, new CloseEventArs(gestureTracker.gesturestate, 0));
                    }
                    else if (gestureTracker.gesturestate != CloseGestureState.INPROGRESS)
                    {
                        gestureTracker.UpdateState(CloseGestureState.INPROGRESS, body.TrackingId, timestamp);
                        long timeremain = CLOSE_THREADHOLD - (timestamp - gestureTracker.timestamp);
                        if (GestureDetected != null)
                            GestureDetected(this, new CloseEventArs(gestureTracker.gesturestate, CLOSE_THREADHOLD));
                    }

                    long timeleft = CLOSE_THREADHOLD - (timestamp - gestureTracker.timestamp);
                    if(timeleft==2000)
                    {
                        if (GestureDetected != null)
                            GestureDetected(this, new CloseEventArs(gestureTracker.gesturestate, timeleft));
                    }
                    else if(timeleft==1000)
                    {
                        if (GestureDetected != null)
                            GestureDetected(this, new CloseEventArs(gestureTracker.gesturestate, timeleft));
                    }
                   
                }
                else
                {
                    if(gestureTracker.gesturestate!=CloseGestureState.NONE)
                    {
                        if (GestureDetected != null)
                            GestureDetected(this, new CloseEventArs(CloseGestureState.NONE, 0));
                    }
                    gestureTracker.Reset();
                }
            }
            else
            {
                if (gestureTracker.gesturestate != CloseGestureState.NONE)
                {
                    if (GestureDetected != null)
                        GestureDetected(this, new CloseEventArs(CloseGestureState.NONE, 0));
                }
                gestureTracker.Reset();
            }
        }
    }

    enum CloseGestureState
    {
        NONE,
        INPROGRESS,
        SUCCEED
    }

    struct CloseGestureTracker
    {
        public CloseGestureState gesturestate;
        public long timestamp;
        public ulong trackid;

        public void UpdateState(CloseGestureState state,ulong id,long timestamp)
        {
            if(state!=gesturestate)
            {
                gesturestate = state;
                trackid = id;
                this.timestamp = timestamp;
            }        
        }

        public void Reset()
        {
            gesturestate = CloseGestureState.NONE;
            timestamp = 0;
        }
    }
}
