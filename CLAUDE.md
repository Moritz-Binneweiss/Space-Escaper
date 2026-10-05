# Space Escaper

3-Lane Endless Runner für Android, Unity **6000.6.0f1**, URP 17.6.0.
Ursprünglich 2020 in Unity 2019.4 gebaut und im Play Store veröffentlicht — aktuell
ein Legacy-Projekt, das schrittweise modernisiert wird. Hobbyprojekt, kein Zeitdruck,
Arbeit passiert in unregelmäßigen Sessions.

## Aufbau

Repo-Root ≠ Unity-Projekt: das Unity-Projekt liegt in `Space-Escaper/`.

- `Space-Escaper/Assets/Scripts/` — 14 Skripte, ~1600 Zeilen. Einstiegspunkte:
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
- Reste der im Januar 2026 (v1.3.2) entfernten Google-Play-Games-Integration:
  `GooglePlayGameSettings.txt`, `GvhProjectSettings.xml`,
  `AndroidResolverDependencies.xml`, `com.google` Scoped Registry in `manifest.json`.
- **Play Store kommt erst mit v2.0** — als *neuer* Store-Eintrag, nicht als Update des
  alten von 2020 (Entscheidung Oktober 2026). Bis dahin bewusst offen:
  `AndroidTargetSdkVersion: 29` (Google verlangt seit 31.08.2026 API 36),
  Debug-Signatur (`androidUseCustomKeystore: 0`), App Bundle. Der alte Paketname
  `com.ANIMOGames.SpaceEscaper` bleibt bei Google Play dem alten Eintrag zugeordnet
  und ist nicht wiederverwendbar — der neue Eintrag braucht einen neuen. Alte
  Spielstände müssen deshalb nicht migriert werden.
- Android läuft mit festen **30 fps**: Standard-Qualitätsstufe ist „Fastest" (vSync
  aus), und kein Skript setzt `Application.targetFrameRate`.
- Standalone-App-ID ist noch `unity.DefaultCompany.FPS2` (Tutorial-Überbleibsel).
- Revive-Mechanik ist funktionslos, seit Unity Ads entfernt wurde: `RequestRevive()`
  ruft `Revive()` ohne Gegenleistung durch. Der Revive-Button ruft per OnClick
  zusätzlich `TileManager.RespawnTile` auf — diese Verbindung existiert nur in der
  Szene, nicht im Code.
- Highscore-Leaderboard entfiel mit Google Play Games; es gibt nur noch lokale
  `PlayerPrefs`-Werte. Der Pokal-Button im Hauptmenü (`UI/Menu/Leaderboard`) bleibt
  trotzdem **bewusst sichtbar**, auch ohne Funktion (Entscheidung Oktober 2026) —
  nicht ausblenden oder löschen. Sein OnClick-Aufruf zeigte auf eine nicht mehr
  existierende Methode und ist entfernt; der Button spielt nur den Klicksound.
- Auf dem GameObject `Settings` hängt ein zweiter, unverdrahteter `SettingsManager`.
  Alle Buttons nutzen den auf `GameManager`; der zweite ist toter Ballast.

## Spielstand (PlayerPrefs)

Schlüssel: `MenuCoins`, `Hiscore`, `CurrentShip`, `CurrentShop`, `UnlockedShips`, dazu
die Audio-Schlüssel (siehe „Audio-System“). Mit v1.4.1 (Oktober 2026) wurden hier
mehrere Bugs behoben; die Regeln dahinter:

- `UnlockedShips` ist eine Bitmaske (Bit n = Schiff n). Bis Januar 2026 lag sie nur
  im Google-Play-Games-Cloud-Save und fiel mit GPG ersatzlos weg — gekaufte Schiffe
  waren danach nach jedem Szenen-Reload wieder gesperrt. Beim Laden gelten
  Startschiff und aktuelles Schiff immer als freigeschaltet und werden sofort
  zurückgeschrieben — sonst wäre ein nur implizit besessenes Schiff nach einem
  Wechsel wieder gesperrt.
- Fehlt `CurrentShip` (Neuinstallation) oder ist der Wert ungültig, fällt `Awake()`
  aufs Startschiff zurück. Der Default 0 zeigte früher auf `FlameContainer` statt auf
  ein Schiff und ließ `Awake()` und `OnDeath()` abstürzen. Lokal fiel das nie auf,
  weil die PlayerPrefs gesetzt waren — **Neuinstallation gezielt testen.**
- Der Score wird überall abgeschnitten angezeigt und gespeichert (`(int)score`), sonst
  weichen Todesbildschirm und Highscore voneinander ab.
- Münzen werden bei jedem Tod gutgeschrieben, nach einem Revive aber nur die seitdem
  gesammelten (`bankedCoins`).
- **Schiffspreise** (Oktober 2026 neu balanciert, vorher Testwerte von 1 Münze):
  ARISTOCRAT-Skins **250**, FREETER **750**, VAGOR **1.250** — ein Preis pro Familie
  wie im Original. Grundlage: Ø 5,9 Münzen pro Abschnitt (60 Einheiten), Strecke
  nach T Sekunden = 11·T + 0,02·T², davon 60 % eingesammelt → ein Ø-Run von 60 s
  bringt ~40 Münzen. Ziel: erstes Skin in der ersten Session, erster FREETER nach
  ~45 min, erster VAGOR nach ~2 h, alles nach ~3,3 h. Die Originalpreise von 2021
  (3.500 / 6.000 / 8.000) hätten über 20 h gebraucht. Der derzeit kostenlose
  Revive hebt das Einkommen pro Run grob um 50–70 % — nach dessen Umbau (v1.9.2)
  die Preise gegenprüfen.

## Shop

Bis zum geplanten Umbau auf ScriptableObjects hängt der Shop an Kind-Indizes. Schiffe
sind 1–9 nummeriert: ARISTOCRAT 1–3, FREETER 4–6, VAGOR 7–9 (je drei Skins bilden eine
Familie).

- `shipContainer`: Kind 0 = `FlameContainer`, Kinder 1–9 = Schiffe, Index = Schiff
- `buttonContainer`, `shopShipContainer`: Index = Schiff − 1
- `shopSpriteContainer`: 0 = Select, 1 = Selected, Schiff + 1 = Preisschild
- `flameContainer`, `skinButtonContainer`: Index = Familie (0–2)
- `currentShop` ist die Familie des *geflogenen* Schiffs, `selectedShop` die gerade im
  Hangar *angesehene*. Flammen und alles im Spiel gehören an `currentShop` — die
  Verwechslung zeigte früher eine falsche, neben dem Schiff schwebende Flamme.

**Antriebsflammen** liegen im Prefab `Playership.prefab` (`FlameACT`, `FlameFTR` und
eine Gruppe mit zwei `FlameVGR`), alle mit eingeschalteter Emission. Der Code schaltet
nur die GameObjects an und aus. `PlayerMotor.drive` zeigt zusätzlich auf `FlameACT`
und schaltet deren Emission im Run selbst ein. Deshalb **keine Emission-Overrides in
der Szene**: Ein versehentlicher Override (September 2026, v1.4.0) ließ den FREETER
bis v1.4.2 ohne sichtbaren Antrieb fliegen. Beim ARISTOCRAT fiel derselbe Override
nicht auf, weil `drive` die Emission dort ohnehin einschaltet.

**Preisschilder sind Bilder, keine Texte.** `shipPrices` allein zu ändern reicht
nicht — der Shop zeigt den Preis aus `Assets/UI/Images/ACT_250.png`, `FTR_750.png`
und `VGR_1250.png`. Aufbau wie bei den Originalen (`ACT.png`, `FTR.png`, `VGR.png`,
pixelgleich nachgeprüft): `ButtonBlanco.png` als Platte, `SECHSKANTMUTTER (1).png`
als Icon, Zahl in `neuropol x rg.ttf` mit 49 px Ziffernhöhe ab y = 71, Icon und Zahl
als Gruppe mittig. Die Original-Schilder mit den alten Preisen liegen unverändert
daneben und sind in der Szene nicht mehr referenziert.

## Eingabe (Input System)

Seit Oktober 2026 (v1.4.2) läuft alle Eingabe über das **Input System Package**
(`com.unity.inputsystem` 1.20.1). Active Input Handling steht auf „Input System
Package (New)" (`activeInputHandler: 1`), der alte Input Manager ist aus. Diese
Einstellung zu ändern erfordert einen Editor-Neustart.

- **Kein `UnityEngine.Input` mehr verwenden** (`Input.GetMouseButton`, `Input.touches`,
  `Input.GetKey` …). Das kompiliert weiterhin, wirft zur Laufzeit aber eine
  `InvalidOperationException`. Neuer Code liest Geräte über `UnityEngine.InputSystem`.
- `MobileInput` liest Touch und Maus über **einen** Codepfad: `Pointer.current` ist
  auf dem Handy der Touchscreen (erster Finger), im Editor die Maus. Die Wisch-Logik
  ist dieselbe wie vorher: 100 px Deadzone, ein Wischer pro Berührung, ausgelöst
  schon während des Ziehens.
- **Pfeiltasten ←/→** wechseln die Spur wie ein Wischer, zum Testen im Play Mode.
  Im Editor kommen Tastatur und Maus nur an, wenn das Game-Fenster den Fokus hat
  (Standardverhalten des Input Systems) — vorher einmal ins Game-Fenster klicken.
- Das UI läuft über `InputSystemUIInputModule` am EventSystem, mit den
  Standardaktionen aus dem Paket (`DefaultInputActions`). Das alte
  `StandaloneInputModule` erkennt mit dem neuen Backend keinen Klick mehr — nicht
  zurücktauschen.
- Eingaben lassen sich im Play Mode per `InputSystem.QueueStateEvent` simulieren
  (so wurde die Umstellung getestet: UI-Klick, Pfeiltasten, Maus-Wischer). Ohne
  Fokus aufs Game-Fenster dafür vorübergehend `InputSystem.settings`
  umstellen (`editorInputBehaviorInPlayMode = AllDeviceInputAlwaysGoesToGameView`,
  `backgroundBehavior = IgnoreFocus`) und danach zurücksetzen. Die Settings sind
  kein Asset, sie leben nur im Speicher.

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
  Das ist gewollt und kein Fehler. Die neuen SFX sind auf **v1.7.0** verschoben
  (Art- und Sound-Remaster); v1.4.0 wurde ohne sie abgeschlossen.

Vier Fallen, die hier schon einmal Bugs verursacht haben:

- **UI-Referenzen und DontDestroyOnLoad.** Die Bedienelemente liegen unter
  `UI/Settings` in der Szene (Toggle „Use new Sound", Lautstärkeregler, SFX- und
  Musik-Button), das AudioSystem überlebt aber den Reload. Deshalb übergibt die
  Szenenkopie in `Awake()` ihre frischen Referenzen an die überlebende Instanz
  (`AdoptSceneReferencesFrom`), bevor sie sich zerstört. Ohne das sind Mute-Buttons
  und Regler nach dem ersten Reload tot. **Neue UI-Elemente dort mit eintragen**,
  sonst überleben sie den Szenenwechsel nicht.
  Aus demselben Grund **keine persistenten OnClick-Aufrufe ins AudioSystem**:
  Klicksounds kommen von der Komponente `ButtonClickSound` am jeweiligen Button, die
  über `AudioSystem.Instance` geht. Die alten Klick-Events zeigten seit Februar 2026
  ins Leere. Kauf-Buttons haben bewusst keinen Klick, sie spielen eigene
  Select-/Kauf-Sounds; der Start-Button hatte nie einen.
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
v1.7.0 (Art- und Sound-Remaster).

Das Material haengt an einem Child namens `Shockwave` in zwei Prefabs:
`BigExplosion.prefab` (die Todesexplosion des Spielers) und `Shockwave.prefab`.
Beide Prefabs sind unveraendert - der Fix sitzt nur im Material.

## Sprache

Antworten auf Deutsch. Code, Bezeichner und Commit-Messages auf Englisch
(bestehende Konvention: `add:`, `edit:`, `fix:`, `refactor:`).
