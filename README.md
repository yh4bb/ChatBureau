# ChatBureau

Un compagnon de bureau pour Windows, personnalisable et compatible avec vos images PNG.

Interface inspirée d’iOS : fenêtre arrondie, navigation verticale à gauche, aperçu pastel et curseurs à reflets clairs. Le curseur **Transparence** de la barre latérale règle la transparence réelle de toute la fenêtre (texte compris), de 0 à 16 %. Cliquez sur **Appliquer au chat** pour conserver ce réglage. Il ne s'agit pas d'un flou d'arrière-plan. Le fond reste opaque en contraste élevé et en session distante.

## Utilisation

Téléchargez `ChatBureau.exe` depuis **Releases**, placez-le dans un dossier où vous pouvez écrire, puis lancez-le. Windows 10/11 et .NET Framework 4.x sont nécessaires. Le binaire n'est pas signé numériquement.

- Atelier avec aperçu animé : pelages, couleurs, motifs, chapeaux, accessoires et expressions.
- **Apparence** : 12 couleurs prédéfinies, 10 motifs et couleurs libres. **Style** : six styles complets, choix aléatoire et annulation du dernier style ; sept accessoires de tête, lunettes, six expressions et couleurs du nez, des oreilles et du deuxième œil. Les styles complets activent le chat dessiné et conservent votre PNG dans sa catégorie.
- Import PNG transparent, proportions conservées, image copiée localement à l'enregistrement.
- Promenade, sieste, saut, étirement, toilette, salut, danse, pirouette, rebonds, secousse et bâillement.
- Clic pour caresser, glisser pour porter, clic droit pour accéder aux options.
- Catégorie **Mises à jour** dans l’atelier → **Rechercher** → **Installer et redémarrer**, sans ouvrir le navigateur. Le clic droit → **Mises à jour** ouvre directement cette catégorie.

Un PNG seul s'anime comme une image entière. Limites : 16 Mo et 4 096 × 4 096 pixels. Pour animer sa silhouette image par image, importez une séquence dans **Image PNG → Un personnage qui s’anime**. Sélectionnez 2 à 24 PNG de dimensions identiques, nommés `01.png`, `02.png`, etc. L’ordre est alphabétique. Maximum : 1 024 × 1 024 pixels par image et 16 millions de pixels par séquence. Deux séquences peuvent être définies : marche (animation habituelle) et repos (sieste). La cadence est réglable de 1 à 24 images/seconde. Les copies locales survivent au déplacement des fichiers d’origine.

## Personnalité et présence

Dans **Habitudes**, choisissez Calme (pauses plus longues, siestes), Joueur (sauts, danse), Pot de colle (salutations, proximité) ou Farceur (pirouettes, gestes malicieux). Les animations décochées ne sont pas utilisées pour les gestes spontanés. Le suivi doux de la souris concerne Pot de colle et Farceur ; il peut être désactivé.

Le chat peut grimper sur les fenêtres suffisamment éloignées du haut de l’écran, sauter entre des rebords proches, puis revenir. Il quitte son trajet si le support se déplace ou se ferme. Réglez la vitesse à zéro ou désactivez **Grimper et sauter entre les fenêtres** pour limiter ses déplacements.

**Dormir en mon absence** déclenche une sieste après deux minutes sans activité et un salut au retour. **Mode discret** suspend déplacements et farces lorsqu’une application recouvre son écran, y compris les jeux en plein écran ou sans bordure. Ces deux réglages sont activés par défaut. La détection se fonde sur la géométrie des fenêtres Windows.

## Garde-robe

Composez votre look, puis ouvrez **Garde-robe**, donnez-lui un nom et cliquez sur **Enregistrer ce look**. Cliquer sur une vignette charge le style dans l’aperçu ; **Appliquer au chat** le rend actif. Le nom du compagnon, sa taille et ses habitudes ne changent pas.

**Exporter le look actuel** produit un fichier `.chatlook` contenant l’apparence et, si utilisé, le personnage PNG avec ses séquences. **Importer un style** ajoute ce fichier à la galerie. Aucun chemin local ni nom du compagnon n’est partagé. Les fichiers exportés sont limités à 48 Mo, dont au maximum 32 Mo de PNG.

## Chat farceur

Ouvrez **Farces** dans la colonne de gauche ou dans le menu du chat. Le mode **Visuel** fait venir le chat près du pointeur et sur les bords des fenêtres. Le mode **Interactif** donne un petit coup de défilement vers le bas ou le haut dans une fenêtre que vous choisissez dans la liste. Cliquez sur **Actualiser les fenêtres** si nécessaire. Un indicateur vérifie la présence d’une zone de défilement. Cette détection ne garantit pas que le contrôle actuellement actif autorisera la farce : une fenêtre incompatible est ignorée.

Réglez l'intervalle (15 à 180 secondes), cliquez sur **Activer pour cette session**, puis fermez l'atelier. **Essayer maintenant** prépare une farce et réduit l’atelier, en conservant vos modifications non enregistrées. L’état affiche le délai restant ou la raison de la pause. La fenêtre choisie doit rester au premier plan. Le chat attend une pause dans vos frappes. Les champs de saisie, mots de passe et menus sont exclus ; aucune touche ni aucun clic ne sont injectés. Les titres de fenêtres servent uniquement à la sélection locale, sans enregistrement ni envoi.

Après une farce, le chat revient progressivement à sa position de départ. **Échap** arrête les farces immédiatement, sur place. Vous pouvez aussi utiliser **Arrêter les farces** dans l'atelier ou le menu du chat. Déplacer le chat manuellement les arrête également. Chaque lancement repart avec les farces désactivées. L'application ne nécessite pas les droits administrateur et ne tente pas de contourner les restrictions Windows.

## Mises à jour

L'application consulte les versions stables du dépôt GitHub public compilé dans l'exécutable. Elle propose la vérification quotidienne au lancement (désactivable dans la fenêtre des mises à jour) et n'installe rien sans clic sur **Installer**. Le téléchargement passe par HTTPS et son empreinte SHA-256 est contrôlée contre celle publiée par GitHub. Une sauvegarde `.previous` est conservée à côté de l'exécutable remplacé.

Les réglages et PNG restent dans `%LOCALAPPDATA%\ChatBureau`. Aucun jeton GitHub n'est distribué dans l'application. Les mises à jour intégrées nécessitent des Releases publiques ; un dépôt privé reste compilable, mais ses versions ne sont pas accessibles anonymement.

L'application contacte GitHub uniquement pour les mises à jour ; elle ne transmet ni images ni préférences. GitHub reçoit les informations réseau habituelles d'une requête, notamment l'adresse IP. Pas de télémétrie.

## Développement

```powershell
./build.ps1 -Test
./build.ps1 -Repository votre-compte/ChatBureau -Version 1.0.0 -Test
```

Le compilateur .NET Framework fourni avec Windows est utilisé. Aucune dépendance NuGet. Les résultats sont dans `dist/`, les tests et aperçus dans `build/tests/`. Sans `-Repository`, les mises à jour sont désactivées pour le binaire local.

## Publier une version

1. Modifiez et testez le code, puis actualisez `VERSION` et `CHANGELOG.md`.
2. Poussez vos commits sur `main`.
3. Créez et poussez un tag comme `v1.0.1`.

```powershell
git tag v1.0.1
git push origin v1.0.1
```

GitHub Actions compile, exécute les tests et publie l'exécutable, son empreinte et le ZIP dans une Release. La compilation encode automatiquement le dépôt et la version. Les tests vérifient les préférences, les PNG, l'interface, les animations et la logique de mise à jour sans modifier le profil utilisateur.

Application indépendante inspirée du principe des animaux de bureau et du style de WorkCat, sans affiliation. Elle ne ferme aucune fenêtre ou vidéo.
