using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace faceTracking
{

    public class comm
    {
        private faceTrack _faceTrack = new faceTrack();

        public comm(faceTrack _faceTrack)
        {
            this._faceTrack = _faceTrack;
        }


        //méthode à lancer après connexion avec un hôte. (Le sera via un Thread)
        /*public void loop1(object clientUDP)
        {
            ClientUDP c = (ClientUDP)clientUDP;
            while (c.EstConnecte)
            {

                string r = c.LireDonnees(Encoding.ASCII);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Reçu1 (" + r.Length + " octet(s)) : " + r);
                int e = c.EnvoyerDonnees("From C1", Encoding.ASCII);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Envoyé1 (" + e.ToString() + " octet(s)) : " + "From C1");
            }
            Console.WriteLine("La connexion a été coupée !");
        }*/


        public void loop2(object clientUDP)
        {
            ClientUDP c = (ClientUDP)clientUDP;
            while (c.EstConnecte)
            {
                if (this._faceTrack.getCaptureInProgress())
                {
                    if (c.EstEnEcoute)
                    {
                        if (this._faceTrack.getCapture() != null)
                        {
                            this._faceTrack.textBoxUpdate(this._faceTrack.getTextBoxUdp(), c.AdresseDeConnexionLocale.ToString());
                        }

                        int e = c.EnvoyerDonnees(this._faceTrack.getCenterFace().ToString(), Encoding.ASCII);

                        //Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Envoyé (" + e.ToString() + " octet(s)) : " + "From faceTracking");

                        string r = c.LireDonnees(Encoding.ASCII);

                        Console.WriteLine("Reçu (" + r.Length + " octet(s)) : " + r);
                    }
                }

                Thread.Sleep(100);
            }

            Console.WriteLine("La connexion a été coupée !");
        }

        public void startCom()
        {
            Console.WriteLine("adresse serveur: ");
            string adresseServeur = "127.0.0.1";// Console.ReadLine();
            Console.WriteLine("port serveur: ");
            string portServeur = "11001";// Console.ReadLine();
            Console.WriteLine("adresse client: ");
            string adresseClient = "127.0.0.1";//Console.ReadLine();
            Console.WriteLine("port client: ");
            string portClient = "11000";//Console.ReadLine();


            /*******************/
            /*ClientUDP c = new ClientUDP(adresseClient, int.Parse(portClient), adresseServeur, int.Parse(portServeur));
            c.NomDeLaMethodePrincipale = loop1;
            Console.WriteLine("Connexion : {0}", c.Connexion());
            if (c.EstConnecte)
            {
                c.ExecuterMethodePrincipale();
            }
            */

            ClientUDP c2 = new ClientUDP(adresseServeur, int.Parse(portServeur), adresseClient, int.Parse(portClient));
            c2.NomDeLaMethodePrincipale = loop2;
            Console.WriteLine("Connexion : {0}", c2.Connexion());
            if (c2.EstConnecte)
            {
                c2.ExecuterMethodePrincipale();
            }
            /******************/

            //Console.WriteLine("Press a key to deconnect client");
            //Console.Read();
            //c.Deconnexion();
            //c2.Deconnexion();

        }
    }
}
