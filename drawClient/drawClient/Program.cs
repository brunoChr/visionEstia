using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace drawClient
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
            
            comm _com = new comm();
            Drawing _drawing = new Drawing(_com);

            Thread threadCom = new Thread(_com.startCom);

            threadCom.Start();

            Application.Run(_drawing);
        }
    }
}
