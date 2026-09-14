# AuditFiles

Application Windows (.NET 8 / WPF) pour auditer une arborescence de fichiers **avant une migration
vers SharePoint Online / OneDrive** : détection des chemins trop longs, caractères et noms interdits,
types de fichiers bloqués, fichiers trop volumineux, imbrication excessive, doublons de noms ne
différant que par la casse, ainsi qu'un rapport de volumétrie (nombre de fichiers/dossiers, taille
totale, répartition par extension, fichiers les plus volumineux).

## Structure du projet

```
AuditFiles.sln
src/
  AuditFiles.Core/     Bibliothèque .NET 8 (multiplateforme) : moteur de scan, règles d'audit,
                        exports CSV/HTML. Ne dépend d'aucun package NuGet externe.
  AuditFiles.App/      Application WPF (net8.0-windows) : interface graphique Windows qui
                        s'appuie sur AuditFiles.Core.
tests/
  AuditFiles.Core.Tests/  Tests unitaires (xUnit) du moteur de scan et des règles d'audit.
```

## Règles d'audit implémentées

Les seuils par défaut (`AuditFiles.Core.Rules.SharePointLimits` et
`AuditFiles.Core.Models.ScanOptions`) reflètent les restrictions documentées par Microsoft pour
SharePoint Online / OneDrive au moment de l'écriture de ce projet. Microsoft fait évoluer ces
limites : **vérifiez-les sur la documentation Microsoft 365 à jour avant de vous fier au rapport
pour une décision de migration réelle**, et ajustez `ScanOptions` en conséquence.

| Règle | Description |
|---|---|
| Longueur de chemin | Chemin relatif trop long compte tenu du budget réservé à l'URL du site + bibliothèque de destination (limite globale par défaut : 400 caractères) |
| Caractères interdits | Présence de `" * : < > ? / \ \| # %` dans un nom |
| Espaces / point final | Nom commençant ou finissant par un espace, ou finissant par un point |
| Longueur de nom | Nom de fichier/dossier trop long (400 caractères par défaut) |
| Noms réservés | Noms de périphériques Windows (`CON`, `PRN`, `LPT1`...), noms système SharePoint (`_vti_`, `forms`, `desktop.ini`), fichiers de verrouillage Office (`~$...`) |
| Types de fichiers bloqués | Extensions bloquées à l'upload par SharePoint (`.exe`, `.dll`, `.ps1`...) |
| Fichiers trop volumineux | Fichier dépassant la limite de téléversement (250 Go par défaut) |
| Profondeur de dossiers | Alerte heuristique au-delà d'un seuil configurable (cause fréquente de dépassement de longueur de chemin) |
| Doublons par casse | Deux éléments d'un même dossier dont les noms ne diffèrent que par la casse (SharePoint est insensible à la casse) |

Le rapport distingue les anomalies **bloquantes** (empêcheraient la migration telle quelle) des
**avertissements** (à vérifier, mais pas nécessairement bloquants).

## Compiler et exécuter

Nécessite le [SDK .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
# Restaurer et lancer les tests de la bibliothèque Core (multiplateforme)
dotnet test tests/AuditFiles.Core.Tests/AuditFiles.Core.Tests.csproj

# Compiler et lancer l'application Windows (nécessite Windows)
dotnet run --project src/AuditFiles.App/AuditFiles.App.csproj
```

L'application permet de :
1. sélectionner un dossier racine à auditer,
2. lancer l'analyse (asynchrone, annulable, avec suivi de progression),
3. consulter la volumétrie et la liste des anomalies détectées,
4. exporter les anomalies en CSV ou un rapport complet en HTML.

## Intégration continue

`.github/workflows/build.yml` compile et teste `AuditFiles.Core` sous Linux, et compile
l'ensemble de la solution (y compris l'application WPF) sous Windows, à chaque push/PR sur `main`.

## Limites connues

- Le scan lit les métadonnées du système de fichiers local (ou d'un partage réseau monté) ; il ne
  se connecte pas à SharePoint.
- La détection des doublons par casse compare les éléments d'un même dossier, telle que vue depuis
  le système de fichiers source ; sur un partage Windows (insensible à la casse), ce cas ne peut
  survenir que si les fichiers proviennent d'un système source sensible à la casse (NAS Linux, etc.).
- Les listes de caractères/noms/extensions interdits sont maintenues manuellement dans
  `SharePointLimits.cs` et doivent être revues périodiquement.
