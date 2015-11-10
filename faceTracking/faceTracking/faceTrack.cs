using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*** Add namespace from EMGU CV ***/
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;



namespace faceTracking
{
    public partial class faceTrack : Form
    {

        Capture capturecam = null;       //instance for capture using webcam
        //bool CapturingProcess = false;  //boolean stating the capturing process status

        private CascadeClassifier _cascadeClassifierFace, _cascadeClassifierEye;
       
        private bool _captureInProgress = false;

        private List<Rectangle> faces = new List<Rectangle>();
        private List<Rectangle> eyes = new List<Rectangle>();
        private List<Point> _trajFace = new List<Point>();

        private Point _centerFace = new Point(0, 0);
        private Point _centerEye = new Point(0, 0);
        private Point _centerEyePrev = new Point(0, 0);
        private Boolean _firstLine = false;
        private LineSegment2D _lindeRef = new LineSegment2D(new Point(0, 0), new Point((int)CapProp.FrameWidth, (int)CapProp.FrameHeight));
        //private Image<Bgr, Byte> frameRegion;

        public bool udpConnected = false;


        public faceTrack()
        {
            InitializeComponent();

            CvInvoke.UseOpenCL = false;

            _cascadeClassifierFace = new CascadeClassifier(Application.StartupPath + "/haarcascade_frontalface_default.xml");
            _cascadeClassifierEye = new CascadeClassifier(Application.StartupPath + "/haarcascade_eye.xml");

            try
            {
                capturecam = new Capture(0);
                capturecam.SetCaptureProperty(CapProp.Fps, 30);
                capturecam.SetCaptureProperty(CapProp.FrameHeight, 240);
                capturecam.SetCaptureProperty(CapProp.FrameWidth, 320);
                capturecam.SetCaptureProperty(CapProp.AutoExposure, 1);

                trackBarUpdate(trackBarContrast, (int)capturecam.GetCaptureProperty(CapProp.Contrast));
                trackBarUpdate(trackBarBrightness, (int)capturecam.GetCaptureProperty(CapProp.Brightness));
                //trackBarUpdate(trackBarGain, (int)capturecam.GetCaptureProperty(CapProp.Gain));
                trackBarUpdate(trackBarZoom, (int)capturecam.GetCaptureProperty(CapProp.Zoom));

                textBoxTime.Text = "Time: ";
                textBoxCodec.Text = "Codec: ";
                textBoxFrameRate.Text = "Frame: ";

                capturecam.ImageGrabbed += ProcessFrame;
                //Application.Idle += ProcessFrame;

                //original.Image = capturecam.QueryFrame();
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
        }


        private void ProcessFrame(object sender, EventArgs arg)
        {
            try
            {
                if (capturecam != null)
                {
                    Mat frame = new Mat();
                    capturecam.Retrieve(frame, 0);


                    if (frame != null)
                    {
                        _captureInProgress = true;

                        Mat cannyFrame = new Mat();
                        CvInvoke.Canny(frame, cannyFrame, 100, 60);

                        Image<Bgr, Byte> frameCanny = cannyFrame.ToImage<Bgr, Byte>();

                        Image<Bgr, Byte> frameBgr = frame.ToImage<Bgr, Byte>();
                        Image<Gray, Byte> frameGray = frame.ToImage<Gray, Byte>();

                        //CvInvoke.EqualizeHist(frameGray, frameGray); // normalizes brightness and increases contrast of the image

                        Rectangle[] facesDetected = _cascadeClassifierFace.DetectMultiScale(frameGray, 1.1, 10, new Size(20, 20));  //Size.Empty); //the actual face detection happens here

                        //faces.AddRange(facesDetected);

                        foreach (Rectangle f in facesDetected)
                        {
                            //frameBgr.Draw(f, new Bgr(Color.OrangeRed), 2); //the detected face(s) is highlighted here using a box that is drawn around it/them
                            CvInvoke.Rectangle(frameBgr, f, new Bgr(Color.OrangeRed).MCvScalar, 2);

                            //Console.WriteLine("Rect : " + f);

                            _centerFace.X = (int)(f.X + f.Width * 0.5);
                            _centerFace.Y = (int)(f.Y + f.Height * 0.5);

                            _trajFace.Add(_centerFace);


                            textBoxUpdate(textBoxPosX, "X : " + _centerFace.X.ToString());
                            textBoxUpdate(textBoxPosY, "Y : " + _centerFace.Y.ToString());

                            CvInvoke.Circle(frameBgr, _centerFace, 1, new Bgr(Color.OrangeRed).MCvScalar, 5, Emgu.CV.CvEnum.LineType.AntiAlias, 0);

                            //centerRect.
                            //Get the region of interest on the faces
                            using (UMat faceRegion = new UMat(frameGray.ToUMat(), f))
                            {
                                Rectangle[] eyesDetected = _cascadeClassifierEye.DetectMultiScale(faceRegion, 1.1, 10, new Size(20, 20));

                                foreach (Rectangle e in eyesDetected)
                                {
                                    Rectangle eyeRect = e;
                                    eyeRect.Offset(f.X, f.Y);
                                    //eyes.Add(eyeRect);
                                    //frameBgr.Draw(eyeRect, new Bgr(Color.Red), 2); //the eyes face(s) is highlighted here using a box that is drawn around it/them
                                    //CvInvoke.Rectangle(frameBgr, eyeRect, new Bgr(Color.Blue).MCvScalar, 2);


                                    _centerEye.X = (int)(eyeRect.X + eyeRect.Width * 0.5);
                                    _centerEye.Y = (int)(eyeRect.Y + eyeRect.Height * 0.5);

                                    CvInvoke.Circle(frameBgr, _centerEye, 1, new Bgr(Color.Blue).MCvScalar, 5, Emgu.CV.CvEnum.LineType.AntiAlias, 0);

                                    LineSegment2D _lindeEye = new LineSegment2D(_centerEye, _centerEyePrev);

                                    if ((_firstLine) && (_lindeEye.P1 != _lindeEye.P2)) CvInvoke.Line(frameBgr, _centerEye, _centerEyePrev, new Bgr(Color.Blue).MCvScalar, 1, Emgu.CV.CvEnum.LineType.AntiAlias, 0);

                                    _centerEyePrev = _centerEye;
                                    _firstLine = true;

                                    if ((_lindeEye.P1 != _lindeEye.P2) && (_lindeEye.P1.X != 0) && (_lindeEye.P2.X != 0) && (_lindeEye.P1.Y != 0) && (_lindeEye.P2.Y != 0))
                                    {
                                        //double angle = (Math.Cos((_lindeRef.P2.X - _lindeRef.P1.X) / ((_lindeEye.P2.X - _lindeEye.P1.X)))*180) / Math.PI;
                                        double angle = (Math.Atan2(Math.Abs(_lindeEye.P1.Y - _lindeEye.P2.Y), (_lindeEye.P1.X - _lindeEye.P2.X)) * 180 / Math.PI);

                                        //Console.WriteLine("Angle : " + angle);

                                        if (angle != 1)
                                        {

                                            //angle -= 57.0;
                                            //Console.WriteLine("Angle : " + angle);

                                            //if ((Math.Abs(angle) > 15) && (Math.Abs(angle) < 50))
                                            if (angle < 90)
                                            {

                                                textBoxUpdate(textBoxAngle, Math.Round(angle).ToString() + "° ");
                                                //frameBgr = frameBgr.Rotate(angle, new Bgr(Color.Gray), false);
                                            }
                                            else if (angle > 90)
                                            {

                                                textBoxUpdate(textBoxAngle, (180 - Math.Round(angle)).ToString() + "° ");
                                                //frameBgr = frameBgr.Rotate((180-angle), new Bgr(Color.Gray), false);
                                            }
                                        }
                                    }

                                    /*using (Image<Bgr, Byte> drawing = new Image<Bgr, Byte>(imageBoxDraw.Width, imageBoxDraw.Height))
                                    {
                                        foreach (Point p in _trajFace)
                                        {
                                            CvInvoke.Circle(drawing, p, 1, new Bgr(Color.Red).MCvScalar, 1, Emgu.CV.CvEnum.LineType.AntiAlias, 0);
                                        }
                                    
                                        imageBoxDraw.Image = drawing;
                                    
                                    }*/
                                    //Graphics g = Graphics.FromHwnd(PictureBox.h);
                                    /*Graphics g = Graphics.FromHwnd(PictureBox.FromHandle);
                                    SolidBrush brush = new SolidBrush(Color.LimeGreen);
                                    Point dPoint = new Point(_centerEye.X, _centerEye.Y);
                                    dPoint.X = dPoint.X - 2;
                                    dPoint.Y = dPoint.Y - 2;
                                    Rectangle rect = new Rectangle(dPoint, new Size(4, 4));
                                    g.FillRectangle(brush, rect);
                                    g.Dispose();*/
                                }

                                processed.Image = faceRegion;
                            }
                        }

                        /*foreach (var eye in eyes)
                        {
                            frameBgr.Draw(eye, new Bgr(Color.Red), 2); //the eyes face(s) is highlighted here using a box that is drawn around it/them
                        }

                        foreach (var face in facesDetected)
                        {
                            frameBgr.Draw(face, new Bgr(Color.OrangeRed), 2); //the detected face(s) is highlighted here using a box that is drawn around it/them
                        }*/

                        original.Image = frameBgr;
                        //processed.Image = frameCanny;

                    }
                    else _captureInProgress = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void showInfo()
        {
            if (capturecam != null)
            {
                if (_captureInProgress)
                {
                    capturecam.SetCaptureProperty(CapProp.Contrast, GetTrackBar(trackBarContrast));
                    capturecam.SetCaptureProperty(CapProp.Brightness, GetTrackBar(trackBarBrightness));
                    //capturecam.SetCaptureProperty(CapProp.Gain, GetTrackBar(trackBarGain));
                    capturecam.SetCaptureProperty(CapProp.Zoom, GetTrackBar(trackBarZoom));

                    //Console.WriteLine("Constrast" + capturecam.GetCaptureProperty(CapProp.Contrast));
                    //Console.WriteLine("Brightness" + capturecam.GetCaptureProperty(CapProp.Brightness));
                    //Console.WriteLine("Gain" + (int)capturecam.GetCaptureProperty(CapProp.Sharpness));

                    trackBarUpdate(trackBarContrast, (int)capturecam.GetCaptureProperty(CapProp.Contrast));
                    trackBarUpdate(trackBarBrightness, (int)capturecam.GetCaptureProperty(CapProp.Brightness));
                    //trackBarUpdate(trackBarGain, (int)capturecam.GetCaptureProperty(CapProp.Gain));
                    trackBarUpdate(trackBarZoom, (int)capturecam.GetCaptureProperty(CapProp.Zoom));

                    double Framesno = capturecam.GetCaptureProperty(CapProp.FrameCount);

                    double time_index = capturecam.GetCaptureProperty(CapProp.PosMsec);

                    textBoxUpdate(textBoxTime, "Time: " + TimeSpan.FromMilliseconds(time_index).ToString().Substring(0, 8));

                    double framenumber = capturecam.GetCaptureProperty(CapProp.PosFrames);

                    textBoxUpdate(textBoxFrameRate, "Frame: " + Framesno.ToString());
                }
            }
        }


        private void timer1_Tick_1(object sender, EventArgs e)
        {
            showInfo();
        }
        

        private void pause_Click(object sender, EventArgs e)
        {
            if (capturecam != null)
            {
                if (_captureInProgress)
                {  //stop the capture
                    pause.Text = "Start Capture";
                    capturecam.Pause();
                }
                else
                {
                   // if(
                    //start the capture
                    pause.Text = "Stop";
                    original.Image = null;
                    capturecam.Start();
                }

                _captureInProgress = !_captureInProgress;
            }
        }


        private void ReleaseData()
        {
            if (capturecam != null)
                capturecam.Dispose();
        }

        #region Getter & Setter

        public Point getCenterFace()
        {
            return _centerFace;    
        }

        public bool getCaptureInProgress()
        {
            return _captureInProgress;
        }

        public Capture getCapture()
        {
            return capturecam;
        }

        public TextBox getTextBoxUdp()
        {
            return textBoxUdp;
        }

        #endregion

        #region ObjectCallback

        /// <summary>
        /// thread-safe call for Windows Forms Update (asynchronous call).
        /// </summary>
        /// 
        delegate void SetTextBoxCallBack(TextBox textboxAff, string mes);
        /// <param name="message"></param>
        /// <summary>
        /// Update textBoxRS232
        /// </summary>
        /// <param name="message"></param>
        public void textBoxUpdate(TextBox textboxAff, string mes)
        {
            try
            {
                if (textboxAff.InvokeRequired) ///InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread. If these threads are different, it returns true.
                {
                    SetTextBoxCallBack d = new SetTextBoxCallBack(textBoxUpdate);
                    Invoke(d, new object[] { textboxAff, mes });
                }
                else
                {
                    textboxAff.Text = mes;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /// <summary>
        /// thread-safe call for Windows Forms Update (asynchronous call).
        /// </summary>
        /// 
        delegate void SetTrackbarCallBack(TrackBar trackBar, int value);
        /// <param name="message"></param>
        /// <summary>
        /// Update TrackBar
        /// </summary>
        /// <param name="message"></param>
        private void trackBarUpdate(TrackBar trackBar, int value)
        {
            if (trackBar.InvokeRequired) ///InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread. If these threads are different, it returns true.
            {
                SetTrackbarCallBack d = new SetTrackbarCallBack(trackBarUpdate);
                Invoke(d, new object[] { trackBar, value });
            }
            else
            {
                trackBar.Value = value;
            }
        }

        /// <summary>
        /// thread-safe call for Windows Forms Update (asynchronous call).
        /// </summary>
        /// 
        delegate int GetTrackbarCallBack(TrackBar trackBar);
        /// <param name="message"></param>
        /// <summary>
        /// Update TrackBar
        /// </summary>
        /// <param name="message"></param>
        private int GetTrackBar(TrackBar trackBar)
        {
            if (trackBar.InvokeRequired) ///InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread. If these threads are different, it returns true.
            {
                GetTrackbarCallBack d = new GetTrackbarCallBack(GetTrackBar);
                return (int) Invoke(d, new object[] { trackBar });
            }
            else
            {
                return trackBar.Value;
            }
        }

        #endregion

        private void flipHorizontalButton_Click(object sender, EventArgs e)
        {
            if (capturecam != null) capturecam.FlipHorizontal = !capturecam.FlipHorizontal;
        }

        private void faceTrack_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.WriteLine("Closing ...");
            //ReleaseData();
        }
    }
}
