#### _Read this in English._
<kbd>[<img title="English" alt="English" src="https://flagcdn.com/w40/gb.png" width="22">](translations/Software Specification_Project_EasySafe_1.0_English.md)</kbd>

### **Cahier des Charges - EasySave Version 1.0**

---

#### **1. Objectif du Logiciel**
EasySave 1.0 est une application console développée en .NET Core. Elle permet la création et l’exécution de travaux de sauvegarde, complets ou différentiels, avec une interface en ligne de commande.  
Le logiciel est conçu pour des environnements professionnels avec :
- Compatibilité multi-utilisateur (français et anglais).
- Suivi en temps réel des opérations.
- Traçabilité complète via des fichiers journaux.

---

#### **2. Fonctionnalités Principales**

##### **2.1 Gestion des Travaux de Sauvegarde**
- Jusqu'à **5 travaux de sauvegarde** configurables.
- Chaque travail comprend :
  - **Nom**
  - **Répertoire source**
  - **Répertoire cible**
  - **Type de sauvegarde** : complète ou différentielle.
- Prise en charge des répertoires :
  - Disques locaux.
  - Disques externes.
  - Lecteurs réseaux (chemins UNC).
- Tous les fichiers et sous-dossiers du répertoire source sont inclus.

##### **2.2 Exécution des Travaux**
- Exécution à la demande d’un ou plusieurs travaux.
- Exécution **séquentielle** de tous les travaux.
- Lignes de commande possibles :
  - `1-3` → Exécute les sauvegardes 1 à 3.
  - `1;3` → Exécute les sauvegardes 1 et 3.

---

#### **3. Journalisation et Suivi en Temps Réel**

##### **3.1 Fichier Log Journalier**
- Généré en **temps réel**, au format JSON.
- Informations incluses :
  - Horodatage
  - Nom de la sauvegarde
  - Chemin complet source (UNC)
  - Chemin complet destination (UNC)
  - Taille du fichier
  - Durée de transfert (en millisecondes, négatif si erreur)
- Lisible avec **Notepad** (retours à la ligne entre les éléments JSON).
- Développé sous forme de **DLL réutilisable**, compatible avec la version 1.0.

##### **3.2 Fichier d'État en Temps Réel**
- Fichier unique : `state.json`.
- Contient pour chaque travail :
  - Nom du travail
  - Horodatage de la dernière action
  - État (Actif / Inactif)
  - Si actif :
    - Nombre total de fichiers
    - Taille totale
    - Progression :
      - Fichiers restants
      - Taille restante
    - Fichier en cours (source & destination au format UNC)

---

#### **4. Contraintes Techniques**

##### **4.1 Emplacement des Fichiers**
- Les dossiers temporaires comme `C:\temp\` sont **interdits**.
- Tous les fichiers doivent être accessibles depuis un **serveur**.

##### **4.2 Format des Fichiers**
- **JSON obligatoire** pour logs, état et configurations.
- **Retours à la ligne** pour la lisibilité.
- **Pagination JSON** souhaitée.

##### **4.3 Internationalisation**
- Interface et logs disponibles en **français et anglais**.

##### **4.4 Modularité**
- Fonctionnalité de log sous forme de **DLL**.
- La DLL doit rester **compatible avec la version 1.0** même après des évolutions.

---

#### **5. Évolutions Futures**
- Une **version 2.0** avec **interface graphique** (architecture MVVM) sera développée si la version 1.0 est satisfaisante.

---

#### **6. Conclusion**
EasySave 1.0 ambitionne de fournir une **solution de sauvegarde simple, flexible et évolutive**.  
Grâce à son suivi en temps réel, sa compatibilité multi-environnements et sa journalisation détaillée, il constitue une **base robuste pour les futures versions**.
