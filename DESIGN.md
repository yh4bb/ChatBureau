# Atelier ChatBureau

Mode de la surface : Operate. Référence approuvée : verre clair et commandes arrondies d'iOS, adaptés à Windows 10/11. Les six catégories forment une colonne à gauche, au-dessus du compagnon animé. L'en-tête à droite nomme la catégorie active. Conserver le bleu d'action et l'aperçu pastel.

Typographie système Segoe UI : titres 22 pt, sections 11 pt semi-gras, champs 10–11 pt, aides 9 pt. Fond neutre très clair, surfaces blanches, texte principal sombre et texte secondaire suffisamment contrasté. Rayons de 12–16 px pour les panneaux, 9–10 px pour les contrôles. Accent bleu réservé à la sélection et à l'action principale. Couleurs du chat affichées sous forme d'échantillons avec noms lisibles.

Regrouper les réglages apparentés plutôt que multiplier les cartes. Le nom reste un vrai champ identifiable. Une barre d'état distingue les changements en cours des réglages appliqués. Fenêtre arrondie à 26 px, boutons à 14 px, curseurs en capsule avec reflet et point central bleu. La transparence réglable utilise l'opacité native de la fenêtre entière : 7 % par défaut, 0–16 %, sans flou d'arrière-plan. Fond opaque en contraste élevé et session distante. Ne pas la présenter comme de l'acrylique natif.

Transitions de 160 ms pour la sélection, le survol et les interrupteurs. Les animations de l'interface respectent SPI_GETCLIENTAREAANIMATION ; chaque minuteur s'arrête au repos et est libéré avec le contrôle. Le chat conserve ses animations existantes. La barre de titre personnalisée offre réduction, fermeture, déplacement et redimensionnement par les bords ; Alt+F4 reste natif.

Les listes déroulantes utilisent le comportement clavier et la gestion des menus de Windows. Leurs ressources persistent jusqu'à la destruction du contrôle, jamais pendant l'événement de fermeture du menu. Vérifier sélection, annulation, réouverture, navigation clavier, taille réduite de la fenêtre et rendu à différents facteurs d'échelle.
