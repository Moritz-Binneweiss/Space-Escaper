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
- `Space-Escaper/Assets/AudioSystem/` — Audio-Clips plus `ClassicBank.asset`,
  `NewBank.asset` und `GameAudio.mixer`. Dateipräfix `C_` = classic (Originale von
  2020), `N_` = neu. Siehe Abschnitt „Audio-System“ unten.
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

- **Git LFS ist aktiv** (siehe `.gitattributes`), seit September 2026 aber **nur noch
  für echte Binärdateien** (Texturen, Audio, Meshes, Libraries). Unity-YAML wie
  `.unity`, `.prefab`, `.asset`, `.mat`, `.anim`, `.controller` liegt als Text im Repo
  und ist damit diffbar — das Projekt nutzt Force Text serialization.
  Diese Formate stehen bewusst auf `merge=binary`: Git würde bei einem Merge-Konflikt
  zeilenweise mischen und dabei kaputtes YAML erzeugen. Ein Konflikt soll deshalb
  hart fehlschlagen. Neue Textformate gehören **nicht** in LFS.
- **`.meta`-Dateien immer mit committen.** Nach einem Unity-Upgrade ändern sich massenhaft
  `.meta`-Dateien (serializedVersion-Bumps) — das ist normal und gehört in einen eigenen
  Upgrade-Commit, nicht vermischt mit inhaltlichen Änderungen.
- Assets, die nicht in einer Szene oder einem Prefab referenziert sind, landen nicht im
  Build — Aufräumen in `Assets/` ist also Repo-Hygiene, keine Build-Größen-Optimierung.

## Bekannte Altlasten (bewusst, noch offen)

- `Assets/UI/` und `Assets/UI/Images/` enthalten 39 bitgleiche Duplikat-PNGs. Unklar,
  welche Kopie die Szene referenziert — vor UI-Arbeiten klären.
- Reste der 2023 entfernten Google-Play-Games-Integration: `GooglePlayGameSettings.txt`,
  `GvhProjectSettings.xml`, `AndroidResolverDependencies.xml`, `com.google` Scoped
  Registry in `manifest.json`.
- `AndroidTargetSdkVersion: 29` — zu alt für Play-Store-Uploads.
- Standalone-App-ID ist noch `unity.DefaultCompany.FPS2` (Tutorial-Überbleibsel).
- Revive-Mechanik ist funktionslos, seit Unity Ads entfernt wurde: `RequestRevive()`
  ruft `Revive()` ohne Gegenleistung durch.
- Highscore-Leaderboard entfiel mit Google Play Games; es gibt nur noch lokale
  `PlayerPrefs`-Werte.

## Audio-System

Zentral ist `AudioSystem` (DontDestroyOnLoad, Singleton) mit drei AudioSources:
One-Shot-SFX, Motor-Loop, Musik. Alle drei routen in `GameAudio.mixer`
(Master → Music / SFX).

**Clips liegen nicht im AudioSystem, sondern in `AudioBank`-ScriptableObjects** —
`ClassicBank.asset` (Originalsounds 2020) und `NewBank.asset`. Umschalten heißt
Bank wechseln. Wichtig für die Erweiterung:

- **Einen neuen Sound hinzufügen = ein Feld in `AudioBank.cs`.** Beide Banks bieten
  den Slot dann automatisch an. Nicht zurück zu Einzelfeldern im AudioSystem gehen.
- Ein leerer Slot fällt automatisch auf die andere Bank zurück. `NewBank` enthält
  aktuell nur die zwei neuen Musikstücke; alle SFX kommen deshalb noch aus Classic.
  Das ist gewollt und kein Fehler.

Vier Fallen, die hier schon einmal Bugs verursacht haben:

- **UI-Referenzen und DontDestroyOnLoad.** Die Bedienelemente liegen unter
  `UI/Settings` in der Szene (Toggle „Use new Sound", Lautstärkeregler, SFX- und
  Musik-Button), das AudioSystem überlebt aber den Reload. Deshalb übergibt die
  Szenenkopie in `Awake()` ihre frischen Referenzen an die überlebende Instanz
  (`AdoptSceneReferencesFrom`), bevor sie sich zerstört. Ohne das sind Mute-Buttons
  und Regler nach dem ersten Reload tot. **Neue UI-Elemente dort mit eintragen**,
  sonst überleben sie den Szenenwechsel nicht.
- **Musik nicht an zwei Stellen steuern.** `PlayerMotor.StartRunning()` rief früher
  zusätzlich einen Musik-Stopp auf und würgte damit die gerade gestartete
  Spielmusik ab. Musik gehört ausschließlich in `GameManager`.
- **Mute ist Pause, nicht Lautstärke 0.** Musik wird pausiert (Position bleibt
  erhalten, Dekodierung stoppt wirklich), SFX werden gar nicht erst abgespielt.
- **Der Lautstärkeregler ist nicht linear.** `SetMasterVolume` bildet die
  Reglerposition exponentiell auf einen 40-dB-Bereich ab (`VolumeRangeDb`), sonst
  passiert die gesamte hörbare Änderung in den unteren 20 % des Wegs. Wichtig:
  Den Wert zu quadrieren hilft **nicht** — eine Potenzkurve streckt den dB-Bereich
  gleichmäßig und lässt das Ungleichgewicht bestehen. In den PlayerPrefs steht die
  Reglerposition, nicht die Amplitude. Beim Ziehen wird bewusst kein
  `PlayerPrefs.Save()` aufgerufen (60×/Sekunde Schreibzugriff); der Wert wird in
  `OnApplicationPause`/`OnApplicationQuit` weggeschrieben.

**Import-Einstellungen** (September 2026 nach Unity-Empfehlung gesetzt): Musik auf
*Streaming* + Load In Background, SFX auf *Decompress on Load* mit 22050 Hz und
Vorbis-Qualität 0.6. Das hat den PCM-Speicher von ~75 MB auf ~0,8 MB gesenkt. Neue
Clips entsprechend importieren, sonst landen sie wieder komplett im RAM.

## Render Pipeline: URP

Das Projekt lief bis September 2026 auf der Built-in Render Pipeline und wurde auf
**URP 17.6.0** umgestellt. Die Migration ist abgeschlossen: URP Asset liegt unter
`Assets/New Universal Render Pipeline Asset.asset` und ist in Project Settings >
Graphics zugewiesen, alle Materialien sind konvertiert.

Shader-Verteilung: 34 Materialien auf `ANIMO/BendWorld`, 13 auf URP/Lit, 7 auf URP
Particles (Lit/Unlit), 2 TextMesh Pro, 1 Skybox.

`Assets/Shader/BendWorld.shader` ist der handportierte Curved-World-Shader (vorher
Surface Shader, jetzt URP mit ForwardLit / ShadowCaster / DepthOnly / DepthNormals).
Wichtig bei Änderungen daran:

- Die Krümmung (`BendObjectPosition`) muss in **jedem** Pass angewandt werden, sonst
  passen Schatten und Tiefe nicht zur sichtbaren Geometrie. Im Built-in erledigte
  das `addshadow`.
- Property-Namen `_MainTex` und `_Curvature` sind bewusst URP-untypisch beibehalten
  (statt `_BaseMap`), damit die 34 Materialien ihre Zuweisungen und Werte behalten.
  Beim Umbenennen wären sie alle weg.
- Alle Properties gehören in den einen `UnityPerMaterial`-CBUFFER, sonst bricht die
  SRP-Batcher-Kompatibilitaet.

**Bekannte Regression (provisorisch entschaerft, echte Loesung offen):**
`Effects/.../Materials/ExplosionDistortion.mat` nutzte im Built-in einen
GrabPass-Verzerrungsshader. GrabPass gibt es in URP nicht; der Converter hat das
Material auf URP Particles/Unlit gesetzt. Ergebnis: statt einer unsichtbaren
Verzerrung ein sichtbarer, hell-oranger Sprite mitten in der Explosion
(im Play Mode bestaetigt).

Provisorischer Fix (September 2026): Alpha von `_BaseColor` und `_Color` auf **0**
gesetzt, RGB absichtlich stehen gelassen, damit der Originalwert
(1.189, 0.767, 0.434) noch ablesbar ist. Das Partikel ist damit unsichtbar - der
Zustand entspricht optisch dem vor der Migration.

Was weiterhin fehlt: der Verzerrungseffekt selbst. Ein echter Ersatz waere ein
URP-Shader ueber die Opaque Texture (Shader Graph, Scene-Color-Node). Gehoert zu
v1.7.0 (Art-Remaster).

Das Material haengt an einem Child namens `Shockwave` in zwei Prefabs:
`BigExplosion.prefab` (die Todesexplosion des Spielers) und `Shockwave.prefab`.
Beide Prefabs sind unveraendert - der Fix sitzt nur im Material.

## Sprache

Antworten auf Deutsch. Code, Bezeichner und Commit-Messages auf Englisch
(bestehende Konvention: `add:`, `edit:`, `fix:`, `refactor:`).
