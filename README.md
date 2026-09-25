# ChatBureau

Un compagnon de bureau pour Windows, personnalisable et compatible avec vos images PNG.

Interface inspirée d’iOS : fenêtre arrondie, navigation verticale à gauche, aperçu pastel et curseurs à reflets clairs. Le curseur **Transparence** de la barre latérale règle la transparence réelle de toute la fenêtre (texte compris), de 0 à 16 %. Cliquez sur **Appliquer au chat** pour conserver ce réglage. Il ne s'agit pas d'un flou d'arrière-plan. Le fond reste opaque en contraste élevé et en session distante.

## Utilisation

Téléchargez `ChatBureau.exe` depuis **Releases**, placez-le dans un dossier où vous pouvez écrire, puis lancez-le. Windows 10/11 et .NET Framework 4.x sont nécessaires. Le binaire n'est pas signé numériquement.

- Atelier avec aperçu animé : pelages, couleurs, motifs, chapeaux, accessoires et expressions.
- Import PNG transparent, proportions conservées, image copiée localement à l'enregistrement.
- Promenade, sieste, saut, étirement, toilette, salut, danse, pirouette, rebonds, secousse et bâillement.
- Clic pour caresser, glisser pour porter, clic droit pour accéder aux options.
- Catégorie **Mises à jour** dans l’atelier → **Rechercher** → **Installer et redémarrer**, sans ouvrir le navigateur. Le clic droit → **Mises à jour** ouvre directement cette catégorie.

Un PNG s'anime comme une image entière, sans animation indépendante des membres. Limites d'import : 16 Mo et 4 096 × 4 096 pixels.

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
