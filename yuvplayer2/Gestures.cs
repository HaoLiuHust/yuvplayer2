using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gestureslib;
namespace Gestureslib
{
    abstract class Gestures
    {
        public static HashSet<SupportedGestures> gestureset=new HashSet<SupportedGestures>();
        public static List<IGesture> gesturelist=new List<IGesture>();
        public static void AddGesture(SupportedGestures gesturename)
        {

            gestureset.Add(gesturename);
        }
        public static void DeleteGesture(SupportedGestures gesturename)
        {
                gestureset.Remove(gesturename);
        }

        public static void CreateGestures()
        {
            foreach(var s in gestureset)
            {
                switch(s)
                {
                    case SupportedGestures.ENLARGE: gesturelist.Add(new EnlargeGestures()); break;
                    case SupportedGestures.CIRCLE: gesturelist.Add(new CircleControlGestures()); break;
                    case SupportedGestures.EXIT: gesturelist.Add(new ExitGetures()); break;
                    case SupportedGestures.CLOSE: gesturelist.Add(new CloseGestures()); break;
                    default: break;
                }
            }
        }
        //public EnlargeGestures _enlargeGestures;
        //public ExitGetures _exitGestures;
        //public CircleControlGestures _circleGestures;
        //public CloseGestures _closeGestures;
        //public Gestures()
        //{
        //    _enlargeGestures = new EnlargeGestures();
        //    _exitGestures = new ExitGetures();
        //    _circleGestures = new CircleControlGestures();
        //    _closeGestures = new CloseGestures();
        //}
    }

    enum SupportedGestures
    {
        ENLARGE,
        EXIT,
        CLOSE,
        CIRCLE
    }
}
