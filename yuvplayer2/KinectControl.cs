using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;

//using Microsoft.Samples.Kinect.ControlsBasics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
namespace yuvplayer2
{
    class KinectControl
    {
        private KinectSensor sensor;
        public BodyFrameReader bfReader;
        public Body []Bodies;
        //public List<CameraSpacePoint> handpos;
        public int ControlState;
        public CameraSpacePoint startpos;
        private CameraSpacePoint priorpos;
        private CameraSpacePoint Currentpos;
        public bool addone = false;
        public bool subone = false;
        public int n = 0;
        private const float CtlWidth=0.05f;
        public bool InitialKinect()
        {
            
            sensor = KinectSensor.GetDefault();
          
            bfReader = sensor.BodyFrameSource.OpenReader();
            
          
            ControlState = 0;
            return true;
        }

        public bool StartKinect()
        {
            sensor.Open();
          
            return true;
        }

        public bool UpdateData()
        {
            bfReader.FrameArrived += bfReader_FrameArrived;
            return true;
        }
        public bool CloseKinect()
        {
            if (sensor.IsOpen)
                sensor.Close();
            return true;
        }
       private void bfReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame bdframe = e.FrameReference.AcquireFrame())
            {
                

                if (null != bdframe)
                {
                    Bodies = new Body[bdframe.BodyCount];
                    bdframe.GetAndRefreshBodyData(Bodies);

                    Body bodyselected = selectTracker(Bodies);
                    if (bodyselected != null)
                    {
                        bdframe.BodyFrameSource.OverrideHandTracking(bodyselected.TrackingId);
                        n++;
                        if (bodyselected.HandRightConfidence == TrackingConfidence.High && bodyselected.HandRightState == HandState.Closed)
                        {
                            startpos = bodyselected.Joints[JointType.HandRight].Position;
                            priorpos = startpos;
                            ControlState = 1;
                        }

                        if (ControlState == 1)
                        {

                            Currentpos = bodyselected.Joints[JointType.HandRight].Position;
                            float dirflag = Currentpos.Y - startpos.Y;//正反转方向
                            float addOrsub = Currentpos.X - priorpos.X;

                            if (Math.Abs(addOrsub) > CtlWidth)
                            {
                                if (addOrsub > CtlWidth)
                                {
                                    if (dirflag > 0)
                                    {
                                        addone = true;
                                        subone = false;
                                    }
                                    else
                                    {
                                        addone = false;
                                        subone = false;
                                    }

                                }
                                else
                                {
                                    if (dirflag > 0)
                                    {
                                        addone = false;
                                        subone = true;
                                    }
                                    else
                                    {
                                        addone = true;
                                        subone = false;
                                    }
                                }
                            }

                        }
                    }

                }
            }
        }
        private Body selectTracker(Body[] candidates)
       {
           Body tempbody = null;
            foreach(Body body in candidates)
            {
                if(body.IsTracked)
                {
                    if(tempbody==null)
                    {
                        tempbody = body;
                    }
                    else 
                    {
                        if(tempbody.Joints[JointType.SpineMid].Position.Z>body.Joints[JointType.SpineMid].Position.Z)
                        {
                            tempbody = body;
                        }
                    }
                }
            }
            return tempbody;
       }
    }

    
}
