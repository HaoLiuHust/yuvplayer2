using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;
namespace yuvplayer2
{
    interface IGesture
    {
        void Update(Body body, BodyFrame bframe = null);
    }
}
