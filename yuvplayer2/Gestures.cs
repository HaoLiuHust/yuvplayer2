using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gestureslib;
namespace Gestureslib
{
    class Gestures
    {
        public EnlargeGestures _enlargeGestures;
        public ExitGetures _exitGestures;
        public CircleControlGestures _circleGestures;
        public CloseGestures _closeGestures;
        public Gestures()
        {
            _enlargeGestures = new EnlargeGestures();
            _exitGestures = new ExitGetures();
            _circleGestures = new CircleControlGestures();
            _closeGestures = new CloseGestures();
        }
    }
}
