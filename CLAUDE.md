# Space Escaper

3-Lane Endless Runner für Android, Unity **6000.6.0f1**, Built-in Render Pipeline.
Ursprünglich 2020 in Unity 2019.4 gebaut und im Play Store veröffentlicht — aktuell
ein Legacy-Projekt, das schrittweise modernisiert wird. Hobbyprojekt, kein Zeitdruck,
Arbeit passiert in unregelmäßigen Sessions.

## Aufbau

Repo-Root ≠ Unity-Projekt: das Unity-Projekt liegt in `Space-Escaper/`.

- `Space-Escaper/Assets/Scripts/` — 12 Skripte, ~1300 Zeilen. Einstiegspunkte:
  `GameManager.cs` (Menü, Shop, Score, Death — macht sehr viel), `PlayerMotor.cs`,
  `AudioSystem.cs`, `MobileInput.cs`, `TileManager.cs` / `FieldManager.cs` (Spawning).
- `Space-Escaper/Assets/Scenes/Game.unity` — **die einzige Szene**. Menü, Hangar/Shop
  und Gameplay laufen alle darin; "Quit" lädt die Szene komplett neu.
- `Space-Escaper/Assets/AudioSystem/` — Audio-Clips. Präfix `C_` = classic (Originale
  von 2020), `N_` = neu. Der Laufzeit-Umschalter dazwischen ist `AudioSystem.useNewSounds`.
- `Space-Escaper/ProjectSettings/` — Android-Buildeinstellungen, Tags, Layer.

## Arbeitsweise

- **Unity ist die Wahrheit beim Kompilieren.** VS-Code-/OmniSharp-Warnungen sagen nichts
  darüber aus, ob das Projekt in Unity baut. Nicht auf Basis von Editor-Warnungen "reparieren".
- Änderungen klein und fokussiert halten; lieber die Ursache beheben als das Symptom.
- Bestehende Struktur und Namensgebung nicht ohne Anlass umbenennen.
- Kein Gameplay-System entfernen/umbenennen, ohne vorher alle Aufrufstellen zu prüfen —
  vieles hängt über Inspector-Referenzen und Unity-Events zusammen, nicht über Code-Aufrufe.
  Grep findet diese Verbindungen nicht; `Game.unity` und die Prefabs mitdurchsuchen.
- Keine destruktiven Git-/Dateioperationen ohne ausdrückliche Aufforderung.

## Unity-Besonderheiten in diesem Repo

- **Git LFS ist aktiv** (siehe `.gitattributes`) — auch für `.unity`, `.asset`, `.anim`,
  `.controller`. Diese Dateien sind dadurch nicht diffbar.
- **`.meta`-Dateien immer mit committen.** Nach einem Unity-Upgrade ändern sich massenhaft
  `.meta`-Dateien (serializedVersion-Bumps) — das ist normal und gehört in einen eigenen
  Upgrade-Commit, nicht vermischt mit inhaltlichen Änderungen.
- Assets, die nicht in einer Szene oder einem Prefab referenziert sind, landen nicht im
  Build — Aufräumen in `Assets/` ist also Repo-Hygiene, keine Build-Größen-Optimierung.

## Bekannte Altlasten (bewusst, noch offen)

- `Assets/UI/` und `Assets/UI/Images/` enthalten 39 bitgleiche Duplikat-PNGs. Unklar,
  welche Kopie die Szene referenziert — vor UI-Arbeiten klären.
- Alle Audio-Clips stehen auf *Decompress on Load*; Musik gehört auf *Streaming*.
- Reste der 2023 entfernten Google-Play-Games-Integration: `GooglePlayGameSettings.txt`,
  `GvhProjectSettings.xml`, `AndroidResolverDependencies.xml`, `com.google` Scoped
  Registry in `manifest.json`.
- `AndroidTargetSdkVersion: 29` — zu alt für Play-Store-Uploads.
- Standalone-App-ID ist noch `unity.DefaultCompany.FPS2` (Tutorial-Überbleibsel).
- Revive-Mechanik ist funktionslos, seit Unity Ads entfernt wurde: `RequestRevive()`
  ruft `Revive()` ohne Gegenleistung durch.
- Highscore-Leaderboard entfiel mit Google Play Games; es gibt nur noch lokale
  `PlayerPrefs`-Werte.

## Sprache

Antworten auf Deutsch. Code, Bezeichner und Commit-Messages auf Englisch
(bestehende Konvention: `add:`, `edit:`, `fix:`, `refactor:`).
