Assignement Developpement d'application Rapide


/*** PROJET DE VISION PAR ORDINATEUR AVEC EMGUCV C# ***/

	/** FONCTIONNE EN 64BITS **/

		/* SI EN 32 BITS CHANGER LA DLL CVEXT DANS LE REPERTOIR EXE */


2 projet c# : 

le 1er effectue : - le tracking de la t�te et des yeux utilisant emguCv
	 	  - Calcul les coordonn�es de la t�te d�tect�
		  - Se connecte � l'autre client et lui envoie ces coordonn�es par protocol UDP


le 2eme effectue : - R�cup�re les coordonn�es envoy� par UDP et les convertits en points
		   - Trace une ellipse sur une pictureBox aux coordonn�es recus



!! ATTENTION !!! ON DOIT D'ABORD LANC� LE PROJET DRAWCLIENT AVANT D'�X�CUTER LE PROJET FACETRACKING	


Les diagrammes de classes se situent dans la solution visual