Assignement Developpement d'application Rapide


/*** PROJET DE VISION PAR ORDINATEUR AVEC EMGUCV C# ***/

	/** FONCTIONNE EN 64BITS **/

		/* SI EN 32 BITS CHANGER LA DLL CVEXT DANS LE REPERTOIR EXE */


2 projet c# : 

le 1er effectue : - le tracking de la tête et des yeux utilisant emguCv
	 	  - Calcul les coordonnées de la tête détecté
		  - Se connecte à l'autre client et lui envoie ces coordonnées par protocol UDP


le 2eme effectue : - Récupére les coordonnées envoyé par UDP et les convertits en points
		   - Trace une ellipse sur une pictureBox aux coordonnées recus



!! ATTENTION !!! ON DOIT D'ABORD LANCÉ LE PROJET DRAWCLIENT AVANT D'ÉXÉCUTER LE PROJET FACETRACKING	


Les diagrammes de classes se situent dans la solution visual