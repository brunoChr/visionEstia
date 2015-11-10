using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace faceTracking
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            faceTrack _faceTrack = new faceTrack();

            comm _com = new comm(_faceTrack);
            Thread threadCom = new Thread(_com.startCom);

            threadCom.Start();

            Application.Run(_faceTrack);
        }
    }
}
