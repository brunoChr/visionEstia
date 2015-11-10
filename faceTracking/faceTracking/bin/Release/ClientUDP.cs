/* Classe ClientUDP :
 *  
 *  Description todo
 * 
 * Auteur  : j.canou (j.canou@estia.fr)
 * Date    : 08/07/2015
 * Version : 0.5 alpha
 * 
 * Copyright © 2015
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Globalization;



/// <summary>
/// 
/// </summary>
public class ClientUDP
{
    #region -------------------------------------Définition des Attributs-------------------------------------

    private Thread _thread_ecoute = null;

    private UdpClient _client = null;

    private Thread _thread = null;

    /// <summary>
    /// Signature d'une méthode pouvant être exécutée après une connexion à un serveur.
    /// </summary>
    public delegate void MainMethode(object clientTCPIP);

    private MainMethode _methodePrincipale = null;
    /// <summary>
    /// Obtient ou définit une référence vers une méthode de type MainMethode, c'est à dire ayant pour signature : void nomMethode(object clientTCPIP).
    /// </summary>
    public MainMethode NomDeLaMethodePrincipale
    {
        get { return _methodePrincipale; }
        set { _methodePrincipale = new MainMethode(value); }
    }

    private string _nomHote = null;
    /// <summary>
    /// Obtient ou définit l'adresse IPv4 de l'hôte auquel se connecter.
    /// </summary>
    public string NomHote
    {
        get { return _nomHote; }
        set { _nomHote = value; }
    }

    private int _port = -1;
    /// <summary>
    /// Obtient ou définit le numéro de port sur lequel la connexion doit être établie.
    /// </summary>
    public int Port
    {
        get { return _port; }
        set { _port = value; }
    }

    private IPEndPoint _adresseDeConnexionLocale = null;
    /// <summary>
    /// Obtient l'adresse et le port de la connexion locale.
    /// </summary>
    public string AdresseDeConnexionLocale
    {
        get { return _adresseDeConnexionLocale.ToString(); }
    }


    private Object thisLock = new Object();

    IPEndPoint ClientIp;
    IPEndPoint localEndPoint;
    IPEndPoint remoteIpEndPoint;
    byte[] received;

    string currentString;

    private Boolean _enEcoute = false;
    /// <summary>
    /// Obtient l'état du serveur.
    /// </summary>
    public Boolean EstEnEcoute { get { return _enEcoute; } }

    private IPAddress _adresseEcoute = IPAddress.Parse("127.0.0.1");
    private IPAddress _adresseEnvoi = IPAddress.Parse("127.0.0.1");
    /// <summary>
    /// Obtient ou définit l'adresse IPv4 locale sur laquelle le serveur doit attendre les connexions.
    /// </summary>
    public string AdresseIP_Ecoute
    {
        get { return _adresseEcoute.ToString(); }
        set { _adresseEcoute = IPAddress.Parse(value); }
    }
    public string AdresseIP_Envoi
    {
        get { return _adresseEnvoi.ToString(); }
        set { _adresseEnvoi = IPAddress.Parse(value); }
    }

    private int _portEcoute = 11001;
    private int _portEnvoi = 11000;
    /// <summary>
    /// Obtient ou définit le port sur lequel le serveur doit écouter.
    /// </summary>
    public int PortEcoute
    {
        get { return _portEcoute; }
        set { _portEcoute = value; }
    }
    public int PortEnvoi
    {
        get { return _portEnvoi; }
        set { _portEnvoi = value; }
    }
    // Pour lister les clients connectés au serveur.
    protected SortedDictionary<string, ClientUDP> _listeDesClients = new SortedDictionary<string, ClientUDP>();
    /// <summary>
    /// Obtient la liste des clients actuellement connectés au serveur.
    /// </summary>
    public SortedDictionary<string, ClientUDP> ListeDesClients
    {
        get { return _listeDesClients; }
    }

    /// <summary>
    /// Obtient le nombre de clients actuellement connectés au serveur.
    /// </summary>
    public int NbrDeClientsConnectes { get { return _listeDesClients.Count; } }


    private bool _isIdClientSet = false;
    private string _idClient = DateTime.Now.Ticks.ToString();
    /// <summary>
    /// Obtient l'identifiant du client. L'identifiant par défaut est le nombre de "Ticks" depuis le 01/01/1900.
    /// Il est possible de changer l'identifiant, mais une fois seulement.
    /// </summary>
    public string IdClient
    {
        get { return _idClient; }
        set
        {
            if (!_isIdClientSet)
            {
                _idClient = value;
                _isIdClientSet = true;
            }
        }
    }


    /// <summary>
    /// Obtient l'état de la connexion.
    /// </summary>
    public Boolean EstConnecte
    {
        get { if (_client != null) return _enEcoute; else return false; }
    }

    #endregion -------------------------------Fin de Définition des Attributs---------------------------------


    #region ------------------------------------Définition des Constructeurs----------------------------------

    /// <summary>
    /// Initialise une nouvelle instance par défaut de la classe ClientUDP sur l'adresse localhost et sur le port 11001.
    /// </summary>
    public ClientUDP()
    {
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11001);
        IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);

        this._client = new UdpClient(localEndPoint);

    }



    /// <summary>
    /// Initialise une nouvelle instance de la classe ClientUDP avec les attributs passés en argument.
    /// </summary>
    /// <param name="adresseEcoute">Adresse IPv4 locale d'écoute.</param>
    /// <param name="portEcoute">Port d'écoute.</param>
    public ClientUDP(string adresseEcoute, int portEcoute, string adresseEnvoi, int portEnvoi)
    {
        AdresseIP_Ecoute = adresseEcoute;
        PortEcoute = portEcoute;
        AdresseIP_Envoi = adresseEnvoi;
        PortEnvoi = portEnvoi;

        localEndPoint = new IPEndPoint(IPAddress.Parse(AdresseIP_Ecoute), portEcoute);
        remoteIpEndPoint = new IPEndPoint(IPAddress.Parse(AdresseIP_Envoi), PortEnvoi);

        this._client = new UdpClient(localEndPoint);



    }

    #endregion ------------------------------Fin de Définition des Constructeurs------------------------------


    #region ---------------------------------------Définition des méthodes------------------------------------

    /// <summary>
    /// Démarre l'écoute de demandes de connexion entrante.
    /// </summary>
    /// <returns>True si l'écoute est démarrée, False sinon.</returns>
    public Boolean StartEcoute()
    {
        if (_thread == null)
        {



            try
            {
                //on démarre un thread qui servira à écouter les demandes de connexion (via la méthode BoucleEcoute).               
                _thread_ecoute = new Thread(new ThreadStart(BoucleEcoute));
                _thread_ecoute.Priority = ThreadPriority.Highest;
                _thread_ecoute.IsBackground = true;

                _thread_ecoute.Start();
                _enEcoute = true;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Problème lors du démarrage de l'écoute du serveur : " + e.Message);
            }
            return false;
        }
        else return true;
    }

    //    public abstract void ConnecterQuiAQuoi(IPEndPoint ClientIpFrom, byte[] trame);

    /// <summary>
    /// Permet de repondre par une trame traitée. A reprendre pour particulariser ClientIpFrom selon que l'on est robot ou other
    /// </summary>
    public int EnvoyerDonnees(String trame, Encoding encodage)
    {
        return _client.Send(Encoding.ASCII.GetBytes(trame), trame.Length);//, remoteIpEndPoint);
       // return 0;
    }

    /// <summary>
    /// Lit et retourne les données reçues.
    /// </summary>
    /// <param name="encodage">Type d'encodage des données. Ex : Encoding.ASCII</param>
    /// <returns>Les données reçues au format spécifié.</returns>
    /// <exception cref="System.IOException">Lancée lorsqu'un problème de connexion intervient.</exception>
    /// <exception cref="System.Exception">Lancée lorsqu'un problème c'est déroulé durant la lecture des données.</exception>
    public string LireDonnees(Encoding encodage)
    {
        if (this.EstEnEcoute)
        {
            received = this._client.Receive(ref remoteIpEndPoint);
            currentString = Encoding.ASCII.GetString(received);
            return currentString;

        }
        else
            return null;
    }

    /// <summary>
    /// Permet de démarrer le serveur en écoute et d'attendre des demandes de connexion.
    /// </summary>
    private void BoucleEcoute()
    {
        while (_enEcoute)
        {
            try
            {
                if (this._client.Available > 0)
                {
                    lock (thisLock)
                    {
                        received = this._client.Receive(ref remoteIpEndPoint);
                        //  ClientIp = new IPEndPoint(remoteIpEndPoint.Address, remoteIpEndPoint.Port);
                        //ConnecterQuiAQuoi(remoteIpEndPoint, received);
                    }
                }

            }
            catch (Exception ex)
            {
                //handle Exception
            }

        }
    }

    /// <summary>
    /// Ferme l'écoute de demandes de connexion entrante.
    /// Ne ferme aucune des connexions acceptées. Utilisez StopClients pour ce faire.
    /// </summary>
    /// <returns>True si l'écoute est fermée, False sinon.</returns>
    public Boolean StopEcoute()
    {
        try
        {
            _enEcoute = false;
            _client.Close();
            _thread_ecoute.Join(); //on attend que le thread se termine.

            _thread_ecoute = null;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Problème lors de la fermeture de l'écoute du client : " + e.Message);
        }
        return false;
    }


    /// <summary>
    /// Lance un Thread exécutant la méthode (déléguée) principale si la connexion à l'hôte est effective.
    /// La méthode déléguée (de type MainMethode) doit avoir la signature suivante : void nomMethode(object clientTCPIP).
    /// </summary>
    /// <returns>True si la méthode est lancée, False sinon.</returns>
    public Boolean ExecuterMethodePrincipale()
    {
        if (_methodePrincipale != null && _enEcoute && _thread == null)
        {
            //on démarre un thread qui servira à exécuter la méthode _methodePrincipale.
            _thread = new Thread(new ParameterizedThreadStart(_methodePrincipale));
            _thread.Priority = ThreadPriority.Highest;
            _thread.IsBackground = true;
            _thread.Start(this);
            Console.WriteLine("(" + IdClient + ") commence son travail ! ");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Stoppe l'exécution de la méthode (déléguée) principale.
    /// </summary>
    /// <returns>True si la méthode est stoppée, False sinon.</returns>
    public Boolean StopMethodePrincipale()
    {
        if (_thread != null || _thread.IsAlive)
        {
            try
            {
                _thread.Abort();
                _thread = null;
                Console.WriteLine("(" + IdClient + ") termine son travail !");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Problème abort :" + e.Message);
            }
        }
        else
        {
            Console.WriteLine("(" + IdClient + ") n'était pas en train de travailler !");
        }
        return false;
    }


    /// <summary>
    /// Etablit une connexion (NomHote et Port préalablement spécifiés)
    /// </summary>
    /// <returns>True si la connexion est effectuée, False sinon.</returns>
    public Boolean Connexion()
    {

        //si on tente de se connecter alors que c'est déjà le cas.
        if (EstConnecte) return true;

        NomHote = AdresseIP_Envoi;
        Port = PortEnvoi;


        //si l'adresse est "valide".
        if (NomHote != null && Port != -1)
        {
            try
            {
                ////connexion à l'hôte.
                //if (_client == null) _client = new UdpClient(NomHote, Port); //cas d'une reconnexion.
                //else 
                _client.Connect(NomHote, Port); //cas d'une première connexion.

                _enEcoute = true;

                _adresseDeConnexionLocale = (IPEndPoint)_client.Client.LocalEndPoint;
                Console.WriteLine("Connexion établie avec {0}:{1} depuis {2}", NomHote, Port, _adresseDeConnexionLocale);
                return true;


            }
            catch (Exception e)
            {
                Console.WriteLine("Problème à la connexion :" + e.Message);
                return false;
            }
        }
        else return false;
    }

    /// <summary>
    /// Stoppe la connexion avec l'hôte et arrête l'exécution de la méthode principale.
    /// </summary>
    /// <returns>True si la déconnexion est effectuée, False sinon.</returns>
    public Boolean Deconnexion()
    {
        StopMethodePrincipale();

        return true;// StopEcoute();

    }



    ///// <summary>
    ///// Déconnecte tous les clients, puis vide la liste des clients.
    ///// </summary>
    ///// <returns>True si tous les clients sont déconnectés, False sinon.</returns>
    //public Boolean StopClients()
    //{
    //    try
    //    {
    //        foreach (ClientUDP c in _listeDesClients.Values) c.Deconnexion();
    //        _listeDesClients.Clear();
    //        return true;
    //    }
    //    catch (Exception e)
    //    {
    //        Console.WriteLine("Problème à la fermeture des clients : " + e.Message);
    //    }
    //    return false;
    //}

    /////// <summary>
    /////// Déconnecte le client identifié par son adresse IP et son port, puis le supprime de la liste des clients.
    /////// </summary>
    /////// <param name="IP_Port">IP et Port du client. Ex : "127.0.0.1:12345"</param>
    /////// <returns>True si le client est stoppé, False sinon.</returns>
    //public Boolean StopUnClient(string IP_Port)
    //{
    //    if (_listeDesClients.ContainsKey(IP_Port))
    //    {
    //        if (_listeDesClients[IP_Port].Deconnexion()) Console.WriteLine("Le client a été déconnecté");
    //        _listeDesClients.Remove(IP_Port);
    //        return true;
    //    }
    //    return false;
    //}

    #endregion --------------------------------Fin de Définition des méthodes---------------------------------
}
