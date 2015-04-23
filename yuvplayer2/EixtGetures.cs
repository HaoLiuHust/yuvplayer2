using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;
namespace Gestureslib
{
   
    class ExitGetures:IGesture
    {
        public static event EventHandler GestureDetected;

        public void Update(Body body,long timestamp)
        {
            if(body!=null&&body.IsTracked)
            {
                TrackExit(body);
            }
            else
            {
                if (GestureDetected != null)
                    GestureDetected(this, new EventArgs());
            }
        }
        private void TrackExit(Body body)
        {
            if (body.Joints[JointType.HandRight].Position.Y - body.Joints[JointType.HipRight].Position.Y < 0.05)
            {
                if(GestureDetected!=null)
                    GestureDetected(this, new EventArgs());
            }
        }
    }
}
