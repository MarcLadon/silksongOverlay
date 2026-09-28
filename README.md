# silksongOverlay

Overlay pour suivi de progression et timer pour Silksong

## Installation et lancement

### Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download) ou supérieur
- Windows (l'application utilise des API spécifiques Windows)

### Cloner le dépôt

```powershell
git clone https://github.com/MarcLadon/silksongOverlay.git
cd silksongOverlay
```

### Build

```powershell
dotnet build
```

### Lancer l'application

```powershell
dotnet run
```

## Configuration des objectifs

Les objectifs du jeu sont définis dans `data/objectives.csv`. Tu peux éditer ce fichier directement pour ajouter, modifier ou supprimer des objectifs.

### Format du CSV

```csv
Order,Name,Location,DetectionType,DetectionValue,ImagePath
1,Nom de l'objectif,Lieu,type_detection,valeur_detection,Icon/image.png
```

**Colonnes** :

- **Order** : numéro d'ordre d'obtention (1, 2, 3, etc.)
- **Name** : nom de l'objectif
- **Location** : lieu où l'objectif se fait
- **DetectionType** : type d'événement (`area_entered`, `boss_defeated`, etc.)
- **DetectionValue** : valeur précise de l'événement (ex: `Deep Docks`, `Bell Beast`)
- **ImagePath** : chemin optionnel vers une icône (ex: `Icon/Hunter_Crest_1.png`)

### Exemple

```csv
Order,Name,Location,DetectionType,DetectionValue,ImagePath
1,Entrer dans les Quais profonds,Quais profonds,area_entered,Deep Docks,Icon/Hunter_Crest_1.png
2,Vaincre le premier boss,Arène principale,boss_defeated,Bell Beast,Icon/Hunter_Crest_1.png
```

## Détection automatique (test)

L'application attend des événements depuis `runtime/game-events.json`. Pour tester, tu peux modifier ce fichier :

```json
{
  "type": "area_entered",
  "value": "Deep Docks"
}
```

Si cet événement correspond à l'objectif courant, il sera validé automatiquement.

## Raccourcis clavier

- **F1** : Basculer le timer (play/pause)
- **F2** : Réinitialiser le timer
- **F3** : Ajouter une mort
- **F4** : Valider l'objectif courant
- **Shift+F4** : Aller à l'objectif précédent
- **Ctrl+E** : Mode édition de l'overlay
- **Ctrl+O** : Afficher/Masquer l'overlay
