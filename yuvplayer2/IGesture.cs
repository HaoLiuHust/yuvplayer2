using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;
namespace Gestureslib
{
    interface IGesture
    {
        void Update(Body body, long timestamp=0);
    }
}
