using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace drawClient
{
    public class comm
    {
        private string dataReceived;
        public bool flagDataReceived = false;

        //méthode à lancer après connexion avec un hôte. (Le sera via un Thread)
        public void loop1(object clientUDP)
        {
            ClientUDP c = (ClientUDP)clientUDP;
            while (c.EstConnecte)
            {
              
                dataReceived = c.LireDonnees(Encoding.ASCII);

                if (!dataReceived.Equals("")) flagDataReceived = true;

                Console.WriteLine("Reçu (" + dataReceived.Length + " octet(s)) : " + dataReceived);

                int e = c.EnvoyerDonnees("From drawClient", Encoding.ASCII);
                Console.WriteLine("Envoyé (" + e.ToString() + " octet(s)) : " + "From drawClient");

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
            ClientUDP c = new ClientUDP(adresseClient, int.Parse(portClient), adresseServeur, int.Parse(portServeur));
            c.NomDeLaMethodePrincipale = loop1;
            Console.WriteLine("Connexion : {0}", c.Connexion());
            if (c.EstConnecte)
            {
                c.ExecuterMethodePrincipale();
            }

            //Console.WriteLine("Press a key to deconnect client");
            //Console.Read();
            //c.Deconnexion();
            //c2.Deconnexion();

        }

        public comm()
        {

        }

        #region Getter & Setter

        public string getDataReceived()
        {
            return dataReceived;
        }

        #endregion
    }
}
