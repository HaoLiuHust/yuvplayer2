using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
namespace Gestureslib
{
    class CircleControlGestures:IGesture
    {
        public delegate void CircleEventHandler(object sender, CircleEventArgs e);
        public static event CircleEventHandler GestureDetected;
        private CircleGestureTracker _circleTracker;
        private const double HandRaiseUpThreadhold = 0.05;
        private const double CtrlThreahold = 0.005f;
        public class CircleEventArgs:EventArgs
        {
            public readonly CircleGestureState CircleState;
            public CircleEventArgs(CircleGestureState state)
            {
                CircleState = state;
            }
        }

        public void Update(Body body,long timestamp)
        {
            if(body!=null&&body.IsTracked)
            {
                TrackCircleGesture(body);
            }
            else
            {
                _circleTracker.Reset();
            }
        }
        private void TrackCircleGesture(Body body)
        {
            if (body.TrackingId != _circleTracker.trackid)
            {
                _circleTracker.Reset();
            }
            Joint handleft = body.Joints[JointType.HandLeft];
            Joint handright = body.Joints[JointType.HandRight];
            Joint Elbowright = body.Joints[JointType.ElbowRight];
            Joint Elbowleft = body.Joints[JointType.ElbowLeft];
            bool IsAllHandsClosed = body.HandRightState == HandState.Closed && body.HandRightConfidence == TrackingConfidence.High && body.HandLeftConfidence == TrackingConfidence.High && body.HandLeftState == HandState.Closed;
            bool IsRightHandRaise = (handright.Position.Y - Elbowright.Position.Y) > HandRaiseUpThreadhold;
            bool IsLeftHandRasie = (handleft.Position.Y - Elbowleft.Position.Y) > HandRaiseUpThreadhold;
            if(_circleTracker._gesturestate==CircleGestureState.NONE)
            {
                if(body.HandRightConfidence == TrackingConfidence.High && body.HandRightState == HandState.Closed && IsRightHandRaise && !IsAllHandsClosed && !IsLeftHandRasie)
                {
                    _circleTracker.UpdateAllState(CircleGestureState.START, body.TrackingId,handright.Position, handright.Position); 
                }
                if (GestureDetected != null)
                    GestureDetected(this, new CircleEventArgs(_circleTracker._gesturestate));
            }
            else
            {
                if(IsRightHandRaise&&!IsAllHandsClosed)
                {
                   
                   CameraSpacePoint currentpos=handright.Position;
                   
                   float dirflag = currentpos.Y - _circleTracker.startposition.Y;//正反转方向
                   float addOrsub = currentpos.X - _circleTracker.preposition.X;
                    
                   if(Math.Abs(addOrsub)>CtrlThreahold)
                   {
                       if (addOrsub > CtrlThreahold)
                       {
                           if (dirflag > 0)
                           {
                               _circleTracker.UpdateState(CircleGestureState.FORWARD, currentpos);
                               if(GestureDetected!=null)
                                     GestureDetected(this, new CircleEventArgs(CircleGestureState.FORWARD));
                           }
                           else
                           {
                               _circleTracker.UpdateState(CircleGestureState.BACKWARD, currentpos);
                               if (GestureDetected != null)                               
                                    GestureDetected(this, new CircleEventArgs(CircleGestureState.BACKWARD));                               
                           }

                       }
                       else
                       {
                           if (dirflag > 0)
                           {
                               _circleTracker.UpdateState(CircleGestureState.BACKWARD, currentpos);
                               if (GestureDetected != null)                               
                                    GestureDetected(this, new CircleEventArgs(CircleGestureState.BACKWARD));                               
                           }
                           else
                           {
                               _circleTracker.UpdateState(CircleGestureState.FORWARD, currentpos);
                               if (GestureDetected != null)
                                   GestureDetected(this, new CircleEventArgs(CircleGestureState.FORWARD));                                                             
                           }
                       }
                   }
                }
                else
                {
                    _circleTracker.Reset();
                    if (GestureDetected != null)
                    {
                        GestureDetected(this, new CircleEventArgs(_circleTracker._gesturestate));
                    }
                }
            }                    
        }

    }

    enum CircleGestureState
    {
        NONE,
        START,
        FORWARD,
        BACKWARD
    }

    struct CircleGestureTracker
    {
        public CircleGestureState _gesturestate;
        public CameraSpacePoint startposition;
        public CameraSpacePoint preposition;
        public ulong trackid;
        public void UpdateAllState(CircleGestureState state,ulong trackid, CameraSpacePoint start, CameraSpacePoint pre)
        {
            _gesturestate = state;
            startposition = start;
            preposition = pre;
            this.trackid = trackid;
        }

        public void UpdateState(CircleGestureState state, CameraSpacePoint pre)
        {
            _gesturestate = state;
            preposition = pre;
        }

        public void Reset()
        {
            _gesturestate = CircleGestureState.NONE;
            startposition = new CameraSpacePoint { X=0,Y=0,Z=0};
            preposition = startposition;
            trackid = 0;
        }
    }
}
