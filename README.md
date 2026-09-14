# AuditFiles

Application de bureau (.NET 8 / Avalonia UI) pour auditer une arborescence de fichiers **avant une
migration vers SharePoint Online / OneDrive** : détection des chemins trop longs, caractères et
noms interdits, types de fichiers bloqués, fichiers trop volumineux, imbrication excessive,
doublons de noms ne différant que par la casse, ainsi qu'un rapport de volumétrie (nombre de
fichiers/dossiers, taille totale, répartition par extension, fichiers les plus volumineux).

L'application cible avant tout **Windows** (c'est l'usage prévu : auditer un partage de fichiers
Windows avant migration), mais s'appuie sur Avalonia UI plutôt que WPF afin de pouvoir être
compilée, testée et exécutée en mode headless sur n'importe quelle plateforme (Linux/macOS inclus),
tout en produisant un exécutable Windows natif (`AuditFiles.App.exe`) via `dotnet publish -r win-x64`.

## Structure du projet

```
AuditFiles.sln
src/
  AuditFiles.Core/     Bibliothèque .NET 8 (multiplateforme) : moteur de scan, règles d'audit,
                        exports CSV/HTML. Ne dépend d'aucun package NuGet externe.
  AuditFiles.App/      Application de bureau Avalonia UI (net8.0, multiplateforme) qui
                        s'appuie sur AuditFiles.Core.
tests/
  AuditFiles.Core.Tests/  Tests unitaires (xUnit) du moteur de scan et des règles d'audit.
```

## Règles d'audit implémentées

Les seuils par défaut (`AuditFiles.Core.Rules.SharePointLimits` et
`AuditFiles.Core.Models.ScanOptions`) sont alignés sur la documentation Microsoft officielle pour
OneDrive et SharePoint dans Microsoft 365 :

- [Restrictions and limitations in OneDrive and SharePoint](https://support.microsoft.com/en-us/office/restrictions-and-limitations-in-onedrive-and-sharepoint-64883a5d-228e-48f5-b3d2-eb39e07630fa)
- [Types of files that cannot be added to a list or library](https://support.microsoft.com/en-us/office/types-of-files-that-cannot-be-added-to-a-list-or-library-30be234d-e551-4c2a-8de8-f8546ffbf5b3)
- [SharePoint limits (Service Descriptions)](https://learn.microsoft.com/en-us/office365/servicedescriptions/sharepoint-online-service-description/sharepoint-online-limits)

Microsoft fait évoluer ces listes (par exemple, `#` et `%` étaient bloqués par défaut et sont
désormais autorisés sauf désactivation explicite par un administrateur de tenant) : **vérifiez-les
sur la documentation Microsoft 365 à jour avant de vous fier au rapport pour une décision de
migration réelle**, et ajustez `ScanOptions` / `SharePointLimits` en conséquence.

| Règle | Description |
|---|---|
| Longueur de chemin | Chemin relatif trop long compte tenu du budget réservé à l'URL du site + bibliothèque de destination (limite globale documentée : 400 caractères, chemin décodé) |
| Caractères interdits | Présence de `" * : < > ? / \ \| { }` dans un nom. `#` et `%` sont volontairement exclus : autorisés par défaut depuis une mise à jour Microsoft, sauf tenant configuré autrement |
| Espaces / point final | Nom commençant ou finissant par un espace, ou finissant par un point |
| Points consécutifs | Nom contenant `..` |
| Longueur de nom | Nom de fichier/dossier individuel trop long (255 caractères, distinct de la limite de 400 caractères sur le chemin complet) |
| Noms réservés | Noms de périphériques Windows (`CON`, `PRN`, `LPT1`...), noms système SharePoint (`_vti_`, `desktop.ini`, `.lock`), fichiers de verrouillage Office (`~$...`), dossiers commençant par `~` |
| Types de fichiers bloqués | Extensions bloquées par défaut à l'upload par SharePoint/OneDrive (`.exe`, `.dll`, `.msi`, `.vbs`...) |
| Fichiers trop volumineux | Fichier dépassant la limite de téléversement (250 Go par défaut) |
| Profondeur de dossiers | Alerte heuristique au-delà d'un seuil configurable — Microsoft ne documente pas de limite de profondeur fixe, seulement la limite globale de 400 caractères ; une imbrication profonde en est simplement une cause fréquente |
| Doublons par casse | Deux éléments d'un même dossier dont les noms ne diffèrent que par la casse (SharePoint est insensible à la casse) |

Le rapport distingue les anomalies **bloquantes** (empêcheraient la migration telle quelle) des
**avertissements** (à vérifier, mais pas nécessairement bloquants).

## Compiler et exécuter

Nécessite le [SDK .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0). Toute la solution
(bibliothèque **et** application graphique) compile et se lance sur Windows, Linux et macOS.

```bash
# Compiler toute la solution
dotnet build AuditFiles.sln

# Lancer les tests de la bibliothèque Core
dotnet test tests/AuditFiles.Core.Tests/AuditFiles.Core.Tests.csproj

# Lancer l'application graphique sur la plateforme courante
dotnet run --project src/AuditFiles.App/AuditFiles.App.csproj

# Produire un exécutable Windows natif (fonctionne même depuis Linux/macOS)
dotnet publish src/AuditFiles.App/AuditFiles.App.csproj -c Release -r win-x64 --self-contained false
```

L'application permet de :
1. sélectionner un dossier racine à auditer,
2. lancer l'analyse (asynchrone, annulable, avec suivi de progression),
3. consulter la volumétrie et la liste des anomalies détectées,
4. exporter les anomalies en CSV, un rapport technique complet en HTML, ou un **rapport PDF de
   synthèse à remettre au client** (`PdfReportWriter`, dans `AuditFiles.Core`) : volumétrie, synthèse,
   puis pour chaque catégorie d'anomalie l'action recommandée en langage clair suivie de la liste
   complète, cas par cas, de tous les éléments concernés (triés, un par ligne). La liste peut
   s'étendre sur autant de pages que nécessaire.

## Intégration continue

`.github/workflows/build.yml` compile et teste l'ensemble de la solution (bibliothèque Core et
application Avalonia) sous Linux, et publie un exécutable Windows natif (`win-x64`) sous Windows,
à chaque push/PR sur `main`.

## Limites connues

- Le scan lit les métadonnées du système de fichiers local (ou d'un partage réseau monté) ; il ne
  se connecte pas à SharePoint.
- La détection des doublons par casse compare les éléments d'un même dossier, telle que vue depuis
  le système de fichiers source ; sur un partage Windows (insensible à la casse), ce cas ne peut
  survenir que si les fichiers proviennent d'un système source sensible à la casse (NAS Linux, etc.).
- Les listes de caractères/noms/extensions interdits sont maintenues manuellement dans
  `SharePointLimits.cs` et doivent être revues périodiquement.
