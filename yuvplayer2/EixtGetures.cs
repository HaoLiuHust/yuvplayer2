using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;
namespace Gestureslib
{
   
    class ExitGetures
    {
        public event EventHandler ExitDetected;

        public void Update(Body body)
        {
            if(body!=null&&body.IsTracked)
            {
                TrackExit(body);
            }
            else
            {
                if (ExitDetected != null)
                    ExitDetected(this, new EventArgs());
            }
        }
        private void TrackExit(Body body)
        {
            if (body.Joints[JointType.HandRight].Position.Y - body.Joints[JointType.HipRight].Position.Y < 0.05)
            {
                if(ExitDetected!=null)
                    ExitDetected(this, new EventArgs());
            }
        }
    }
}
