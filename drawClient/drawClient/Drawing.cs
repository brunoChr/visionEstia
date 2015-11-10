using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;

namespace drawClient
{
    public partial class Drawing : Form
    {
        private comm _com;
        private Point position = new Point();
        private string data;

        public Drawing(comm _com)
        {
            InitializeComponent();
            this._com = _com;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this._com.flagDataReceived)
            {
                data = _com.getDataReceived();

                TryParsePoint(data, out position);

                textBoxUpdate(textBoxPosition, data);

                drawrectangle(pictureBox1, position, 3, Color.Black);
            }
        }


        public static bool TryParsePoint(string s, out System.Drawing.Point p)
        {
            p = new System.Drawing.Point();
            string s1 = "{X=";
            string s2 = ",Y=";
            string s3 = "}";
            int x1 = s.IndexOf(s1, StringComparison.OrdinalIgnoreCase);
            int x2 = s.IndexOf(s2, StringComparison.OrdinalIgnoreCase);
            int x3 = s.IndexOf(s3, StringComparison.OrdinalIgnoreCase);
            if (x1 < 0 || x1 >= x2 || x2 >= x3) { return false; }
            s1 = s.Substring(x1 + s1.Length, x2 - x1 - s1.Length);
            s2 = s.Substring(x2 + s2.Length, x3 - x2 - s2.Length); int i = 0;
            if (Int32.TryParse(s1, out i) == false) { return false; } p.X = i;
            if (Int32.TryParse(s2, out i) == false) { return false; } p.Y = i;
            return true;
        } // public static bool TryParsePoint(string s, out System.Drawing.Point p)


        public void drawrectangle(PictureBox pb, Point p1, float Bwidth, Color c1)
        {
            //refresh the picture box
            pb.Refresh();
            //create a new bitmap
            Bitmap map = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            //create a graphics object
            Graphics g = Graphics.FromImage(map);
            //create a pen object and setting the color and width for the pen
            Pen p = new Pen(c1, Bwidth);
            //draw line between  point p1 and p2
            g.DrawEllipse(p, (p1.X - 50), (p1.Y - 50), 15, 15);
            pb.Image = map;
            //dispose pen and graphics object
            p.Dispose();
            g.Dispose();
        }

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
        private void textBoxUpdate(TextBox textboxAff, string mes)
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
                return (int)Invoke(d, new object[] { trackBar });
            }
            else
            {
                return trackBar.Value;
            }
        }

        #endregion
    
    }
}
