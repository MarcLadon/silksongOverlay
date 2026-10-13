# silksongOverlay

Overlay pour suivi de progression en temps réel pour Silksong.

## Raccourcis clavier

- **F3** : Aller à l'objectif précédent
- **F4** : Valider l'objectif courant et passer au suivant
- **F8** : Fermer l'application
- **Ctrl+E** : Mode édition de l'overlay (déplacer/redimensionner)
- **Ctrl+O** : Afficher/Masquer l'overlay


## Configuration des objectifs

Les objectifs du jeu sont définis dans `data/objectives.csv`. Tu peux éditer ce fichier directement pour ajouter, modifier ou supprimer des objectifs.

### Format du CSV

```csv
Order,Name,Location,DetectionType,DetectionValue,ImagePath,MapImagePath
1,Nom de l'objectif,Lieu,type_detection,valeur_detection,Icon/image.png,Map/map.png
```

**Colonnes** :

- **Order** : numéro d'ordre d'obtention (1, 2, 3, etc.)
- **Name** : nom de l'objectif
- **Location** : lieu où l'objectif se fait
- **ImagePath** : chemin optionnel vers une icône (ex: `Icon/Hunter_Crest_1.png`)
- **MapImagePath** : chemin optionnel vers une minimap (ex: `Map/deep_docks.png`)

### Exemple

```csv
Order,Name,Location,ImagePath,MapImagePath
1,Entrer dans les Quais profonds,Quais profonds,Icon/Hunter_Crest_1.png,Map/deep_docks.png
2,Vaincre le premier boss,Arène principale,Icon/Hunter_Crest_1.png,Map/arena.png
```
