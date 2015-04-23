using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.Kinect;
using Microsoft.Kinect.Input;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Wpf.Controls;
using Microsoft.Kinect.Toolkit.Input;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;

using Gestureslib;
namespace yuvplayer2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region args
        private int width ;
        private int height ;
        private int picsize;
        private int datasize;
        private int m_tot_view,m_tot_frame;
        private int current_frame, current_view;
       // private bool playstatus;
        byte[] yuvframeL, yuvframeR;
        Image<Bgr, byte> bgrimage;
        byte[] mergedframe;
        
        public List<CameraSpacePoint> handpos;
        public bool addone = false;
        public bool subone = false;
        ImageViewer imageviewer;//播放窗口
        private int screenWidth, screenHeight;
        #endregion args

        KinectControl kinectcontrol;
        //private Gestures _gestures;
        public Body[] Bodies;
        //配置文件路径
        private const string cfgpath = "video.cfg";
        public MainWindow()
        {
            InitializeComponent();
            
            this.Loaded += MainWindow_Loaded;
            this.Focusable = true;
            this.Closed += MainWindow_Closed;
        }

        /// <summary>
        /// 窗体载入时获取和设置一些信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.kinectregion.KinectSensor = KinectSensor.GetDefault();
            KinectRegion.SetKinectRegion(this, kinectregion);
            App app = ((App)Application.Current);
            app.KinectRegion = kinectregion;           
            app.KinectRegion.CursorSpriteSheetDefinition = KinectRegion.DefaultSpriteSheet;
            m_tot_view = 0;
            current_frame = 0;
            current_view = 0;
            m_tot_frame = 0;
            //playstatus = false;
            screenWidth = (int)System.Windows.SystemParameters.PrimaryScreenWidth;
            screenHeight = (int)System.Windows.SystemParameters.PrimaryScreenHeight;
        }
        void MainWindow_Closed(object sender, EventArgs e)
        {
            exitApp();
        }

        private void exitApp()
        {
            if(this.kinectregion.KinectSensor.IsOpen)
            {
                this.kinectregion.KinectSensor.Close();
            }
            this.kinectregion.KinectSensor = null;
        }
        
        
        private FileStream[] yuvFile = new FileStream[100];
        
        [DllImport("imageop.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MergeAndConv(byte[] leftimg, byte[] rightimg, byte[] dstimg, int width, int height);
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (imageviewer != null)
                return;
            string videopath;
            string widthstr,heightstr;
            string[] strsplit=new string[2];
            StreamReader sr = new StreamReader(cfgpath, Encoding.Default);
            if(sr==null)
            {
                MessageBox.Show("file does not exist");
                return;
            }
            videopath = sr.ReadLine();
            widthstr = sr.ReadLine();
            heightstr = sr.ReadLine();

            strsplit=widthstr.Split(':');
            width = System.Int32.Parse(strsplit[1]);
            strsplit = heightstr.Split(':');
            height = System.Int32.Parse(strsplit[1]);
            OpenFile(videopath);
            KinectProcess();
            Gestures.AddGesture(SupportedGestures.ENLARGE);
            Gestures.AddGesture(SupportedGestures.CLOSE);
            Gestures.AddGesture(SupportedGestures.CIRCLE);
            Gestures.AddGesture(SupportedGestures.EXIT);
            EnlargeGestures.GestureDetected += _EnlargeGestures_EnlargeGestureDetected;
            CloseGestures.GestureDetected += _closeGestures_CloseGestureDetected;
            ExitGetures.GestureDetected += _exitGestures_ExitDetected;
            CircleControlGestures.GestureDetected += _circleGestures_CircleGestureDetected;
            Gestures.CreateGestures();
        }

       

        #region gestures
        private void KinectProcess()
        {
            kinectcontrol = new KinectControl();
            kinectcontrol.InitialKinect();
            kinectcontrol.StartKinect();
            Emgu.CV.CvInvoke.cvEllipse(bgrimage, new System.Drawing.Point(bgrimage.Width >> 2, bgrimage.Height * 3 >> 2), new System.Drawing.Size(20, 20), 0.0, 0.0, 360.0, new MCvScalar(0, 0, 255), 2, Emgu.CV.CvEnum.LINE_TYPE.CV_AA, 0);
            Emgu.CV.CvInvoke.cvEllipse(bgrimage, new System.Drawing.Point(bgrimage.Width * 3 >> 2, bgrimage.Height * 3 >> 2), new System.Drawing.Size(20, 20), 0.0, 0.0, 360.0, new MCvScalar(0, 0, 255), 2, Emgu.CV.CvEnum.LINE_TYPE.CV_AA, 0);
            imageviewer.Image = bgrimage.Resize(imageviewer.Width, imageviewer.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            kinectcontrol.bfReader.FrameArrived += bfReader_FrameArrived;
        }

        void bfReader_FrameArrived(object sender, Microsoft.Kinect.BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame bdframe = e.FrameReference.AcquireFrame())
            {
                if (null != bdframe)
                {
                    Bodies = new Body[bdframe.BodyCount];
                    bdframe.GetAndRefreshBodyData(Bodies);

                    //选择最近的对象
                    Body bodyselected = (from s in Bodies where s.IsTracked && s.Joints[JointType.SpineMid].TrackingState == TrackingState.Tracked select s).OrderBy(s => s.Joints[JointType.SpineMid].Position.Z).FirstOrDefault();
                    if (bodyselected != null)
                        bdframe.BodyFrameSource.OverrideHandTracking(bodyselected.TrackingId);
                    if(imageviewer!=null&&!imageviewer.IsDisposed)
                    {
                        foreach (var s in Gestures.gesturelist)
                            s.Update(bodyselected, (long)bdframe.RelativeTime.TotalMilliseconds);
                    }                  
                }
            }
        }
        void _closeGestures_CloseGestureDetected(object sender, CloseGestures.CloseEventArs e)
        {
            if (e.gesturestate == CloseGestureState.INPROGRESS)
            {
                int time = (int)(e.timeleft / 1000);
                string stime = time.ToString();
                PlayOneFrame(current_view, ref current_frame);
                MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_PLAIN, 10.0, 10.0);
                bgrimage.Draw(stime, ref font, new System.Drawing.Point(bgrimage.Width >> 2, bgrimage.Height * 3 >> 2), new Bgr(0, 255, 0));
                bgrimage.Draw(stime, ref font, new System.Drawing.Point(bgrimage.Width * 3 >> 2, bgrimage.Height * 3 >> 2), new Bgr(0, 255, 0));
                imageviewer.Image = bgrimage.Resize(imageviewer.Width, imageviewer.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            }
            else if (e.gesturestate == CloseGestureState.NONE)
            {
                PlayOneFrame(current_view, ref current_frame);
                Emgu.CV.CvInvoke.cvEllipse(bgrimage, new System.Drawing.Point(bgrimage.Width >> 2, bgrimage.Height * 3 >> 2), new System.Drawing.Size(20, 20), 0.0, 0.0, 360.0, new MCvScalar(0, 0, 255), 2, Emgu.CV.CvEnum.LINE_TYPE.CV_AA, 0);
                Emgu.CV.CvInvoke.cvEllipse(bgrimage, new System.Drawing.Point(bgrimage.Width * 3 >> 2, bgrimage.Height * 3 >> 2), new System.Drawing.Size(20, 20), 0.0, 0.0, 360.0, new MCvScalar(0, 0, 255), 2, Emgu.CV.CvEnum.LINE_TYPE.CV_AA, 0);
                imageviewer.Image = bgrimage.Resize(imageviewer.Width, imageviewer.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            }
            else
            {
                CloseImageview();
            }
        }
        void _circleGestures_CircleGestureDetected(object sender, CircleControlGestures.CircleEventArgs e)
        {
            if(e.CircleState==CircleGestureState.FORWARD)
            {
                ++current_frame;
                PlayOneFrame(current_view, ref current_frame);
            }
            else if(e.CircleState==CircleGestureState.BACKWARD)
            {
                --current_frame;
                PlayOneFrame(current_view, ref current_frame);
            }
            else if(e.CircleState==CircleGestureState.START)
            {
                Emgu.CV.CvInvoke.cvEllipse(bgrimage, new System.Drawing.Point(bgrimage.Width >> 2, bgrimage.Height * 3 >> 2), new System.Drawing.Size(20, 20), 0.0, 0.0, 360.0, new MCvScalar(0, 255, 0), 2, Emgu.CV.CvEnum.LINE_TYPE.CV_AA, 0);
                Emgu.CV.CvInvoke.cvEllipse(bgrimage, new System.Drawing.Point(bgrimage.Width * 3 >> 2, bgrimage.Height * 3 >> 2), new System.Drawing.Size(20, 20), 0.0, 0.0, 360.0, new MCvScalar(0, 255, 0), 2, Emgu.CV.CvEnum.LINE_TYPE.CV_AA, 0);
                imageviewer.Image = bgrimage.Resize(imageviewer.Width, imageviewer.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            }
            else
            {
                _exitGestures_ExitDetected(sender, new EventArgs());
            }
        }

        void _exitGestures_ExitDetected(object sender, EventArgs e)
        {
            Emgu.CV.CvInvoke.cvEllipse(bgrimage, new System.Drawing.Point(bgrimage.Width >> 2, bgrimage.Height * 3 >> 2), new System.Drawing.Size(20, 20), 0.0, 0.0, 360.0, new MCvScalar(0, 0, 255), 2, Emgu.CV.CvEnum.LINE_TYPE.CV_AA, 0);
            Emgu.CV.CvInvoke.cvEllipse(bgrimage, new System.Drawing.Point(bgrimage.Width * 3 >> 2, bgrimage.Height * 3 >> 2), new System.Drawing.Size(20, 20), 0.0, 0.0, 360.0, new MCvScalar(0, 0, 255), 2, Emgu.CV.CvEnum.LINE_TYPE.CV_AA, 0);

            imageviewer.Image = bgrimage.Resize(imageviewer.Width, imageviewer.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
        }

        void _EnlargeGestures_EnlargeGestureDetected(object sender, EnlargeGestures.EnlargeEventArgs e)
        {
            PlayOneFrame(current_view, ref current_frame, e.scaleindex);
        }
        #endregion gestures

        private void OpenFile(string fileName)
        {
            picsize = width * height;
            datasize = (picsize >> 1) * 3;
            yuvframeL = new byte[(picsize >> 1) * 3];
            yuvframeR = new byte[(picsize >> 1) * 3];
            bgrimage = new Image<Bgr, byte>(width * 2, height);
            mergedframe = new byte[bgrimage.Width * bgrimage.Height * 3];
            if (!File.Exists(fileName))
            {
                MessageBox.Show("file does not exist");
                return;
            }
            if (File.OpenRead(fileName).CanRead)
            {
                yuvFile[m_tot_view] = File.OpenRead(fileName);
                m_tot_view++;
            }
           
            int Count = 0;//指示数字位数
           
            if (Char.IsNumber(fileName, fileName.Length - 5))
            {
                Count++;
                if (Char.IsNumber(fileName, fileName.Length - 6))
                {
                    Count++;
                    if (Char.IsNumber(fileName, fileName.Length - 7))
                        Count++;
                }
            }
            else
            {
                MessageBox.Show("错误的文件名格式，最后一位需为数字！", "error");
                Environment.Exit(0);
            }
            // MessageBox.Show(Count.ToString(), "Count");
            //分配文件流
            for (int i = Int32.Parse(fileName.Substring(fileName.Length - 4 - Count, Count)) + 1; ; i++)
            {
                var filestr = i.ToString();
                if (i < 10)
                {
                    fileName = fileName.Remove(fileName.Length - 5, 1);
                }
                else if (i < 100)
                {
                    fileName = fileName.Remove(fileName.Length - 6, 2);
                }
                else
                {
                    fileName = fileName.Remove(fileName.Length - 7, 3);
                }
                fileName = fileName.Insert(fileName.Length - 4, filestr);
                if (File.Exists(fileName) && File.OpenRead(fileName).CanRead)
                {
                    yuvFile[m_tot_view] = File.OpenRead(fileName);
                    m_tot_view++;
                }
                else { break; }
            }

           if(m_tot_view<2)
           {
               MessageBox.Show("至少要有2个视点", "error");
               Environment.Exit(0);
           }
           m_tot_frame = (int)(yuvFile[0].Length / yuvframeL.Length);
           imageviewer = new ImageViewer();
           imageviewer.Select();//激活窗口
                
           imageviewer.KeyDown += imageviewer_KeyDown;//定义快捷键操作
           imageviewer.WindowState = System.Windows.Forms.FormWindowState.Normal;
           imageviewer.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

           imageviewer.ImageBox.FunctionalMode = ImageBox.FunctionalModeOption.Minimum;
           ImageFullScreen(ref imageviewer);
           PlayOneFrame(current_view, ref current_frame);
        }

        #region display
        /// <summary>
        /// 读取一帧
        /// </summary>
        /// <param name="currentview">当前视点</param>
        /// <param name="pos">当前帧号</param>
       private void ReadOneFrame(int currentview, ref int pos)
       {
           if (currentview > m_tot_view - 2)
           {
               currentview = 0;
           }
           if (pos >= m_tot_frame)
           {
               pos = 0;
           }
           else if (pos < 0)
           {
               pos = m_tot_frame - 1;
           }
           yuvFile[currentview].Seek(pos * yuvframeL.Length, SeekOrigin.Begin);
           yuvFile[currentview + 1].Seek(pos * yuvframeR.Length, SeekOrigin.Begin);

           yuvFile[currentview].Read(yuvframeL, 0, yuvframeL.Length);
           yuvFile[currentview + 1].Read(yuvframeR, 0, yuvframeR.Length);

       }

        /// <summary>
        /// 播放一帧
        /// </summary>
        /// <param name="currentview">当前视点</param>
        /// <param name="framenum">帧号</param>
        /// <param name="scale">放大尺度，默认为1</param>
        private void PlayOneFrame(int currentview,ref int framenum,double scale=1.0)
       {
           ReadOneFrame(currentview, ref framenum);
           MergeAndConv(yuvframeL, yuvframeR, mergedframe, width, height);
           bgrimage.Bytes = mergedframe;
           if (scale<1.0+1e-6)
           {
               CvInvoke.cvResetImageROI(bgrimage);
               imageviewer.Image = bgrimage.Resize(imageviewer.Width, imageviewer.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);

           }
           else
           {
               Image<Bgr, byte> leftimage = new Image<Bgr, byte>(width, height);
               Image<Bgr, byte> rightimage = new Image<Bgr, byte>(width, height);

               int dx = (int)Math.Round(width / scale);
               int dy = (int)Math.Round(height / scale);
               int x = (int)Math.Round((width - dx) / 2.0);
               int y = (int)Math.Round((height - dy) / 2.0);

               CvInvoke.cvSetImageROI(bgrimage, new System.Drawing.Rectangle(x, y, dx, dy));
               leftimage = bgrimage.Resize(width, height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
               CvInvoke.cvSetImageROI(bgrimage, new System.Drawing.Rectangle(0, 0, width, height));
               leftimage.CopyTo(bgrimage);
               CvInvoke.cvSetImageROI(bgrimage, new System.Drawing.Rectangle(x + width - 1, y, dx, dy));
               rightimage = bgrimage.Resize(width, height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
               CvInvoke.cvSetImageROI(bgrimage, new System.Drawing.Rectangle(width - 1, 0, width, height));
               rightimage.CopyTo(bgrimage);
               leftimage.Dispose();
               rightimage.Dispose();
               leftimage = null;
               rightimage = null;
               CvInvoke.cvResetImageROI(bgrimage);
               imageviewer.Image = bgrimage.Resize(imageviewer.Width, imageviewer.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
           }
           imageviewer.Show();
       }
        /// <summary>
        /// 关闭播放窗口
        /// </summary>
        private void CloseImageview()
        {
            if (imageviewer != null)
            {
                if (kinectcontrol.bfReader != null)
                    kinectcontrol.bfReader.FrameArrived -= bfReader_FrameArrived;
                foreach(var s in Gestures.gesturelist)
                {
                    Gestures.gesturelist.Remove(s);
                }
                foreach(var s in Gestures.gestureset)
                {
                    Gestures.gestureset.Remove(s);
                }
                EnlargeGestures.GestureDetected -= _EnlargeGestures_EnlargeGestureDetected;
                ExitGetures.GestureDetected -= _exitGestures_ExitDetected;
                CloseGestures.GestureDetected -= _closeGestures_CloseGestureDetected;
                CircleControlGestures.GestureDetected -= _circleGestures_CircleGestureDetected;
                imageviewer.Close();
                imageviewer.Dispose();
            }
        }
        /// <summary>
        /// 播放窗口全屏
        /// </summary>
        /// <param name="view"></param>
       private void ImageFullScreen(ref ImageViewer view)
       {

               imageviewer.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;             
               imageviewer.TopMost = true;
               if (this.Left > screenWidth)
               {
                   imageviewer.Left = screenWidth;
                   imageviewer.Top = 0;
                   imageviewer.Width = (int)(System.Windows.SystemParameters.VirtualScreenWidth - System.Windows.SystemParameters.PrimaryScreenWidth);
                   imageviewer.Height = (int)System.Windows.SystemParameters.VirtualScreenHeight;
                   imageviewer.Image = bgrimage.Resize(imageviewer.Width, imageviewer.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
               }
               else
               {
                   imageviewer.Left = 0;
                   imageviewer.Top = 0;
                   imageviewer.Width = (int)System.Windows.SystemParameters.PrimaryScreenWidth;
                   imageviewer.Height = (int)System.Windows.SystemParameters.PrimaryScreenHeight;
                   imageviewer.Image = bgrimage.Resize(imageviewer.Width, imageviewer.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
               }                        
       }

       void imageviewer_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
       {
           switch (e.KeyCode)
           {
               case System.Windows.Forms.Keys.Left: current_frame--; PlayOneFrame(current_view, ref current_frame); break;
               case System.Windows.Forms.Keys.Right: current_frame++; PlayOneFrame(current_view, ref current_frame); break;
               case System.Windows.Forms.Keys.Escape: CloseImageview(); break;
               default: break;

           }
       }
        #endregion display
       /// <summary>
        /// 启动Kinect
        /// </summary>
      

        /*
        private bool DetectCrossHands(Body body)
       {
            const double ThreadholdZ = 0.20;
            const double ThreadholdY=0.1;
            const double ThreadholdX = 0.1;
           Joint Handleft = body.Joints[JointType.HandLeft];
           Joint Handright = body.Joints[JointType.HandRight];
           Joint Elbowleft = body.Joints[JointType.ElbowLeft];
           Joint Elbowright = body.Joints[JointType.ElbowRight];

           CameraSpacePoint middleright, middleleft;
           middleright.X = (Handright.Position.X + Elbowright.Position.X)/2;
           middleright.Y = (Handright.Position.Y + Elbowright.Position.Y) / 2;
           middleright.Z = (Handright.Position.Z + Elbowright.Position.Z) / 2;
           middleleft.X = (Handleft.Position.X + Elbowleft.Position.X) / 2;
           middleleft.Y = (Handleft.Position.Y + Elbowleft.Position.Y) / 2;
           middleleft.Z = (Handleft.Position.Z + Elbowleft.Position.Z) / 2;

           bool Isx = Math.Abs(middleright.X - middleleft.X) < ThreadholdX;
           bool Isy = Math.Abs(middleright.Y - middleleft.Y) < ThreadholdY;
           bool Isz = Math.Abs(middleright.Z - middleleft.Z) < ThreadholdZ;

           var angleright = Math.Atan2(Handright.Position.Y - Elbowright.Position.Y, Elbowright.Position.X - Handright.Position.X)*180/Math.PI;
           var angleleft = Math.Atan2(Handleft.Position.Y - Elbowleft.Position.Y, Handleft.Position.X - Elbowleft.Position.X) * 180 / Math.PI;

           bool angletr = angleright >= 20 && angleright < 90;
           bool angletl = angleleft >= 20 && angleleft < 90;

           //FileStream file = new FileStream("a.txt", FileMode.OpenOrCreate | FileMode.Append);
           //StreamWriter sw = new StreamWriter(file);
           //sw.WriteLine(angleright.ToString());
           //sw.WriteLine(angleleft.ToString());
           //sw.Close();
           //file.Close();
           //angletr = true;
           //angletl = true;
            if(Isx&&Isy&&Isz&&angletl&&angletr)
            {
                return true;
            }

            return false;
       }*/
    }
}

