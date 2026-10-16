# Space Escaper

3-Lane Endless Runner für Android, Unity **6000.6.0f1**, URP 17.6.0.
Ursprünglich 2020 in Unity 2019.4 gebaut und im Play Store veröffentlicht - aktuell
ein Legacy-Projekt, das schrittweise modernisiert wird. Hobbyprojekt, kein Zeitdruck,
Arbeit passiert in unregelmäßigen Sessions.

## Aufbau

Repo-Root ≠ Unity-Projekt: das Unity-Projekt liegt in `Space-Escaper/`.

- `Space-Escaper/Assets/Scripts/` - 34 Skripte, ~3000 Zeilen, alle im Namespace
  `SpaceEscaper` (Stil siehe „Code-Stil“), in Ordnern nach Bereich (siehe
  „Ordnerstruktur“). Einstiegspunkte: `Game/GameManager.cs` (Spielzustand),
  `Game/RunScore.cs` (Score und Münzen eines Runs), `Ship/PlayerMotor.cs` und
  `Ship/PlayerShip.cs` (das Schiff), je eine Komponente pro Screen in `UI/`
  (`MainMenuScreen.cs` usw., siehe „UI“), `Audio/AudioSystem.cs`,
  `Ship/MobileInput.cs`, `World/TileManager.cs` (Spawning), `Save/SaveSystem.cs` /
  `Save/SaveData.cs` (Spielstand), `Ship/ShipData.cs` / `Ship/ShipCatalog.cs`
  (Schiffe).
- `Space-Escaper/Assets/Tests/` - EditMode-Tests in `EditMode/`, ein PlayMode-Test in
  `PlayMode/`, siehe „Tests“. Dort liegt auch `TestFrameworkFix`, siehe „Play Mode
  ohne Domain Reload“ unter „Unity-Besonderheiten“.
- `Space-Escaper/Assets/Editor/` - das Test-Menü im Editor, siehe „Tests“.
- `Space-Escaper/Assets/Scenes/Game.unity` - **die einzige Szene**. Menü, Hangar/Shop
  und Gameplay laufen alle darin; "Quit" lädt die Szene komplett neu.
- `Space-Escaper/Assets/Audio/` - Audio-Clips in `Classic/` (Originale von 2020)
  und `New/`, dazu `ClassicBank.asset`, `NewBank.asset` und `GameAudio.mixer`, in
  `New/` außerdem das Ableton-Projekt der neuen SFX. Siehe Abschnitt „Audio-System“
  unten.
- Alle übrigen Ordner in `Assets/` sind nach Asset-Typ sortiert, siehe
  „Ordnerstruktur“. `TextMesh Pro/` und `Effects/EffectExamples/` sind
  Fremd-Pakete. Benennung siehe „Benennung von Dateien und Objekten“.
- `Space-Escaper/ProjectSettings/` - Android-Buildeinstellungen, Tags, Layer.

## Arbeitsweise

- **Unity ist die Wahrheit beim Kompilieren.** Seit v1.4.11 kennt VS Code die Projekte
  wieder und zeigt echte Fehler samt Unity-Analyzern (siehe „VS Code“ unter
  „Unity-Besonderheiten“), bis dahin sagten seine Warnungen nichts. Ob das Projekt baut,
  entscheidet trotzdem Unity, etwa nach neuen Skripten, bevor Unity die Projektdateien
  neu geschrieben hat.
- Änderungen klein und fokussiert halten; lieber die Ursache beheben als das Symptom.
- Bestehende Struktur und Namensgebung nicht ohne Anlass umbenennen.
- Kein Gameplay-System entfernen/umbenennen, ohne vorher alle Aufrufstellen zu prüfen -
  vieles hängt über Inspector-Referenzen und Unity-Events zusammen, nicht über Code-Aufrufe.
  Grep findet diese Verbindungen nicht; `Game.unity` und die Prefabs mitdurchsuchen.
- Keine destruktiven Git-/Dateioperationen ohne ausdrückliche Aufforderung.
- **Tests auf dem Handy** ausführlich erst kurz vor v2.0 (Entscheidung Oktober 2026).
  Bis dahin reichen Tests im Editor und kurze Stichproben auf dem Handy. Test-APKs
  nur bauen, wenn danach gefragt wird.

## Code-Stil

Seit v1.4.5 folgen alle Skripte dem Unity-Styleguide „Use a C# style guide for
clean and scalable game code“ (Unity-6-Ausgabe, 2025, PDF über
unity.com/resources/c-sharp-style-guide-unity-6). Wo er die Wahl lässt, gilt hier:

- **Namespace** `SpaceEscaper` für alle Skripte. In `unity command eval` deshalb
  `SpaceEscaper.GameManager` usw. schreiben. Veraltete APIs lehnt `eval` als
  Fehler ab, in Unity 6.6 etwa `FindObjectsByType` mit `FindObjectsSortMode` und
  `GetInstanceID`.
- **Namen:** private Felder `m_camelCase`, private statische Felder `s_camelCase`,
  Konstanten `k_PascalCase`, Typen, Methoden und Properties PascalCase, lokale
  Variablen und Parameter camelCase. Booleans beginnen mit einem Verb
  (`m_isMusicMuted`, `HasSwipedLeft`), Methoden auch (`OpenHangar` statt `HangarOn`).
  Keine Abkürzungen außer in Mathe (`k_VolumeRangeDb`).
- **Keine öffentlichen Felder.** Was im Inspector stehen soll, ist
  `[SerializeField] private` (Attribut in derselben Zeile), andere Klassen lesen
  über Properties (`public bool IsMusicMuted => m_isMusicMuted;`).
- **Strings als Konstanten** oben in der Klasse: Animator-Trigger, Tags,
  Szenenname, Dateiname des Spielstands. Die Werte selbst sind Daten und bleiben,
  wie sie sind (etwa der Tag `Obstacle` oder `SaveData.json`), sonst brechen
  Prefabs und Spielstände.
- **Formatierung:** Allman-Klammern, 4 Leerzeichen, Klammern auch um einzelne
  Anweisungen, eine Deklaration pro Zeile, `private` immer ausgeschrieben, `switch`
  mit `default`, Zeilen höchstens 120 Zeichen, UTF-8 ohne BOM, LF. Die
  `.editorconfig` im Repo-Root hält das für die IDE fest.
- **Reihenfolge in der Klasse:** Konstanten, serialisierte Felder, private Felder,
  Properties, Events, Unity-Methoden (`Awake`, `Start`, `Update` …), öffentliche
  Methoden, private Methoden.
- **Events** (seit v1.4.13, das erste ist `FloatingOrigin.WorldShifted`) wie im
  Styleguide: `event Action<…>`, benannt als Verbphrase, im Partizip für „danach“
  (`WorldShifted`). Ausgelöst wird es in einer Methode mit `On` davor
  (`OnWorldShifted`), Handler heißen `Subjekt_Event`
  (`FloatingOrigin_WorldShifted`). An- und abmelden in `OnEnable` und `OnDisable`.
- **Statische Felder setzen sich selbst zurück** (seit v1.5.0), denn der Editor startet
  den Play Mode ohne Domain Reload (siehe „Play Mode ohne Domain Reload“ unter
  „Unity-Besonderheiten“). Jede Klasse leert ihre statischen Felder, Events und
  Properties in einer Methode `ResetStaticState` mit
  `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`,
  in Komponenten vor `Awake`. Die ruft Unity beim Start des Play Mode als Erstes auf,
  vor dem Laden der Szene. Konstanten und `static readonly`-Werte bleiben, wie sie
  sind, eine `static readonly`-Liste wird geleert (`SpinManager`). Ein neues
  statisches Feld gehört mit hinein, sonst meldet es `StaticFieldTests`.
- **Kommentare** erklären das Warum, nicht das Was. `/// <summary>` nur an
  öffentlichen Membern, wo der Name nicht reicht, `[Tooltip]` statt Kommentar an
  serialisierten Feldern. Kein auskommentierter Code, keine `#region`, keine
  Trennlinien und keine Tagebuch-Kommentare („früher war …“), das gehört in die
  Commits.

**Umbenennen ist hier gefährlich**, weil Szene und Assets über Namen am Code hängen:

- **Serialisierte Felder:** Unity speichert Werte unter dem Feldnamen. Ein
  umbenanntes Feld verliert seine Inspector-Werte, außer es bekommt vorübergehend
  `[FormerlySerializedAs("alterName")]`. Danach die Szene speichern, betroffene
  Assets neu schreiben lassen und das Attribut wieder entfernen. Prefabs dabei
  lieber per Text anpassen: Unity 6.6 schreibt ältere Prefabs beim Speichern
  komplett im neuen Format, das gibt große Diffs ohne inhaltliche Änderung. Aus dem
  `Playership.prefab` kamen Modelle und Flammen in v1.4.8 deshalb per Text heraus.
- **Buttons hängen seit v1.5.0 nicht mehr am Namen:** Jeder Screen verbindet seine
  Buttons im Code (siehe „UI“), ein Umbenennen prüft also der Compiler. Bis v1.4.13
  standen 23 OnClick-Aufrufe als Methodenname in `Game.unity`, ein umbenannter Button
  tat dann einfach nichts. Steht doch wieder ein Aufruf in der Szene, meldet ihn
  `SceneTests`, er liefe zusätzlich zum Code.
- **Klassen von Komponenten:** Datei und Klasse heißen gleich. Umbenennen über
  `AssetDatabase.RenameAsset`, dann bleibt die GUID, und jede Komponente in Szene und
  Prefabs findet ihr Skript. Den Eintrag `m_EditorClassIdentifier`
  (`SpaceEscaper::SpaceEscaper.Klasse`) schreibt Unity 6.6 an vorhandenen Komponenten
  dabei nicht neu, er behält den alten Namen. Per `SerializedObject` nachziehen, so in
  v1.5.0 beim Umbenennen von `AudioSettingsMenu` in `SettingsScreen`.
- **Felder in `SaveData`:** Ihre Namen sind die Schlüssel in der Spielstand-Datei,
  siehe „Spielstand“.
- **Absichern:** vor dem Umbau alle serialisierten Werte dumpen und hinterher
  vergleichen. So lief v1.4.5: 627 Werte, alle gleich. Seit v1.4.11 findet
  `SceneTests` fehlende Skripte und leere Referenzen von selbst. Ob die Werte gleich
  geblieben sind, zeigt weiter nur der Vergleich.

## Tests

Seit v1.4.7 gibt es EditMode-Tests mit dem Unity Test Framework (1.8.0, im
Manifest als direkte Abhängigkeit). Sie liegen in `Assets/Tests/EditMode/` und
prüfen die Regeln des Spielstands ohne Szene: Neuinstallation, älterer Spielstand,
unbekanntes Schiff, Kauf, Münzen nach einem Revive, Highscore, Audio-Einstellungen
und Versionsnummer. Seit v1.4.8 prüfen sie auch die Schiffsdaten
(`ShipCatalogTests`): jedes `ShipData` genau einmal im Katalog, das Startschiff
darin und gratis, alle anderen mit Preis und Preisschild, jedes mit eindeutiger ID,
Modell, Hangar-Größe und Antriebsflamme, jede Familie mit Schiffen. Seit v1.4.9
prüfen sie die Streckenabschnitte (`ChunkTests`): Jeder feste Collider in einem
Chunk trägt `Obstacle`, jeder Trigger ist eine Münze (Tag `Coin` und Komponente
`Coin`), und die Asteroidenfelder haben keine Collider. Seit v1.4.12 haben Chunks
und Asteroidenfelder auch keine Animatoren (siehe „Drehen ohne Animator“ unter
„Aufbau der Szene“). Seit v1.4.11 prüft `SceneTests` die Szene `Game.unity`: kein
fehlendes Skript, jede Referenz unserer Komponenten ist gesetzt (auch in Listen),
jeder Screen unter `UI` hat `Canvas` und `GraphicRaycaster` und keinen Animator, und
der `ScreenManager` kennt jeden Screen genau einmal. Seit v1.4.12 gibt es außerdem
genau einen `SpinManager` und seit v1.4.13 genau einen `FloatingOrigin`. Seit v1.5.0
steht in der Szene kein Aufruf mehr, den ein Button oder ein anderes Bedienelement
auslöst. Bis dahin prüfte der Test, dass jeder Button-Aufruf Ziel, Methode und
Argument findet. Seit v1.5.0 prüft `StaticFieldTests`, dass sich jedes statische Feld
des Spiels beim Start des Play Mode zurücksetzt (siehe „Code-Stil“): Der Test füllt
alle, ruft die Methoden mit `SubsystemRegistration` auf wie Unity und erwartet alle
wieder leer. Danach stellt er die alten Werte wieder her, der Editor liest sie auch.

Seit v1.4.11 gibt es außerdem einen PlayMode-Test in `Assets/Tests/PlayMode/`
(`RunTests`). Er spielt einen Run über die Buttons des Spiels: Play, geradeaus bis
zum ersten Hindernis, dann prüft er Todesbildschirm und Spielstand und seit v1.5.0,
dass der Score dort eine Sekunde lang stehen bleibt. Dann tippt er Revive
und prüft, dass das Schiff 25 Einheiten weiterfliegt, ohne wieder zu crashen. Mit
dem Revive-Bug aus v1.4.4 crashte es im Frame des Revive gleich wieder (siehe
„Bekannte Altlasten“). Seit v1.4.13 rückt die Welt dabei alle 20 statt alle 4800
Einheiten zurück (siehe „Floating Origin“ unter „Aufbau der Szene“), zweimal vor
dem Crash und einmal kurz nach dem Revive, während die Explosion noch brennt. In
jedem Frame prüft er, dass nichts springt: Das Schiff bleibt nahe am Ursprung,
Schiff und Kamera bewegen sich stetig, alles andere behält seinen Platz in der
Welt, die Gruppen der Szene bleiben im Ursprung, und die Abschnitte liegen
lückenlos hintereinander bis zur nächsten Spawn-Position.

- **Ausführen:** im Editor unter Window > General > Test Runner, Reiter EditMode
  oder PlayMode. Per CLI die EditMode-Tests mit
  `unity command run_tests --mode EditMode`. Sie dauern wenige Sekunden, also vor
  jedem Commit, der `SaveData`, `SaveSystem`, `RunCoins`, die Szene oder eine
  Methode ändert, die ein Button aufruft. Den PlayMode-Test nur asynchron:
  `unity command run_tests --mode playmode --async_tests true`, dann
  `unity command test_status` abfragen, bis es nach `running` `completed` meldet.
  Bis der neue Lauf läuft, zeigt es noch das Ergebnis des letzten. Synchron führt
  das Test Framework nur EditMode-Tests aus, die in einem Frame fertig sind, und
  die CLI meldet 0/0. `--mode all` startet asynchron gar nichts, also beide Modi
  einzeln. Mit dem Wechsel in den Play Mode dauert er rund 12 s, also
  vor Commits, die Run, Tod, Revive oder das Spawnen ändern. Meldet ein asynchroner
  PlayMode-Lauf 0/0, greift der Umweg `TestFrameworkFix` nicht mehr (siehe „Play Mode
  ohne Domain Reload“ unter „Unity-Besonderheiten“). Die CLI legt eine
  `TestResults.xml` neben den Spielstand, die darf weg. Ist die offene Szene
  ungespeichert, fragt der Test Runner vorher per Dialog, ob er sie speichern soll.
  Bis jemand klickt, steht Unity, und die CLI bricht nach 30 s ab.
- **Szenen im Test:** Während des Laufs tauscht der Test Runner die offenen Szenen
  gegen eine leere und stellt sie danach wieder her. `SceneTests` öffnet
  `Game.unity` deshalb selbst daneben und schließt sie am Ende wieder. Ist sie schon
  geladen, prüft der Test sie, wie sie ist, und lässt sie offen.
- **Der PlayMode-Test spielt echt, aber stumm und ohne den echten Spielstand.** Ein
  Tod schreibt Münzen und Highscore, deshalb lenkt `SetUp` den Spielstand mit
  `SaveSystem.LoadFrom` in einen eigenen Ordner um, im Editor
  `%LOCALAPPDATA%\Temp\ANIMO Games\Space Escaper\PlayModeTests\`. Der beginnt leer
  wie eine Neuinstallation, nur der Lautstärkeregler steht auf 0: Musik und Sounds
  laufen durch denselben Code wie im Spiel, man hört sie nur nicht. Den echten
  Spielstand fasst der Test nicht an (Hash vor und nach jedem Lauf gleich).
  Sichern und zurücklegen ginge nicht sauber: Beim Verlassen des Play Mode speichert
  `AudioSystem.OnApplicationQuit` noch einmal und überschriebe den zurückgelegten
  Stand. Die Umleitung gilt deshalb bis zum Ende des Play Mode, der nächste Start
  lädt wieder den echten Spielstand, auch ohne Domain Reload: `SaveSystem` lädt bei
  jedem Start des Play Mode neu.
- **Tippen wie ein Finger:** Der Test tippt jeden Button an seiner Mitte. Die Buttons
  reichen ihm die Screens (`MainMenuScreen.StartButton` usw., nur für Tests sichtbar).
  Vorher prüft ein Raycast, dass der Tipp dort bei ihm ankommt und nichts darüber liegt.
  Der Tipp selbst kommt in `PreUpdate`, vor allen `Update()`, wie ein echter (siehe
  „Eingabe“). `Random.InitState` wählt immer dieselben Abschnitte, das Schiff
  crasht also immer an derselben Stelle (derzeit bei z = 56,8 nach rund 4,4 s, gezählt,
  als wäre die Welt nie zurückgerückt). Pausiert
  ein Fokuswechsel im Editor den Run, tippt der Test Continue und vermerkt das in
  seiner Ausgabe.
- **Am schnellsten testbar ist, was ohne laufende Szene geht.** Regeln gehören
  deshalb in einfache Klassen wie `SaveData` und `RunCoins`, nicht in MonoBehaviours.
  `SceneTests` sieht nur, wie die Szene verdrahtet ist, nicht, was sie im Spiel tut.
  Run, Tod und Revive spielt `RunTests` durch. Was sonst nur im Spiel geht (dass der
  `ShopScreen` nach einem Kauf sofort speichert, Wischer, Auto-Pause), bleibt beim
  Ausprobieren im Play Mode, siehe „Bewegung testen“ und „Eingabe“.
- **Test-Menü** (seit v1.4.11): in der Menüleiste unter „Space Escaper“, der Code
  in `Editor/TestMenu.cs`.
  - **Reset Save (New Install)** löscht nach einer Rückfrage den Spielstand, der
    nächste Start ist eine Neuinstallation. Im Play Mode beendet es zuerst den Play
    Mode und löscht danach, weil dessen Ende noch einmal speichert.
  - **Add 1000 Coins** und **Unlock All Ships** ändern außerhalb des Play Mode den
    Spielstand auf der Platte, im Play Mode den laufenden. Dort lädt die Szene
    danach neu wie nach „Exit“, damit Hauptmenü und Hangar die Änderung zeigen.
    Ein Run endet dabei.
  - **Invincible** lässt das Schiff durch Hindernisse fliegen: `PlayerMotor`
    ignoriert ein getroffenes Hindernis per `Physics.IgnoreCollision`, statt zu
    crashen, und sammelt weiter Münzen. Am Hindernis hängt es dabei einen Frame
    (gemessen: 3 Frames in 25 s Flug). Der Haken gilt für jeden Play Mode der
    Editor-Sitzung (`SessionState`): `PlayerMotor` schaltet die Unverwundbarkeit beim
    Start des Play Mode aus, das Test-Menü bei `EnteredPlayMode` wieder ein, und eine
    Zeile in der Konsole erinnert daran. Der PlayMode-Test schaltet sie für seinen Lauf
    aus, auch bei gesetztem Haken (geprüft in v1.5.0).
  - **Im Build** schaltet die Unverwundbarkeit ein Panel „Space Escaper“ im
    Rendering Debugger (`Scripts/Game/TestPanel.cs`), auf dem Handy per
    Dreifinger-Doppeltipp. Im Editor steht es im Play Mode unter Window > Analysis
    > Rendering Debugger und gilt nur für den laufenden Play Mode.
  - **In den Settings** steht seit v1.5.0 dafür auch ein Haken „Invincible“ unter dem
    Zurück-Button. Der `SettingsScreen` legt ihn in `Awake` als Kopie von „Use new
    Sound“ an, in der Szene steht er nicht. Auf dem Handy ist er leichter zu treffen
    als der Dreifinger-Doppeltipp, der auf dem Pixel 9 Pro nicht immer klappte, und er
    braucht den Rendering Debugger nicht. Der ist in URP 17.6 mit UI Toolkit gebaut:
    Sein erstes Öffnen kostete dort einen Frame von 561 ms und 10 MB, danach legt er
    in jedem Frame weiter Speicher an, auch geschlossen.
  - **Nie im Release:** Schalter, Panel und Haken stehen unter
    `#if UNITY_EDITOR || UNITY_ENABLE_CHECKS`. `UNITY_ENABLE_CHECKS` setzen die
    Managed Code Variants „Checked“ und „Debug“, also genau die Builds, in denen
    es auch den Rendering Debugger gibt. `DEVELOPMENT_BUILD` meldet der
    Unity-Analyzer in Unity 6.6 als veraltet (UAC0009). Der Haken „Development
    Build“ allein setzt keine Variante, für Android steht sie in den Player
    Settings auf Release: Ein solcher Build hat weder Panel noch Rendering Debugger
    (geprüft in v1.4.12).
- **Ein neuer Test sollte einmal rot gewesen sein.** Den Bug dafür kurz wieder
  einbauen und prüfen, dass der Test ihn findet. So in v1.4.7: Mit dem alten
  Revive-Bug zeigte der Test 13 statt 8 Münzen, ohne den Rückfall aufs Startschiff
  scheiterten alle vier ungültigen Schiffe. In v1.4.9 schlug der Chunk-Test vor der
  Reparatur genau an den drei `Asteroid017` an. In v1.4.11 fand der Szenen-Test
  alle Fehler, die eine kaputt gespeicherte Kopie der Szene enthielt: eine
  umbenannte Methode, einen Skin-Button ohne `ShipData`, eine leere Referenz, eine
  Lücke in der Prefab-Liste des `TileManager`, einen Screen ohne Raycaster, einen
  mit Animator, das Hauptmenü doppelt im `ScreenManager` und ein fehlendes Skript.
  Der PlayMode-Test scheiterte mit `RespawnTiles` ohne das Ausschalten der alten
  Abschnitte im Frame des Revive. Tippte er dabei aus dem Test selbst, also nach
  `Update()`, blieb er grün: So wäre der Bug durchgerutscht. In v1.4.13 fand er vier
  eingebaute Fehler im Floating Origin: eine Spawn-Position, die nicht mitrückt,
  Gruppen, die als Ganzes rücken, eine Welt, die nie zurückrückt, und Abschnitte
  und Effekte, die stehen bleiben. Die Spawn-Position fiel erst mit dem Vergleich
  zum letzten Abschnitt auf: Das Schiff kommt lokal nie weit genug, um einen
  Abschnitt zu recyceln, eine Lücke entstand also nie. In v1.5.0 scheiterte die
  Prüfung des Scores mit dem alten Code: nach einer Sekunde Todesbildschirm 16,13 statt
  13,13, nach dem Aufteilen in ganzen Punkten des `RunScore` 16 statt 13. Seit v1.5.0
  fand der Szenen-Test in der Szene des Commits davor alle 23 Button-Aufrufe, und
  „nur das Hauptmenü ist an“ schlug an, sobald in einer Kopie der Szene der Shop an
  oder das Hauptmenü aus war. `StaticFieldTests` fand im Code davor alle 16 statischen
  Felder und ohne die Zeilen für `Coin.Collected` und die Liste des `SpinManager` genau
  diese beiden.
- Die Tests brauchen eigene Assemblies, siehe „Assembly Definitions“ unter
  „Unity-Besonderheiten“.

## Ordnerstruktur

Seit v1.4.5 ist `Assets/` nach Asset-Typ sortiert, wie es Unitys Leitfaden zur
Projektorganisation und die Unity-Templates vorsehen. Im Root von `Assets/` liegen
nur Ordner.

| Ordner | Inhalt |
|---|---|
| `Animations/` | Animator Controller und Clips, nach Gruppe (seit v1.4.12 nur noch `UI/`) |
| `Audio/` | Clips in `Classic/` und `New/`, die beiden Banks, der Mixer, in `New/` das Ableton-Projekt der neuen SFX |
| `Branding/` | App-Icon, Splash-Logos und Splash-Hintergrund (Player Settings), Banner, weitere Logos |
| `Data/` | ScriptableObjects mit Spieldaten, nach Gruppe (`Ships/`: ein `ShipData` pro Schiff und der `ShipCatalog`) |
| `Editor/` | Werkzeuge für den Editor: das Test-Menü und seine Assembly Definition |
| `Materials/` | Materialien, nach Gruppe |
| `Models/` | `.fbx` und `.blend`-Quellen samt ihren Texturen, nach Gruppe |
| `Prefabs/` | nach Gruppe, dazu `Chunks/` (Streckenabschnitte) und `AsteroidFields/` (Hintergrund) |
| `Scenes/` | `Game.unity` |
| `Scripts/` | alle Skripte in Ordnern nach Bereich (siehe unten), im Root nur `SpaceEscaper.asmdef` und `AssemblyInfo.cs` |
| `Settings/` | URP-Asset, Renderer, Global Settings, Volume Profile, Build Profiles |
| `Shaders/` | `BendWorld.shader` |
| `Tests/` | `EditMode/` und `PlayMode/`, je mit den Tests und ihrer Assembly Definition |
| `Textures/` | Texturen der Unity-Materialien, nach Gruppe |
| `UI/` | `Images/` nach Herkunft (siehe unten), `Fonts/`, `Mockups.png` |

- **Gruppen heißen in allen Typ-Ordnern gleich:** `Asteroids`, `Coin`, `Hangar`,
  `Ships`, `Spaceport` und `Skybox`, bei den Animationen dazu `UI`. Was zum
  Schiff gehört, liegt also in `Models/Ships/`, `Materials/Ships/`,
  `Textures/Ships/` und `Prefabs/Ships/`.
- **Neue Assets** kommen in ihren Typ-Ordner und dort in die passende Gruppe,
  nicht lose in einen Typ-Ordner und nie in den Root von `Assets/`. Eine neue
  Gruppe bekommt in jedem Typ-Ordner, den sie braucht, denselben Namen.
- **Modellordner nur als Ganzes verschieben:** Die `.blend`-Quellen verlinken ihre
  Texturen relativ im eigenen Ordner (siehe „Benennung von Dateien und
  Objekten“), deshalb liegen sie in
  `Models/` beieinander und nicht in `Textures/`.
- **Skripte liegen nach Bereich** (seit v1.5.0, bis dahin flach, Entscheidung Oktober
  2026): `Audio/` (`AudioSystem`, `AudioBank`), `Game/` (Spielzustand, Score, Bildrate,
  Test-Panel), `Save/` (Spielstand), `Ship/` (das Schiff, seine Eingabe und die
  Schiffsdaten), `UI/` (die Screens und ihre Bausteine) und `World/` (Strecke,
  Floating Origin, Drehen, Münze, Raumhafen, Hangar, Kamera). Ein neues Skript kommt in
  den Ordner seines Bereichs, die Assembly `SpaceEscaper.asmdef` im Root gilt für alle
  Ordner darunter. Verschieben nur über Unity, siehe „Benennung von Dateien und
  Objekten“. Ordner, die zwischen `AssetDatabase.StartAssetEditing` und
  `StopAssetEditing` entstehen, kennt Unity erst danach, ein `MoveAsset` hinein
  scheitert vorher. Also erst die Ordner anlegen, dann gebündelt verschieben.
- **UI-Bilder liegen nach Herkunft** (seit v1.4.12): `UI/Images/2020/` hat die 38
  Bilder von v1.0 bis v1.2, glänzend orange auf 2048er-Leinwänden. `2021/` hat die 39
  aus dem UI-Umbau von v1.3, flach und weiß, die Shop-Bilder in `2021/Shop/`. `2026/`
  hat die Preisschilder aus v1.4.1. Das Spiel nutzt 36: aus `2020/` nur `CoinIcon`,
  aus `2021/` alle bis auf die leere Platte `ButtonBlank`, die drei Namensschriftzüge
  in `Shop/` und die alten Preisschilder, dazu die drei aus `2026/`. Neue Bilder
  kommen in den Ordner ihres Jahres.
- **Woher die Zuordnung stammt** (die Namen verraten es nicht): Die Bilder von 2020
  lagen auch im Ordner `GUI/` der Quelldateien des Originals, die im Januar 2026 ins
  Repo kamen (`84f02f9`). Sieben tragen einen PNG-Zeitstempel von August oder
  September 2020, und die Szene von v1.3 nutzte nur `CoinIcon`. Die Bilder von v1.3
  sind exakt zugeschnitten, ihre `.meta` kannte schon iOS als Plattform, und die
  Vagor-Bilder gehören dazu (Vagor kam mit v1.3).
- Fremd-Pakete bleiben an ihrem Platz, Asset-Store-Updates erwarten sie dort.

## Aufbau der Szene

Seit v1.4.5 ist `Game.unity` nach Rollen gruppiert. Die Gruppen sind leere
Objekte im Ursprung (Position 0, keine Rotation, Skalierung 1), ihre Kinder
behalten dadurch ihre Weltposition.

| Root-Objekt | Inhalt |
|---|---|
| `Systems` | `GameManager`, `RunScore`, `Hangar`, `TileManager`, `FieldManager`, `SpinManager`, `FloatingOrigin`, `EventSystem` |
| `AudioSystem` | bleibt im Root, `DontDestroyOnLoad` wirkt nur auf Root-Objekte |
| `Playership` | bleibt im Root, eine leere Gruppe wie die anderen. Das Schiff selbst ist ihr Kind `Ship` (`CharacterController`, `PlayerMotor`, `PlayerShip`) und bewegt sich jeden Frame |
| `Cameras` | `MainCamera`, `ShopCamera` |
| `Environment` | `DirectionalLight`, `Spaceport`, `Hangar`, `ShopShips` |
| `UI` | Canvas mit `ScreenManager`, darin je ein Canvas pro Screen mit seiner Komponente: `DeathMenu` (`DeathMenuScreen`), `MainMenu` (`MainMenuScreen`), `GameMenu` (`GameMenuScreen`), `PauseMenu` (`PauseMenuScreen`), `Settings` (`SettingsScreen`), `Shop` (`ShopScreen`), siehe „UI“ |

- **Neue Objekte** kommen in die passende Gruppe. Eine neue Gruppe gehört in den
  Ursprung, sonst verschiebt sie alle Kinder mit, und bleibt leer (nur der
  Transform): Daran erkennt der Floating Origin sie.
- **Was sich ständig bewegt, bleibt flach.** Unity aktualisiert Transforms pro
  Hierarchie, bewegte Kinder in großen Hierarchien kosten mehr. Deshalb liegt
  `Playership` mit dem Schiff im Root, und die Streckenabschnitte spawnen dort
  ebenfalls.
- **Spawning:** `Systems/TileManager` und `Systems/FieldManager` tragen dieselbe
  Komponente `TileManager` (seit v1.4.9, vorher zwei fast gleiche Klassen). Die
  eine legt die Streckenabschnitte vor das Schiff (`Chunk…`, 60 Einheiten lang, 4
  auf einmal), die andere die Asteroidenfelder im Hintergrund (`AsteroidField…`, 40
  lang, 8 auf einmal). Wann ein passierter Abschnitt nach vorn wandert, legt
  `m_recycleDistance` fest: 55 bei der Strecke, 50 bei den Feldern. Nur die Strecke
  setzt ein Revive neu (`m_respawnsOnRevive`), siehe „Bekannte Altlasten“.
- **Floating Origin** (seit v1.4.13): Ist das Schiff 4800 Einheiten vom Ursprung
  entfernt, rückt `Systems/FloatingOrigin` Schiff und Welt um genau diese Strecke
  zurück, alles im selben Frame (`LateUpdate`), das Bild bleibt also gleich. `float`
  rechnet umso gröber, je weiter vom Ursprung: Ohne Zurückrücken wäre das Schiff nach
  10 Minuten (Tempo 37) bei rund 15.000 Einheiten und dort auf etwa ein Tausendstel
  genau, so bleibt alles unter rund 5100 Einheiten. 4800 ist ein Vielfaches beider
  Abschnittslängen (60 und 40). Die Strecke steht als `m_distance` im Inspector und
  lässt sich zum Ausprobieren auch im Play Mode klein stellen.
  - Es rücken alle Root-Objekte außer dem UI. Die Gruppen der Szene (`Systems`,
    `Playership`, `Cameras`, `Environment`) bleiben dabei im Ursprung, ihre Kinder
    rücken: Schiff und Kamera bewegen sich in ihrer Gruppe, ihre lokale Position
    wüchse sonst mit der geflogenen Strecke, und die Genauigkeit wäre wieder weg.
    Welche Objekte Gruppen sind, merkt sich `FloatingOrigin` in `Awake`, bevor der
    erste Abschnitt spawnt, denn auch die Wurzeln der Abschnitte sind leer.
  - Partikel, die im Weltraum simulieren, verschiebt es selbst (derzeit nur die
    Rauchspuren am Debris der Explosion), danach gleicht `Physics.SyncTransforms()`
    die Collider ab.
  - Positionen, die nicht in einem Transform stehen, ziehen über das Event
    `FloatingOrigin.WorldShifted` nach, derzeit nur die Spawn-Position im
    `TileManager`. Neuer Code, der Positionen als Zahl speichert, meldet sich dort an.
    Nicht betroffen: `BendWorld` krümmt relativ zur Kamera, der Sternenhimmel ist eine
    Skybox, die Audio-Quellen sind 2D, und der Score hängt an der Zeit.
  - Geprüft in v1.4.13: Ein Run mit festem Zufall spawnte mit Zurückrücken alle 120
    Einheiten dieselben 46 Abschnitte an denselben Stellen wie ohne. Die Explosion
    behielt beim Zurückrücken mitten im Effekt in jedem Frame ihre Partikelzahlen
    (kein Schwall durch „Emit over Distance“). Ein Bild bei z = 4800 und dasselbe
    nach dem Zurückrücken unterscheiden sich in 0,1 % der Pixel, einzeln und fast
    alle in Partikeln: Das ist die Rechengenauigkeit bei 4800 selbst, und es betrifft
    nur diesen einen Frame. Ein Zurückrücken dauert im Editor 0,2 bis 0,4 ms.
- **Effekte** (Staub beim Einsammeln einer Münze, Explosion beim Crash) entstehen
  zur Laufzeit im Root und löschen sich selbst, der Staub nach 2 s, die Explosion
  nach 4 s (`k_DustLifetime` in `Coin`, `k_ExplosionLifetime` in `PlayerMotor`). Das
  ist die Laufzeit ihrer Partikelsysteme plus die längste Lebensdauer eines
  Teilchens, leer sind sie schon nach 1 und gut 2 s. Bis v1.4.8 blieben sie bis zum
  nächsten Szenen-Reload liegen, jede eingesammelte Münze hinterließ ein Objekt. Wer
  einen Effekt austauscht, etwa im Remaster (v1.7.0), passt die Zeit mit an.
- **Drehen ohne Animator** (seit v1.4.12): Asteroiden und Münzen tragen einen
  `Spinner` mit Grad pro Sekunde um die x-, y- und z-Achse ihres Parents, und
  `Systems/SpinManager` dreht alle in einem einzigen `Update`. Mehrere Achsen lassen
  einen Asteroiden taumeln. Bis v1.4.11 drehte jedes Objekt ein eigener Animator auf
  „Always Animate“, rund 185 gleichzeitig, mit Kurven aus Blender, die in jedem
  Durchlauf bis zum Stillstand abbremsten und wieder anliefen. Jetzt drehen sie
  gleichmäßig (Entscheidung Oktober 2026): jeder Asteroid um dieselben Achsen wie sein
  alter Clip und mit dessen mittlerem Tempo, die Münze mit 100°/s um die Senkrechte, so
  schnell, wie man sie vorher beim Einsammeln sah. `Asteroid005`, `006`, `007`, `017`
  und `025` drehen sich nicht, wie schon vorher. Eingesammelt schrumpft die Münze in
  0,25 s auf null (`Coin`). Ein neues drehendes Objekt bekommt einen `Spinner`, keinen
  Animator.
- **Was der Animator in jedem Frame überschrieb, steht jetzt im Prefab:** die
  Startdrehung von `Asteroid001` (-90° um x) und die Skalierung 1 statt 1,1 des Modells
  in `Asteroid021`. In `AsteroidField004` und `005` saßen drei Modelle versetzt zu ihrer
  Wurzel, im Spiel war das nie zu sehen, die Verschiebung ist weg. Elf stehende
  `Asteroid001` in den Feldern (Animator und Collider entfernt) behalten ihre alte
  Drehung per Override. Geprüft: Alle 564 Renderer in Chunks und Feldern stehen beim
  Spawnen an derselben Stelle, gleich groß und höchstens 0,7° anders gedreht.
- **Die Collider drehen mit,** wie schon mit dem Animator, sie sitzen am selben Objekt
  wie das Modell. Die Kugeln der Asteroiden liegen um den Drehpunkt, die Kollision bleibt
  also gleich, aber die Physik zieht ihre Pose in jedem Frame nach. Gemessen im Editor
  im Hauptmenü: Animation 0,28 ms pro Frame vorher, 0,065 ms nachher (es bleibt der Puls
  von „Tap to Start“). Dafür kostet der `SpinManager` 0,07 ms für 184 Spinner, die 25
  mitdrehenden Collider 0,05 ms Physik (im selben Lauf mit ausgeschalteten Collidern
  verglichen).
- **Die Reihenfolge im UI ist die Zeichenreihenfolge:** Spätere Geschwister
  liegen oben. Zwischen den Screens spielt sie seit v1.4.10 keine Rolle mehr, es ist
  immer nur einer an (siehe „UI“). Innerhalb eines Screens gilt sie weiter.
- **Code hängt kaum an der Hierarchie:** Skripte finden einander über
  Inspector-Referenzen, `FindAnyObjectByType` und den Tag `Player`, nicht über
  Pfade. Ausnahmen sind die Reihenfolge der Kinder im `SkinButtonContainer`, eins
  pro Familie (siehe „Shop“), und Clips, die über einen Pfad animieren (siehe
  „Benennung von Dateien und Objekten“).
- **Tags:** `Player` (Schiff), `Obstacle` (Crash) und `MainCamera` (Unity-Standard)
  werden gebraucht, `Coin` steht an der Münze, das Spiel liest es nicht, nur
  `ChunkTests` prüft es. `Pause`, `Shop`, `TileManager`, `Audio` und `Shootable`
  las kein Code, sie sind seit v1.4.5 entfernt, ebenso das deaktivierte
  `SkinChange` an der `ShopCamera` und ein ungenutzter `CharacterController` am
  `TileManager`.
- **Hindernisse auf der Strecke:** `PlayerMotor` crasht nur an Collidern mit Tag
  `Obstacle`. Ein fester Collider ohne das Tag hält das Schiff bloß auf, es hängt
  dann fest, und der Score läuft weiter. So war es bis v1.4.8 an `Asteroid017` in
  Chunk023, Chunk027 und Chunk029. Die Asteroiden-Prefabs (`Prefabs/Asteroids/`)
  betten das Modell als Prefab ein: `Obstacle` steht an ihrer Wurzel und als
  Override am eingebetteten Modell, das den Collider trägt. Der große Felsbogen
  `Asteroid023` (Chunk022, Chunk027) hat bewusst keinen Collider: Er lässt alle drei
  Spuren frei, das Schiff fliegt hindurch. Nach jedem neuen oder getauschten
  Hindernis (etwa im Remaster, v1.7.0) `ChunkTests` laufen lassen.
- **Tags nie bei offenem Editor aus der Mitte der Liste löschen.** In Dateien
  stehen Tags als Text, im Speicher als Nummer nach ihrer Position in der Liste.
  Als in v1.4.5 `Shootable` vorne wegfiel, rutschten `Coin` und `Obstacle` im
  laufenden Editor eine Stelle nach vorn: Asteroiden hießen „Undefined“, Münzen
  „Obstacle“, und im Play Mode tötete kein Asteroid mehr. Auf der Platte blieb
  alles richtig. Danach also alle Prefabs neu importieren (`ImportAsset` mit
  `ForceUpdate`) oder Unity neu starten, und vorher nichts speichern.
  Auch im Spieltest prüfen, ob das Schiff noch crasht.

## Benennung von Dateien und Objekten

Seit v1.4.5 heißen alle eigenen Dateien, Ordner und Objekte in Szene und Prefabs
einheitlich: **PascalCase, Englisch, ohne Leerzeichen und Sonderzeichen.**
Fremd-Pakete (`TextMesh Pro/`, `Effects/EffectExamples/`) bleiben, wie sie
geliefert wurden.

- **Nummern in Serien dreistellig** (`Asteroid001`, `Chunk000`,
  `AsteroidField007`), Varianten einstellig (`AristocratSkin2`). Schiffsfamilien
  ausgeschrieben, keine Kürzel wie ACT, FTR oder VGR. Eine Kopie im Project-Fenster
  bekommt seit v1.4.12 die nächste freie Nummer ohne Leerzeichen (Project Settings >
  Editor, „Space Before Number in Asset Names“ aus): aus `Asteroid001.prefab` wird
  `Asteroid026.prefab`, aus `Coin.prefab` `Coin1.prefab`. Bis dahin hieß sie
  `Asteroid001 1.prefab`.
- **Animationsclips** heißen `Ziel_Zustand` (`TapToStart_Pulse`),
  das ist der einzige erlaubte Unterstrich. Texturen enden auf `Color`
  (`Asteroid001Color.png`).
- **Ungenutzte Doppelgänger** einer benutzten Datei tragen `Unused` am Ende
  (`Asteroid004Unused.mat`), damit sie beim Aufräumen auffallen.
- **Szene-Objekte nach ihrer Rolle:** `…Button`, `…Text`, `…Icon`, `Title`; die
  Grafik eines Buttons heißt `Image`. Kopien-Endungen wie `(1)` kommen weg
  (`Coin`, `Asteroid012`), Namen dürfen unter Geschwistern doppelt vorkommen.

Beim Umbenennen:

- **Dateien nur über Unity verschieben** (`AssetDatabase.MoveAsset`), nie im
  Explorer: So bleiben GUID und `.meta` erhalten, und keine Referenz bricht. Unity
  importiert Modelle dabei neu, bei vielen `.blend`-Dateien dauert das Minuten.
  Die CLI bricht Aufrufe auf dem Main Thread nach 5 s ab, die Arbeit läuft aber
  weiter. Danach warten, bis `eval` wieder antwortet, und das Ergebnis selbst
  prüfen.
- **Nur Groß-/Kleinschreibung ändern** geht unter Windows in Unity nur über einen
  Zwischennamen und in Git nur per `git mv`. Sonst behält der Index den alten
  Namen, weil `core.ignorecase` an ist.
- **Texturen neben `.blend`-Quellen:** Blender verlinkt sie relativ über den
  Dateinamen. Nach dem Umbenennen die Links per Blender im Hintergrund
  nachziehen (`blender.exe -b --python …`, Blender liegt über Steam), sonst fehlen
  sie beim nächsten Öffnen. So in v1.4.5 bei 35 Dateien gemacht.
- **Objekte, die eine Animation über ihren Pfad ansteuert:** Ein Clip speichert den
  Pfad und einen Hash davon, ein Umbenennen bricht die Animation still. Mit
  `AnimationUtility` umhängen, nicht per Text. In der Szene animieren seit v1.4.10
  alle Clips ihr eigenes Objekt, bis dahin steuerte `GameMenu_Show` auch
  `UI/GameMenu/CoinIcon` an.
- **Objekte in Prefabs per Text umbenennen** (`m_Name` der GameObjects, `value:`
  unter `propertyPath: m_Name`), aus demselben Grund wie unter „Code-Stil“. Ältere
  Prefabs liegen auf der Platte noch mit CRLF, Git normalisiert das beim Commit.
  Damit ein Prefab nicht versehentlich neu geschrieben wird, speichert der Prefab
  Mode seit v1.4.12 nicht mehr von selbst (Project Settings > Editor, „Allow Auto
  Save“ aus). Beim Verlassen fragt Unity, ob gespeichert werden soll.

## Versionen, PRs & Tags

Versionsnummern wie in der README (`vMAJOR.MINOR.PATCH`). Seit Oktober 2026 gilt:
**eine Version = ein PR von `develop` nach `main`, getaggt wird immer dessen
Merge-Commit.** Auf `main` stehen damit nur fertige Versionen, und jede Version hat
genau eine Tag-Stelle. Ablauf:

1. Auf `develop` fertig machen: README-Abschnitt, Versionszeile und `bundleVersion`
   hochziehen, dann der Versions-Commit (z. B. `fix: v1.4.3 …`). Eine Version darf
   auch aus mehreren Commits bestehen, einer pro README-Punkt (so bei v1.4.3).
   Ab v1.4.5 steht die Version dann schon während der Arbeit in der README, mit
   Checkboxen und ohne Datum, und jeder Commit hakt seinen Punkt ab. Der letzte
   macht daraus den fertigen Abschnitt mit Datum und Emoji-Punkten und zieht
   Versionszeile und `bundleVersion` hoch.
2. PR `develop` → `main` mit Titel `v1.4.3 | <Titel aus der README>`, Beschreibung =
   README-Abschnitt plus „Tagged as `v1.4.3` on the merge commit of this PR.“
   Labels: `release` plus die passenden Typen (`bug`, `enhancement`, `refactoring`,
   `unity upgrade`).
3. Mergen mit **„Create a merge commit“**, nicht Squash oder Rebase - sonst entstehen
   auf `main` andere Commits als auf `develop`, und die Branches laufen auseinander.
4. Den Merge-Commit annotiert taggen (`git tag -a`), mit dem Commit-Datum als
   Tag-Datum (`GIT_COMMITTER_DATE`) und den README-Punkten als Beschreibung. Dann den
   Tag pushen, Tags gehen nicht automatisch mit.
5. GitHub-Release zum Tag anlegen: Titel wie der PR, Text = Datum · PR-Nummer und die
   README-Punkte, als „Latest“ markieren. Erst, wenn der annotierte Tag remote liegt
   (`git ls-remote origin 'refs/tags/vX.Y.Z^{}'`): Fehlt er, legt GitHub beim Release
   selbst einen einfachen Tag auf `main` an. So passiert bei v1.4.3, als der Tag-Push
   an einem „Internal Server Error“ von GitHub scheiterte; der Tag wurde danach per
   `--force-with-lease` durch den annotierten ersetzt. Solche 500er beim Push
   verschwinden meist, wenn man die Refs einzeln erneut pusht.
6. `develop` und `test` per Fast-Forward auf `main` ziehen und pushen, damit alle
   Branches auf demselben Stand sind.

Keine PRs ohne eigene Version nach `main` (wie früher #1 und #4). Liegen auf
`develop` schon Commits einer späteren Version, bekommt die ältere einen kurzlebigen
Branch `release/vX.Y.Z` auf ihrem Versions-Commit als PR-Quelle (so bei v1.4.1, #6).
Das README-Datum ist die Fertigstellung, das Tag-Datum der Merge - deshalb trägt
v1.4.0 im Tag den 05.10. statt des 10.09.

Stand (Oktober 2026 rückwirkend gesetzt, alte PR-Titel aufs Schema gebracht):
v1.3.0 auf dem Import des Stands von 2021 (`c49e85f`), mit dem das Repo beginnt;
v1.3.1, v1.3.2, v1.4.0, v1.4.1 und v1.4.2 auf den Merges der PRs #2, #3, #5, #6
und #7. Zu allen sechs Tags gibt es ein GitHub-Release, alle PRs tragen Labels, und
`main`, `develop` und `test` stehen auf demselben Commit. v1.0.0 bis v1.2.0 (2020)
liegen vor dem Repo und haben keinen Tag. Ab v1.4.3 läuft jede Version gleich nach
diesem Ablauf. Der Android-`versionCode` bleibt bis zum neuen Store-Eintrag (v2.0)
bei 8.

## Unity-Besonderheiten in diesem Repo

- **Git LFS ist aktiv** (siehe `.gitattributes`), seit September 2026 aber **nur noch
  für echte Binärdateien** (Texturen, Audio, Meshes, Libraries). Unity-YAML wie
  `.unity`, `.prefab`, `.asset`, `.mat`, `.anim`, `.controller` liegt als Text im Repo
  und ist damit diffbar - das Projekt nutzt Force Text serialization.
  Diese Formate stehen bewusst auf `merge=binary`: Git würde bei einem Merge-Konflikt
  zeilenweise mischen und dabei kaputtes YAML erzeugen. Ein Konflikt soll deshalb
  hart fehlschlagen. Neue Textformate gehören **nicht** in LFS.
- **`.meta`-Dateien immer mit committen.** Nach einem Unity-Upgrade ändern sich massenhaft
  `.meta`-Dateien (serializedVersion-Bumps) - das ist normal und gehört in einen eigenen
  Upgrade-Commit, nicht vermischt mit inhaltlichen Änderungen.
- **Blender: die `.blend` ins Repo, ihre Sicherungen nicht.** Die `.blend` ist die
  Quelle zum Weiterarbeiten und liegt in LFS. Blender legt beim Speichern den
  vorherigen Stand als `.blend1` daneben (bei mehr „Save Versions“ auch `.blend2`
  usw.) und schreibt zuerst in eine `.blend@`, die nur nach einem abgebrochenen
  Speichern liegen bleibt. Git ignoriert beides samt der `.meta`, die Unity dafür
  anlegt (seit Oktober 2026). Eine `.meta` gehört nur zusammen mit ihrer Datei ins
  Repo, sonst löscht Unity sie nach einem Clone mit einer Warnung. Die LFS-Regel
  erfasst nur `*.blend`: `NEROUS SPACEPORT.blend1` (6,8 MB) lag von September 2025
  bis Januar 2026 als normale Datei im Repo und steckt bis heute im Verlauf. Eigene
  Regeln stehen am Ende der `.gitignore` unter `### Space Escaper ###`, der Teil
  darüber ist generiert.
- **Das Spiel nutzt die `.fbx`-Exporte, nicht die `.blend`.** Von den 38 `.blend`
  ist nur `Asteroid001Animated.blend` direkt eingebunden (Stand Oktober 2026). Eine
  Änderung an den anderen kommt erst mit einem neuen FBX-Export ins Spiel. Drehungen
  kommen so an: Blender-x wird Unity-x, Blender-y wird Unity-z, Blender-z wird
  Unity -y, angewendet in Blenders Reihenfolge x, y, z (in v1.4.12 an den
  Asteroiden-Clips nachgemessen, auf 0,4° genau).
- Assets, die nicht in einer Szene oder einem Prefab referenziert sind, landen nicht im
  Build - Aufräumen in `Assets/` ist also Repo-Hygiene, keine Build-Größen-Optimierung.
- **Assembly Definitions** (seit v1.4.7): Alle Skripte liegen in der Assembly
  `SpaceEscaper` (`Scripts/SpaceEscaper.asmdef`) statt in `Assembly-CSharp`, die
  Tests in `SpaceEscaper.Tests.EditMode` (seit v1.4.11 mit `UnityEngine.UI` in den
  References, für `SceneTests`) und seit v1.4.11 `SpaceEscaper.Tests.PlayMode`. Die
  PlayMode-Assembly gilt für alle Plattformen, nur dann führt der Test Runner sie
  unter PlayMode. Das Test-Menü liegt seit v1.4.11 in `SpaceEscaper.Editor`
  (`Editor/`, nur für den Editor). `Scripts/AssemblyInfo.cs` öffnet ihm und den
  PlayMode-Tests `internal` (`InternalsVisibleTo`), derzeit für
  `SaveSystem.LoadFrom` und `SaveSystem.Delete`, den PlayMode-Tests seit v1.4.13
  auch für `FloatingOrigin.Distance`, `TileManager.Tiles` und
  `TileManager.NextSpawnZ`, seit v1.5.0 für die Buttons der Screens
  (`MainMenuScreen.StartButton` usw.). Test-Assemblies kommen an
  `Assembly-CSharp` nicht heran. Neue Skripte in `Scripts/` landen von selbst in
  `SpaceEscaper`. **Nutzt ein Skript ein weiteres Paket** (etwa TextMesh Pro in
  v1.5.1), gehört dessen Assembly in die References der `SpaceEscaper.asmdef`, sonst
  findet der Compiler es nicht. Derzeit stehen dort `Unity.InputSystem`,
  `Unity.RenderPipelines.Core.Runtime` (für `TestPanel`) und `UnityEngine.UI`. Szene
  und Prefabs finden ihre Skripte über deren GUID, der Umzug
  hat nichts gebrochen (geprüft: kein fehlendes Skript, alle 34 OnClick-Aufrufe
  finden ihre Methode).
- **VS Code** (seit v1.4.11): Das Paket „Visual Studio Editor“
  (`com.unity.ide.visualstudio`, 2.0.28) hängt VS Code an Unity. Es fehlte vom
  Unity-6-Update am 17.01.2026 bis v1.4.10, so lange kannte VS Code die
  Unity-Typen nicht.
  - Unity schreibt nach Änderungen an Skripten oder Assemblies
    `Space-Escaper.slnx` und je Assembly ein `.csproj` (`SpaceEscaper`,
    `SpaceEscaper.Editor`, `SpaceEscaper.Tests.EditMode`,
    `SpaceEscaper.Tests.PlayMode`) nach `Space-Escaper/`. Alle sind erzeugt und von
    Git ignoriert. Von Hand neu schreiben: Preferences > External Tools >
    Regenerate project files. Bis v1.4.10 lag eine alte `.slnx` im Repo, die nur
    ein `Assembly-CSharp.csproj` nannte.
  - In VS Code braucht es die Unity-Erweiterung (`visualstudiotoolsforunity.vstuc`,
    empfohlen in `.vscode/extensions.json`), sie bringt C# Dev Kit mit.
    `dotnet.preferCSharpExtension` darf nicht auf `true` stehen, das schaltet
    C# Dev Kit ab. Bis v1.4.10 stand es so in `.vscode/settings.json`.
  - VS Code im Unity-Projektordner `Space-Escaper/` öffnen, so macht es Unity beim
    Doppelklick auf ein Skript oder über Assets > Open C# Project. Nur dort gelten
    `.vscode/settings.json` und „Attach to Unity“ (F5) aus `.vscode/launch.json`.
  - Prüfen ohne VS Code: `dotnet build Space-Escaper.slnx` in `Space-Escaper/`
    (.NET-SDK 10) baut alle vier Assemblies mit den Unity-Analyzern, die Ausgabe
    landet in `Temp/`. Stand v1.4.11: keine Fehler, keine Warnungen.
  - Die Analyzer liegen im Ordner der VS-Code-Erweiterung, ihr Pfad enthält deren
    Version. Nach einem Update der Erweiterung stimmt er erst wieder, wenn Unity die
    Projektdateien neu schreibt.
- **Play Mode ohne Domain Reload** (seit v1.5.0): Project Settings > Editor, „When
  entering Play Mode“ steht auf „Reload Scene only“ (`m_EnterPlayModeOptions: 1` in
  `ProjectSettings/EditorSettings.asset`). Beim Start des Play Mode lädt Unity nur die
  Szene neu, alle Skripte nur noch nach dem Kompilieren. Gemessen in v1.5.0 im Editor:
  vom Start bis zum ersten Frame 0,07 statt 0,87 s, und die ersten gut 4 s liefen mit
  60 statt rund 19 FPS, vermutlich weil nach einem Domain Reload der Code erst neu
  übersetzt wird (JIT).
  - **Statisches überlebt dafür den Start des Play Mode** und bleibt bis zum nächsten
    Kompilieren stehen: statische Felder und Events, Handler an statischen Ereignissen
    von Unity und vom Editor, `AppDomain`-Daten und Änderungen an der Player Loop (die
    letzten beiden geprüft in v1.5.0). Unsere Klassen setzen ihre statischen Felder
    deshalb selbst zurück (siehe „Code-Stil“), sonst liefe etwa ein Handler aus dem
    letzten Play Mode weiter. Wer zum Testen die Player Loop oder Ereignisse von Unity
    ändert, stellt sie danach wieder her, wie `RunTests` in `TearDown`.
  - **Editor-Code mit `[InitializeOnLoad]`** läuft nur noch nach dem Kompilieren, nicht
    bei jedem Start des Play Mode. Was bei jedem Start passieren soll, hängt an
    `EditorApplication.playModeStateChanged`: `EnteredPlayMode` kommt im ersten Frame,
    nach den `Awake` der Szene. So setzt das Test-Menü den Haken „Invincible“, bis
    v1.4.13 tat es das im statischen Konstruktor.
  - **Fehler im Test Framework 1.8.0** (neueste Version, Oktober 2026): Ohne Domain
    Reload laufen PlayMode-Tests nur einmal pro Kompilieren, jeder weitere Lauf findet
    keinen Test und meldet 0 von 0, auch im Test-Runner-Fenster.
    `PlayerTestAssemblyProvider` hält die geladenen Test-Assemblies in einer
    statischen Liste und leert sie beim Start des Play Mode, lädt aber nur nach,
    solange es gar keine Liste gibt. `Tests/PlayMode/TestFrameworkFix.cs` verwirft die
    Liste beim Start jedes Play Mode wie ein Domain Reload und warnt in der Konsole,
    wenn es das Feld nicht mehr findet. Gefunden in v1.5.0, als schon der zweite
    `RunTests`-Lauf hintereinander 0 von 0 meldete, mit dem Umweg liefen fünf
    hintereinander grün. Behebt Unity den Fehler, kommt die Datei weg.
  - Die CLI meldet sich bei jedem Testlauf für die Ergebnisse an und bleibt ohne Domain
    Reload angemeldet. Die Konsole zeigt dann mehrfach „Run started“ und „Ignoring
    duplicate RunFinished“, das Ergebnis stimmt trotzdem.
  - Mit Domain Reload läuft der Code genauso, die Einstellung lässt sich jederzeit
    zurückstellen.
- **UI-Texte nicht über die Skalierung vergrößern.** Legacy-`Text` wird in seiner
  Schriftgröße gerastert und dann hochgezogen, das wird unscharf. Größer heißt:
  Schriftgröße und Rect-Größe erhöhen, Skalierung 1 lassen. Der Toggle „Use new
  Sound“ stand seit v1.4.0 auf Skalierung 2 mit Schriftgröße 10 und war deshalb
  matschig. In v1.4.4 wurde die Skalierung eingebacken: doppelte Maße,
  Schriftgröße 20, Sliced-Rahmen mit `Pixels Per Unit Multiplier` 0,5.
- **Bildrate: 60 FPS** seit v1.4.5. `FrameRate.cs` setzt
  `Application.targetFrameRate = 60` einmal beim Start, vor der ersten Szene
  (`RuntimeInitializeOnLoadMethod`). Ohne Ziel rendert Android fest mit 30 FPS,
  vSync ignoriert es dort. Passt 60 nicht glatt in die Bildwiederholrate des
  Displays, rundet Unity laut Doku ab, auf die nächste Rate, die glatt hineinpasst:
  90 Hz ergibt 45 FPS, 144 Hz 48 FPS. Gemessen ist bisher nur ein Display mit 60 und
  120 Hz, da passt 60 glatt (siehe „Messen auf dem Handy“). „Optimized Frame Pacing“
  bleibt aus, siehe dort. Im Editor gilt das Ziel nur fürs Game-Fenster.
- **Garbage Collection inkrementell** seit v1.4.12 (Player Settings > Other
  Settings > „Use incremental GC“, Unitys Standard für neue Projekte, hier war es
  aus). Der GC räumt dann in kleinen Schritten über mehrere Frames auf, statt das
  Spiel für einen ganzen Durchlauf anzuhalten. Bei 60 FPS hat ein Frame 16,7 ms,
  ein voller Durchlauf kann darüber liegen und ruckeln. Wie oft der GC überhaupt
  läuft, hängt daran, wie viel das Spiel pro Frame neu anlegt. Gemessen in v1.5.0
  auf dem Handy: 48 bis 134 Byte pro Frame, im Run ein Durchlauf pro Minute, keiner
  über 3 ms (siehe „Messen auf dem Handy“).
- **Auto Sync Transforms bleibt an** (Project Settings > Physics, Entscheidung
  Oktober 2026), anders als in neuen Unity-Projekten. Unity gleicht bewegte Collider
  damit vor jeder Physik-Abfrage ab, auch vor jedem `CharacterController.Move`. Aus
  brachte in v1.5.0 zu wenig für seine Nachteile:
  - Im Spiel änderte es nichts (Runs mit festem Zufall bei 30, 60 und 144 FPS Frame
    für Frame gleich, Crash und Münzen gleich) und sparte im Editor nur rund 6 µs pro
    Frame beim Abgleich.
  - Dafür übernähme der `CharacterController` eine direkt gesetzte Position erst nach
    dem nächsten Physik-Schritt (bei 120 FPS ging sie in 7 von 12 Frames verloren),
    und die Flugbahn hinge in der Rundung am Takt der Physik (bis 0,0003 Einheiten in
    15 s).
  - Der Floating Origin ruft `Physics.SyncTransforms()` trotzdem selbst auf und
    funktioniert auch mit aus. Neu prüfen, falls pro Frame viele Physik-Abfragen
    zwischen bewegten Collidern dazukommen.
  - **Physik-Einstellungen per Text ändern:** `ProjectSettings/DynamicsManager.asset`
    steht noch im alten Format (`serializedVersion: 3`, ohne die meisten heutigen
    Felder). Über das Physik-Fenster gespeichert, schriebe Unity die Datei komplett
    neu. Nach einer Änderung übernimmt Unity den Wert mit `AssetDatabase.Refresh()`,
    ohne Neustart.
- **Spiellogik an die Zeit binden, nicht an Frames:** Bewegung mit
  `Time.deltaTime`, Abläufe mit `Time.time`. Vorsicht bei Glättungen, die pro
  Frame den Anteil `k × Time.deltaTime` der Reststrecke schließen: Sie hängen
  trotzdem an der Bildrate, umso stärker, je größer `k` ist, und schießen ab
  `k × Time.deltaTime` > 1 übers Ziel hinaus. Bildratenfest ist der Anteil
  `1 - Mathf.Exp(-k * Time.deltaTime)`. So glättet seit v1.4.9 `CameraMotor`,
  bis dahin wich der Abstand der Kamera zum Schiff zwischen 30 und 60 FPS um
  1,4 % ab.
- **Tempo:** Das Schiff startet mit 13 Einheiten pro Sekunde (bis v1.4.8 mit 11)
  und wird alle 5 s um 0,2 schneller, ohne Obergrenze. Seit v1.4.9 zählt `PlayerMotor` dafür nur die
  Zeit im Flug: Menü, Pause und Todesbildschirm bringen den nächsten Schritt nicht
  näher. Bis v1.4.8 maß er an `Time.time` ab App-Start, wer länger als 5 s im Menü
  blieb, bekam den ersten Schritt schon im ersten Frame des Runs.
- **Kamera:** `CameraMotor` hält im Run den Abstand `m_offset` zum Schiff, derzeit
  14 Einheiten dahinter und 8 darüber wie bis v1.4.8 (Entscheidung Oktober 2026),
  bei jedem Tempo und jeder Bildrate gleich (gemessen bei 30 und 60 FPS: 14,00).
  Ihre Glättung (`k_FollowSharpness` = 1 pro Sekunde) ließe sie rund eine Sekunde
  Flug hinter diesem Platz herlaufen. Deshalb liest sie das Tempo bei
  `PlayerMotor.Speed` und zielt genau um diesen Nachlauf voraus. Bis v1.4.8 rechnete
  sie die Tempo-Rampe mit eigener Uhr nach. Ihr Abstand hing deshalb am Starttempo
  (bei 11 rund 14 Einheiten, bei 13 rund 16) und schrumpfte nach einem Revive, weil
  ihre Rampe auch auf dem Todesbildschirm weiterlief. `m_offset` lässt sich im
  Inspector auch im Play Mode verstellen. Beim Start des Runs schwenkt sie in rund
  3 s aus dem Menü hinter das Schiff. Nach einem Crash bremst sie bis
  `m_crashOffset` hinter dem Wrack ab, derzeit 8 Einheiten dahinter und 8 darüber
  (seit v1.4.9, Entscheidung Oktober 2026). So bleibt die Explosion unter den
  Buttons des Todesbildschirms im Bild (Sichtfeld 75°, Neigung 20°). Damit sie dabei
  nicht ruckartig langsamer wird, schließt sie die Lücke mit Tempo geteilt durch den
  Abstand der beiden Offsets pro Sekunde und fährt so mit ihrem Flugtempo los. Nach
  2 s steht sie, bei jedem Tempo. Bis v1.4.8 flog sie nach dem Tod weiter auf ihren
  Platz im Run zu und stand nach langen Runs über oder vor dem Wrack.
- **Spurwechsel:** `PlayerMotor.Move()` schließt die Lücke zur Zielspur
  exponentiell mit der geflogenen Strecke (`k_LaneChangeSharpness` = 1,25 pro
  Einheit). Ein Wechsel braucht damit bei jeder Geschwindigkeit dieselbe Strecke,
  95 % nach 2,4 Einheiten, und bei jeder Bildrate dieselbe Zeit: 0,18 s bei
  Startgeschwindigkeit, rund 0,1 s bei Geschwindigkeit 25. Das ist das Tempo des
  alten Spurwechsels bei 30 FPS auf dem Handy (Entscheidung Oktober 2026). Bis
  v1.4.5 schloss er pro Frame `Geschwindigkeit × Frame-Zeit` der Lücke, lief
  deshalb bei 60 FPS langsamer als bei 30 und schoss bei 30 FPS ab
  Geschwindigkeit 30 über die Spur hinaus.
- **Neigung beim Spurwechsel** gibt es noch nicht. Sie kommt mit dem
  Schiffs-Remaster in v1.7.0 (Entscheidung Oktober 2026), am besten im Code aus
  der Seitwärtsbewegung. Die alten Neige-Clips (20° zur Seite und zurück in
  0,5 s) liefen seit mindestens 2021 nicht, weil der Animator am `Ship` im
  Prefab `Playership` aus war. Animator, `Player.controller` und Clips sind seit
  v1.4.6 gelöscht.
- **Bewegung testen:** Am einfachsten mit „Space Escaper > Invincible“ (seit
  v1.4.11, siehe „Tests“): Das Schiff fliegt unten durch alle Hindernisse und
  sammelt Münzen, hängt an jedem aber einen Frame. Für genaue Messungen das Schiff
  im Play Mode auf y = 100 heben (`CharacterController` dafür kurz aus), dann
  trifft es nichts und fliegt normal weiter. Der erste zufällige Streckenabschnitt
  beginnt bei z = 60, seine Hindernisse reichen teils bis 57 zurück. Beim
  Starttempo ist das Schiff nach gut 4 s dort, bis dahin also heben oder die
  Collider der Hindernisse davor ausschalten. Soll es ohne den Schalter Münzen
  einsammeln, bleibt es unten, und alle festen Collider der Abschnitte (`Chunk…`
  im Root, ohne Trigger) gehen jeden Frame aus. Einen
  bestimmten Abschnitt testet man, indem man sein Prefab per `Instantiate` vor das
  Schiff setzt (etwa bei z = 100) und die übrigen festen Collider ausschaltet, so in
  v1.4.9 der Crash an `Asteroid017`.
  `Time.captureFramerate` legt die Frame-Zeit fest, egal wie schnell der
  Editor gerade läuft, und ein Handler an `Application.onBeforeRender` schreibt
  die Position pro Frame mit. So wurde der Spurwechsel in v1.4.6 bei 30, 60 und
  144 FPS gemessen. Für einen Vergleich vor und nach einem Umbau `Random.InitState`
  vor einem Szenen-Reload und noch einmal beim Start des Runs setzen, dann spawnen
  die `TileManager` dieselben Abschnitte (so in v1.4.9: alle 62 Ereignisse gleich).
  Mit fester Bildrate fliegt das Schiff dann bitgleich, Münzen zählen aber je nach
  Takt der Physik-Schritte einen Frame früher oder später (geprüft in v1.5.0).
  Eine Auto-Pause verschiebt dabei nur die Bildnummern. Ein Run im Test schreibt
  beim Tod Münzen und Highscore in den Spielstand. Ihn direkt vor jedem Lauf sichern
  und danach genau diese Sicherung zurücklegen, keine ältere: Dazwischen kann jemand
  im Editor gespielt haben (so ging in v1.4.9 ein Spielstand verloren). Seit v1.4.13
  springt z alle 4800 Einheiten zurück (siehe „Floating Origin“). Wer über längere
  Strecken misst, zählt die Verschiebungen über `FloatingOrigin.WorldShifted` mit, wie
  `RunTests`.
- **Test-APKs** baut die Unity-CLI asynchron: `unity command build --outputPath
  Builds/<Name>.apk --confirm true`, danach `build_status` abfragen, bis es
  `completed` meldet. Vorher den Play Mode beenden. Die Einstellungen kommen aus
  den Player Settings (IL2CPP für ARMv7 und ARM64, Debug-Signatur), ein Build
  dauert rund 6 Minuten. `Space-Escaper/Builds/` ist von Git ignoriert, der Ordner
  `…_BackUpThisFolder_ButDontShipItWithYourGame` neben der APK enthält nur
  Debug-Symbole. Ein Build ändert zwei URP-Assets, siehe „Render Pipeline: URP“.
  Messbuilds und wie man auf dem Handy misst, stehen unter „Messen auf dem Handy“.
- **App-Icon auf Android:** Unity 6 nimmt dafür nur die Android-Icons der Player
  Settings, nicht das Default Icon. Fehlen sie, steckt das Unity-Logo in der APK, so
  vom Wechsel auf Unity 6 bis v1.4.4. Bei `minSdkVersion` 26 gibt es nur noch adaptive
  Icons, die das Handy rund oder abgerundet eckig zuschneidet. Sichtbar ist nur die
  Mitte, 72 von 108 dp. `Branding/AppIconAdaptive.png` ist deshalb `AppIcon.png` mit
  dunklem Rand (768 statt 512 px) und steckt in beiden Ebenen, so bleibt das ganze
  Schiff sichtbar. Beide Bilder sind unkomprimiert importiert, `AppIcon` dient weiter
  als Default Icon und TV-Banner. Das graue Unity-Icon (96 px), das daneben noch in der
  APK liegt, ist nur der Ersatz für Android vor 8 und wird nie angezeigt. Ein echtes
  adaptives Icon mit getrennten Ebenen kommt mit dem Icon-Remaster (v1.7.0).
- **Grafik-API auf Android:** nur OpenGL ES 3 (Player Settings, „Auto Graphics API“
  aus). Unity 6.6 setzt dabei immer ES 3.1 voraus, der Haken „Require ES3.1“ wirkt
  nicht mehr (`PlayerSettings.openGLRequireES31` liefert fest `true`, egal was in
  `ProjectSettings.asset` steht). „Require ES3.1+AEP“ ist seit v1.4.12 aus: Das
  Android Extension Pack bringt Geometrie- und Tessellation-Shader, kein Shader im
  Projekt nutzt sie. Die Pflicht stand trotzdem im Manifest
  (`android.hardware.opengles.aep`, required), und der Play Store bietet eine App dann
  nur Geräten mit AEP an. Vulkan kommt vorerst nicht dazu: In v1.5.0 zeigte es auf
  dem Handy das Bild fehlerfrei, brachte es aber einen Frame später aufs Display
  (siehe „Messen auf dem Handy“).
- **Keine Define-Symbole** in den Player Settings (seit v1.4.12). Bis dahin standen
  dort für 19 Plattformen Überbleibsel entfernter Pakete, für Android
  `CROSS_PLATFORM_INPUT` und `MOBILE_INPUT` (Standard Assets),
  `UNITY_POST_PROCESSING_STACK_V2`, `SENTIS_ANALYTICS_ENABLED` und
  `APP_UI_EDITOR_ONLY`. Kein Code las sie, weder in `Assets/` noch in den Paketen.
  Ebenso weg ist der Eintrag `com.unity.dt.app-ui` in den Build Settings, er zeigte
  auf ein gelöschtes Asset. Ein neues Symbol nur zusammen mit Code, der es liest.

## Bekannte Altlasten (bewusst, noch offen)

- Ungenutzte Bilder, Materialien und Modelle liegen noch im Projekt, etwa die alten
  Preisschilder oder Doppelgänger mit `Unused` im Namen. Aufräumen gehört zum UI
  Overhaul (v1.5.1) und zum Remaster (v1.7.0). Bitgleiche Duplikate sind seit
  v1.4.5 gelöscht (39 in `Assets/UI/`, 4 beim Sortieren der Ordner), alle
  UI-Bilder liegen in `UI/Images/`. Von ihren 80 nutzt das Spiel 36, die übrigen 44
  liegen fast alle in `UI/Images/2020/` (siehe „Ordnerstruktur“). Gleiche Texturen
  in `Models/` und `Textures/` sind Absicht: die einen gehören zu den
  `.blend`-Quellen, die anderen zu den Unity-Materialien.
- **Play Store kommt erst mit v2.0** - als *neuer* Store-Eintrag, nicht als Update des
  alten von 2020 (Entscheidung Oktober 2026). Bis dahin bewusst offen:
  `AndroidTargetSdkVersion: 29` (Google verlangt seit 31.08.2026 API 36),
  Debug-Signatur (`androidUseCustomKeystore: 0`), App Bundle. Wegen API 29 sperrt
  Google Play Protect eine Test-APK beim Installieren erst („Unsafe app blocked“,
  „built for an older version of Android“), über „Install anyway“ geht es trotzdem.
  Der alte Paketname `com.ANIMOGames.SpaceEscaper` bleibt bei Google Play dem alten
  Eintrag zugeordnet und ist nicht wiederverwendbar - der neue Eintrag braucht einen
  neuen. Alte
  Spielstände müssen deshalb nicht migriert werden. Die Standalone-App-ID nutzt seit
  v1.4.3 ebenfalls `com.ANIMOGames.SpaceEscaper` (vorher das Tutorial-Überbleibsel
  `unity.DefaultCompany.FPS2`) und zieht mit dem neuen Paketnamen mit.
- Revive-Mechanik ist funktionslos, seit Unity Ads entfernt wurde: `RequestRevive()`
  ruft `Revive()` ohne Gegenleistung durch, einmal pro Run (`GameManager.CanRevive`,
  seit v1.5.0, bis dahin nur über den danach ausgeblendeten Button). Danach setzt der
  `TileManager` der Strecke (`Systems/TileManager`, `m_respawnsOnRevive` an) seine
  Abschnitte neu, er hört dafür auf `GameManager.ShipRevived` (seit v1.5.0, bis dahin
  ein zweiter OnClick-Aufruf am Revive-Button, der nur in der Szene stand). Das
  Ereignis kommt mit dem Tipp, also vor allen `Update()`. `RespawnTiles`
  deaktiviert die alten Abschnitte, bevor es sie löscht: `Destroy` greift erst am
  Frame-Ende, das Schiff fliegt aber im selben Frame wieder los. Ohne das krachte es
  beim Revive sofort ins selbe Hindernis (zweite Explosion, Spiel-UI weg, Schiff
  unsichtbar; behoben in v1.4.4). Seit v1.4.11 prüft `RunTests` das bei jedem Lauf.
- Highscore-Leaderboard entfiel mit Google Play Games; es gibt nur noch den lokalen
  Highscore im Spielstand. Der Pokal-Button im Hauptmenü (`UI/MainMenu/LeaderboardButton`) bleibt
  trotzdem **bewusst sichtbar**, auch ohne Funktion (Entscheidung Oktober 2026) -
  nicht ausblenden oder löschen. Sein OnClick-Aufruf zeigte auf eine nicht mehr
  existierende Methode und ist entfernt, auch der `MainMenuScreen` verbindet ihn mit
  nichts. Der Button spielt nur den Klicksound.
- **Angeschnittene Texte auf schmalen Displays** (gesehen in v1.5.0 auf dem Pixel 9
  Pro, 20:9): In den Settings zeigt „Use new Sound“ nur „Use new“, die Titel
  „SETTINGS“ und „PAUSED“ sind links angeschnitten. Vermutlich, weil der Canvas nach
  der Höhe skaliert (siehe „UI“): Auf 20:9 bleiben rund 360 Einheiten Breite, auf
  16:9 sind es 450. Gehört zum UI Overhaul (v1.5.1).

## Spielstand (SaveData)

Seit v1.4.7 steht der ganze Spielstand in einer Klasse, `SaveData`: Münzen,
Highscore, geflogenes Schiff, gekaufte Schiffe und die Audio-Einstellungen
(Entscheidung Oktober 2026). `SaveSystem` lädt ihn einmal beim Start, vor der ersten
Szene, und schreibt ihn als `SaveData.json` nach `Application.persistentDataPath`.
Alle Skripte lesen und ändern ihn über `SaveSystem.Data`, PlayerPrefs nutzt das
Spiel nicht mehr.

- **Gespeichert wird sofort** bei Tod, Kauf, Schiffswahl und den Audio-Schaltern,
  dazu beim Pausieren und Beenden der App, für den Lautstärkeregler (siehe
  „Audio-System“). Bis dahin stehen Änderungen nur im Speicher.
- **Versionsnummer:** Jede Datei trägt `m_version`, derzeit 2 (seit v1.4.8, Schiffe
  als ID statt als Nummer). Einen Stand mit anderer Version verwirft
  `SaveData.FromJson`, das Spiel fängt dann neu an (Entscheidung Oktober 2026, es
  gibt nur Test-Spielstände). Ab dem Store-Release (v2.0) ältere Stände stattdessen
  dort umwandeln. Fehlt ein Feld in der Datei, behält es beim Laden seinen
  Standardwert, ein neues Feld braucht also keine neue Version.
- **Feldnamen sind die Schlüssel in der Datei** (`m_coins`, `m_currentShipId` …). Ein
  umbenanntes Feld verliert seinen Wert in bestehenden Spielständen, außer es
  bekommt `[FormerlySerializedAs]`.
- **Kein halber Spielstand:** `SaveSystem.Save` schreibt erst `SaveData.json.tmp`,
  löscht dann die alte Datei und benennt die neue um. Wird die App mitten im
  Schreiben beendet, bleibt der alte Stand ganz, und liegt beim Start nur die
  `.tmp`-Datei da, lädt das Spiel sie. Eine unlesbare Datei ergibt eine Warnung in
  der Konsole und einen neuen Spielstand, der sie beim nächsten Speichern
  überschreibt.
- **Die Familie wird nicht gespeichert,** sie steht im `ShipData` des Schiffs. Bis
  v1.4.6 lag sie unter einem eigenen Schlüssel und konnte vom Schiff abweichen.
- **Alte Spielstände** aus den PlayerPrefs (bis v1.4.6) übernimmt das Spiel nicht,
  es gab nur Test-Spielstände (Entscheidung Oktober 2026). Die alten Schlüssel
  liegen ungenutzt weiter in der Registry und auf den Testhandys.

Mit v1.4.1 (Oktober 2026) wurden im Spielstand mehrere Bugs behoben. Seit v1.4.7
stecken die Regeln dahinter nicht mehr im `GameManager`, sondern in `SaveData` und
`RunCoins`, wo Tests sie ohne Szene prüfen können:

- Gekaufte Schiffe stehen als Liste von Schiffs-IDs in `m_unlockedShipIds` (bis
  v1.4.7 Nummern, bis v1.4.6 eine Bitmaske). Bis Januar 2026 lagen sie nur im
  Google-Play-Games-Cloud-Save und fielen mit GPG ersatzlos weg - gekaufte Schiffe
  waren danach nach jedem Szenen-Reload wieder gesperrt. Beim Laden gelten
  Startschiff und aktuelles Schiff immer als freigeschaltet und kommen sofort in die
  Liste (`SaveData.RepairShips`) - sonst wäre ein nur implizit besessenes Schiff nach
  einem Wechsel wieder gesperrt.
- Ein neuer Spielstand bekommt das Startschiff (`ShipCatalog.StarterShip`). Ist das
  gespeicherte Schiff unbekannt, fällt `SaveData.RepairShips` darauf zurück, die
  bekannten IDs übergibt der `PlayerShip` beim Laden der Szene aus dem `ShipCatalog`. Bis
  v1.4.0 war der Default 0, das zeigte auf `FlameContainer` statt auf ein Schiff und
  ließ das Spiel beim Start und beim Tod abstürzen. Lokal fiel das nie auf, weil der
  Spielstand gesetzt war - **Neuinstallation gezielt testen** (siehe unten).
- Ein Kauf (`SaveData.TryBuyShip`) zieht die Münzen ab und schaltet das Schiff frei,
  beides oder nichts. Danach speichert der `ShopScreen` sofort.
- Der Score wird überall abgeschnitten angezeigt und gespeichert (`RunScore.Score`),
  sonst weichen Todesbildschirm und Highscore voneinander ab.
- Münzen werden bei jedem Tod gutgeschrieben, nach einem Revive aber nur die seitdem
  gesammelten (`RunCoins.BankInto`).
- **Schiffspreise** (Oktober 2026 neu balanciert, vorher Testwerte von 1 Münze):
  ARISTOCRAT-Skins **250**, FREETER **750**, VAGOR **1.250** - ein Preis pro Familie
  wie im Original. Grundlage: Ø 5,9 Münzen pro Abschnitt (60 Einheiten), Strecke
  nach T Sekunden = 11·T + 0,02·T², davon 60 % eingesammelt → ein Ø-Run von 60 s
  bringt ~40 Münzen. Ziel: erstes Skin in der ersten Session, erster FREETER nach
  ~45 min, erster VAGOR nach ~2 h, alles nach ~3,3 h. Die Originalpreise von 2021
  (3.500 / 6.000 / 8.000) hätten über 20 h gebraucht. Der derzeit kostenlose
  Revive hebt das Einkommen pro Run grob um 50-70 % - nach dessen Umbau (v1.9.2)
  die Preise gegenprüfen. Seit v1.4.9 startet das Schiff mit 13 statt 11: Ein
  gleich langer Run fliegt rund 15 % weiter und bringt entsprechend mehr Münzen,
  60 s also ~50 statt ~40. Ob Runs dadurch kürzer werden, zeigt das Spielen.
- **Merkposten: Der Spielstand ist unverschlüsselt** (früher ein TODO im
  `GameManager`, in v1.4.3 hierher verschoben). Die Datei ist lesbares JSON und
  lässt sich mit Zugriff auf den Datenordner der App ändern. Ohne Echtgeld-Käufe und
  ohne Online-Rangliste betrifft das nur den Spieler selbst. Kommt eins davon (z. B.
  ein Leaderboard über Play Games in v2.0), neu bewerten: Verschlüsselung in der App
  hält nur Gelegenheits-Schummler ab, weil der Schlüssel mit ausgeliefert wird.
- **Wo der Spielstand liegt:** auf Android im Datenordner der App, „Speicher
  löschen“ in den App-Einstellungen setzt ihn zurück. Im Editor unter
  `%USERPROFILE%\AppData\LocalLow\ANIMO Games\Space Escaper\SaveData.json`. Der Pfad
  hängt an Firma und App-Name (`productName`), ein neuer App-Name lässt den
  Spielstand im Editor deshalb verschwinden. Der PlayMode-Test schreibt in einen
  eigenen Ordner, siehe „Tests“.
- **Neuinstallation im Editor testen:** „Space Escaper > Reset Save (New Install)“
  (seit v1.4.11), oder von Hand: Play Mode beenden und `SaveData.json` samt einer
  eventuellen `SaveData.json.tmp` wegschieben. Der nächste Start beginnt mit 0
  Münzen, Highscore 0, dem Startschiff, den neuen Sounds, Musik und SFX an und voller
  Lautstärke.
- **Die alten PlayerPrefs** liegen in der Registry unter
  `HKCU\Software\Unity\UnityEditor\ANIMO Games\Space Escaper`, daneben noch der
  Schlüssel des App-Namens bis v1.4.4, `Space-Escaper`.

## Shop

Seit v1.4.8 beschreibt ein `ShipData`-Asset pro Schiff, was der Shop wissen muss:
Familie, Preis, Preisschild, Modell und Antriebsflamme. Die Assets liegen in
`Data/Ships/`, der `ShipCatalog` daneben listet alle Schiffe in der Reihenfolge des
Hangars: ARISTOCRAT, FREETER und VAGOR (`ShipFamily`) mit je drei Skins, das
Startschiff vorn. Bis v1.4.7 hing der Shop an Kind-Indizes, mit 9 Kauf-Buttons, 11
Bildern, einer Preisliste im `GameManager` und jedem Modell zweimal in Szene und
Prefab.

- **Der Hangar liest die Daten:** Jeder Skin-Button trägt einen `ShipButton` mit
  seinem `ShipData` und meldet es beim Tipp, der `ShopScreen` zeigt dann dieses Schiff.
  Die Pfeile zeigen das erste Schiff der
  Familie davor oder danach (`ShipCatalog.GetFirstShipOfFamily`). Ein einziger
  unsichtbarer Button, `UI/Shop/SelectOrBuyButton`, kauft oder wählt das gezeigte
  Schiff (`SelectOrBuyShownShip`). Sein Kind `Image` zeigt Select, Selected oder das
  Preisschild aus dem `ShipData` und lässt Klicks zum Button durch.
- **Modelle sind Prefabs** in `Prefabs/Ships/` (`AristocratSkin1.prefab` …), mit
  ihrem Platz auf dem Schiff: Versatz und Skalierung stecken in der Wurzel. Der
  `PlayerShip` setzt das Modell des geflogenen Schiffs unter `Playership/Ship` ein, der
  `Hangar` das des gezeigten unter `Environment/ShopShips`, dort in der Mitte und auf
  `ShipData.HangarScale`, beim ersten Öffnen des Hangars. Beim Tod schaltet der
  `PlayerShip` das Modell aus, beim Revive wieder an.
  Die VAGOR-Skins haben eine etwas andere Skalierung als ihre Familie (Skin 1 2,6 ×
  3,2 × 3,2, Skin 2 und 3 2,7 × 3 × 3), so übernommen aus dem Stand vor v1.4.8.
- **IDs:** Jedes `ShipData` hat eine ID (`AristocratSkin1` …), unter der der
  Spielstand das Schiff speichert. Eine geänderte ID verliert das Schiff in
  bestehenden Spielständen. Das Startschiff legt `ShipCatalog.StarterShip` fest, die
  Reihenfolge im Katalog ist nur die des Hangars.
- **Ein neues Schiff:** Modell-Prefab in `Prefabs/Ships/` anlegen, ein `ShipData` in
  `Data/Ships/` mit ID, Familie, Preis, Preisschild-Bild, Modell, Hangar-Größe und
  Flamme, das `ShipData` in den `ShipCatalog` eintragen und einen Skin-Button mit
  `ShipButton` und dem `ShipData` in die Gruppe der Familie legen, am einfachsten einen
  vorhandenen duplizieren. Der `ShopScreen` findet ihn beim ersten Öffnen des Hangars
  von selbst. Die `ShipCatalogTests` prüfen danach die Daten.
- `m_skinButtonContainer` hat ein Kind pro Familie, in der Reihenfolge von
  `ShipFamily`.
- `PlayerShip.Ship` ist das *geflogene* Schiff, `m_shownShip` im `ShopScreen` das
  gerade im Hangar *angesehene*. Flammen und alles im Spiel gehören zum geflogenen -
  die Verwechslung zeigte früher eine falsche, neben dem Schiff schwebende Flamme.

**Antriebsflammen** sind Prefabs in `Prefabs/Ships/`: `FlameAristocrat`,
`FlameFreeter` und `FlameVagor` mit den Düsen `FlameLeft` und `FlameRight`. Jedes
`ShipData` verweist auf die Flamme seiner Familie, die Skins teilen sie sich. Der
`PlayerShip` setzt sie neben das Modell unter `Playership/Ship` und schaltet sie
für jedes Schiff gleich, nur über das GameObject: aus im Menü und im Hangar, an vom
Start des Runs bis zum Crash und wieder nach einem Revive. Die Emission bleibt
immer an. Bis v1.4.7 lagen die Flammen im `Playership.prefab`, und
`PlayerMotor.m_engineFlame` schaltete beim ARISTOCRAT zusätzlich die Emission. Ein
versehentlicher Emission-Override in der Szene ließ deshalb den FREETER von v1.4.0
bis v1.4.2 ohne sichtbaren Antrieb fliegen, während er beim ARISTOCRAT nicht
auffiel. Weil die Flammen jetzt erst zur Laufzeit entstehen, kann die Szene sie
nicht mehr überschreiben.

**Preisschilder sind Bilder, keine Texte.** Den Preis im `ShipData` allein zu ändern
reicht nicht - der Shop zeigt ihn aus `Assets/UI/Images/2026/`:
`PriceTagAristocrat250.png`, `PriceTagFreeter750.png` und `PriceTagVagor1250.png`.
Aufbau wie bei den Originalen von v1.3 (`2021/PriceTagAristocrat3500.png`,
`PriceTagFreeter6000.png`, `PriceTagVagor8000.png`, pixelgleich nachgeprüft):
`2021/ButtonBlank.png` als Platte, `2020/CoinIcon.png` als Icon, Zahl in
`UI/Fonts/NeuropolXRegular.ttf` mit 49 px Ziffernhöhe ab y = 71, Icon und Zahl als
Gruppe mittig. Die Original-Schilder mit den alten Preisen liegen unverändert in
`2021/` und sind in der Szene nicht mehr referenziert. Sie bleiben bewusst liegen,
entschieden wird beim UI Overhaul (v1.5.1) - bis dahin nicht löschen.

## Spielzustand (GameState)

Nicht zu verwechseln mit dem Spielstand (`SaveData`): Seit v1.5.0 hat das Spiel einen
Zustand, `GameManager.State`, mit den Werten `Menu`, `Hangar`, `Run`, `Paused` und `Dead`
(`GameState`). Bis v1.4.13 verteilte er sich auf vier Schalter (`m_isGameStarted` im
`GameManager`, `PlayerMotor.IsRunning`, `CameraMotor.IsMoving` und
`PauseMenu.IsPaused`). Weil der erste davon nach dem Tod an blieb, zählte der Score
hinter dem Todesbildschirm weiter, bis ein Revive ihn zurücksetzte.

- **Nur der `GameManager` ändert ihn,** in den Methoden, die die Buttons aufrufen:
  `Play` (Menu → Run), `OpenHangar` und `CloseHangar` (Menu ↔ Hangar), `Pause` und
  `Continue` (Run ↔ Paused), `HandleDeath` beim Crash, gemeldet vom `PlayerMotor`
  (Run → Dead), und `RequestRevive` (Dead → Run, einmal pro Run, siehe `CanRevive`).
  Passt ein Aufruf nicht zum Zustand, etwa eine Pause außerhalb des Runs, tut er
  nichts. `ReturnToMenu` lädt die Szene neu, der Zustand beginnt dann wieder bei
  `Menu`. Mehr macht der `GameManager` seit dem Aufteilen in v1.5.0 nicht. Bis dahin
  führte er auch den Shop, den Score und die Texte der Screens.
- **Jeder Wechsel** zeigt den Screen des Zustands (`ScreenManager.Show(GameState)`) und
  setzt `Time.timeScale`, 0 nur in der Pause.
- **Die Settings sind kein Zustand,** sondern ein Screen über Hauptmenü oder Pause. Der
  Zustand bleibt dabei stehen, und „Zurück“ zeigt wieder seinen Screen.
- **Ereignisse,** statisch am `GameManager` wie `FloatingOrigin.WorldShifted`:
  `RunStarted`, `HangarOpened`, `HangarClosed`, `RunPaused`, `RunResumed`, `ShipCrashed`
  und `ShipRevived`. Sie kommen direkt nach dem Wechsel im selben Aufruf. Ein Tipp
  erreicht die Buttons vor allen `Update()`, beim Revive ist die Strecke also neu,
  bevor das Schiff in diesem Frame weiterfliegt. So braucht auch das `AudioSystem`, das
  den Reload überlebt, keine Referenz in die Szene.

| Wer | hört auf | und tut |
|---|---|---|
| `AudioSystem` | `RunStarted` | Spielmusik und Motor an |
| | `RunPaused`, `RunResumed` | Motor aus und wieder an, die Musik läuft weiter (siehe „Pause“) |
| | `ShipCrashed` | Musik anhalten, Motor aus (siehe „Audio-System“) |
| | `ShipRevived` | Musik an derselben Stelle weiter, Motor an |
| `RunScore` | `ShipCrashed` | Münzen des Runs gutschreiben, Highscore, speichern |
| `PlayerShip` | `RunStarted` | Flamme an |
| | `ShipCrashed`, `ShipRevived` | Modell und Flamme aus und wieder an |
| `Spaceport` | `RunStarted` | schaltet sich 3 s später aus |
| `Hangar` | `HangarOpened`, `HangarClosed` | Hangar mit Hangar-Kamera oder Hauptkamera (bis v1.4.13 `CameraSwitch`, das Gebäude schaltete der `GameManager`) |
| `TileManager` | `ShipRevived` | neue Abschnitte, nur mit `m_respawnsOnRevive` (die Strecke) |

- **Den Zustand lesen** `PlayerMotor` (fliegt nur in `Run`), `CameraMotor` (folgt in
  `Run`, bremst in `Dead`, steht sonst), `RunScore` (zählt nur in `Run`), der
  Zurück-Button im `SettingsScreen` und `RunTests`. Die übrigen Screens brauchen ihn
  nicht: Der `ScreenManager` schaltet sie ein, und sie zeigen beim Einschalten, was
  gerade gilt (siehe „UI“).
- **Münzen** melden sich über das statische Ereignis `Coin.Collected` (seit v1.5.0,
  bis dahin `GameManager.CollectCoin`), `RunScore` zählt sie. Den Multiplikator rechnet
  `RunScore` aus dem Tempo (`PlayerMotor.SpeedGain`), bis v1.4.13 schickte der
  `PlayerMotor` ihn bei jeder Tempo-Stufe.
- **Neues, das auf den Spielverlauf reagiert,** meldet sich an einem Ereignis an (in
  `OnEnable`, ab in `OnDisable`) oder liest `State`, statt einen eigenen Schalter zu
  führen. Ein neuer Übergang ist eine Methode im `GameManager`: Zustand prüfen,
  `ChangeState`, dann das Ereignis auslösen.
- **Geprüft in v1.5.0** mit einem Klick-Durchlauf über die echten Buttons, vorher und
  nachher, mit festem Zufall und fester Bildrate: alle Übergänge, „Exit“ aus Pause und
  Tod und die Auto-Pause in jedem Zustand, 32 Schritte. Screens, Kameras,
  `Time.timeScale`, Musik, Motor, Hangar, Modell, Flamme, Revive und Crash-Stelle
  blieben gleich. Anders sind nur der Score nach dem Tod (der Bug) und der Frame des
  Pause-Tipps (siehe „Pause“).
- **Das Aufteilen** prüfte ein erweiterter Durchlauf mit eigenem Spielstand (1000
  Münzen, nur das Startschiff), 57 Schritte, auch durch den Shop (Familien, Skins,
  Kaufen, zu teuer, Auswählen) und mit Münzen, nach jedem Schritt mit allen sichtbaren
  Texten, Bildern, Modellen und dem Spielstand. Die Buttons findet er über ihren Pfad,
  das ging mit altem und neuem Code. Anders sind nur zwei Stellen: Das Modell auf dem
  Ständer entsteht erst beim ersten Öffnen des Hangars statt beim Laden, und eine
  Münze, die erst nach dem Crash gemeldet wird, behält nach dem Revive ihren Punkt
  (der Durchlauf hat das erzwungen, ob es beim Spielen vorkommt, ist nicht geprüft).
  Bis dahin nahm der Revive den Punkt zurück, der Score-Text zeigte ihn aber weiter.

## UI

Seit v1.4.10 ist jeder Screen ein eigener Canvas unter `UI`: `MainMenu`, `GameMenu`
(die Anzeige im Run), `PauseMenu`, `Settings`, `Shop` und `DeathMenu`. Der Root-Canvas
`UI` trägt den `CanvasScaler` (Referenzhöhe 800, skaliert nach der Höhe) und den
`ScreenManager`. Bis v1.4.9 lagen alle Screens auf einem Canvas, und sechs Animatoren
schoben die gerade nicht gebrauchten aus dem Bild.

- **Es ist immer genau ein Screen an.** `ScreenManager.Show(GameState)` zeigt den
  Screen des Spielzustands (siehe „Spielzustand“), `ShowSettings()` die Settings.
  Beide schalten ihn per `SetActive` ein und alle anderen aus, ohne
  Übergang (Entscheidung Oktober 2026). Beim Start zeigt er das Hauptmenü, egal welcher
  Screen beim Bearbeiten an blieb. Gespeichert ist die Szene mit nur dem Hauptmenü an,
  so sieht sie im Editor aus wie beim Start. Zum Bearbeiten einen Screen einschalten,
  vor dem Speichern wieder nur das Hauptmenü. Das prüft seit v1.5.0 `SceneTests`: Ein
  anderer Screen wachte sonst mit der Szene auf, vielleicht bevor bereit ist, was er
  zeigt, etwa der `ShopScreen` vor dem `PlayerShip`, und erst danach schaltete der
  `ScreenManager` ihn aus.
- **Jeder Screen hat eine Komponente mit seinem Namen und `Screen` dahinter** (seit
  v1.5.0), etwa `MainMenuScreen` am `UI/MainMenu`. Sie verbindet die Buttons ihres
  Screens in `OnEnable` und löst sie in `OnDisable`, eine Zeile pro Button
  (`m_startButton.onClick.AddListener(m_gameManager.Play)`). Was ein Button tut, steht
  also in der Komponente seines Screens, im Inspector des Buttons steht nichts mehr.
  Die Screens erreichen `GameManager`, `ScreenManager` und den Rest über Referenzen im
  Inspector, wie die meisten Skripte hier. Die Skin-Buttons im Hangar tragen dazu je
  einen `ShipButton` mit ihrem Schiff (siehe „Shop“). Ein neuer Button bekommt ein
  Feld in der Komponente seines Screens und je eine Zeile in `OnEnable` und
  `OnDisable`.
- **Die Komponente zeigt auch, was auf ihrem Screen steht** (seit v1.5.0, bis dahin
  setzte der `GameManager` alle Texte): Hauptmenü (Münzen, Highscore) und
  Todesbildschirm (Score, Münzen, Revive-Button nur, solange der Run ihn noch hat)
  setzen ihre Texte beim Einschalten, aus dem Spielstand und dem `RunScore`. Die
  Spiel-UI folgt dem `RunScore` in `LateUpdate`, also mit dem Score dieses Frames, und
  setzt einen Text nur, wenn sich seine Zahl ändert. Steigen die Münzen, poppt das
  Icon. Der `ShopScreen` führt den Shop selbst (siehe „Shop“).
- **Ein Canvas pro Screen,** damit eine Änderung nur ihren eigenen Screen neu baut. Im
  Run baut der Canvas nur noch neu, wenn sich Score oder Münzen ändern (gemessen in 2
  von 30 Frames), in Pause, Settings, Tod und Hangar gar nicht. Bis v1.4.9 baute der
  ganze UI-Canvas in jedem Frame neu, außer in der Pause. Die Screen-Canvases erben
  Render Mode und Skalierung vom Root. Ihr eigener Render Mode steht trotzdem auf
  Overlay, falls ein Screen je aus `UI` herauswandert.
- **Jeder Screen braucht seinen eigenen `GraphicRaycaster`.** Ein Raycaster trifft nur
  die Grafiken seines Canvas, ohne ihn reagiert kein Button des Screens. Der Root hat
  seit v1.4.10 keinen mehr, direkt unter ihm liegt nichts Klickbares.
- **Ein neuer Screen:** Objekt unter `UI`, auf den ganzen Canvas gestreckt (Anker 0
  bis 1, Abstände 0), mit `Canvas`, `GraphicRaycaster` und seiner eigenen
  `…Screen`-Komponente für die Buttons. Im `ScreenManager` bekommt
  er ein Feld und einen Platz in `m_screens`, dazu einen Fall in `GetScreen`, wenn er
  zu einem Zustand gehört, sonst eine eigene `Show…`-Methode wie `ShowSettings`. Dass
  Canvas, Raycaster und `m_screens` stimmen, prüft `SceneTests`.
- **Keine Animatoren für Übergänge oder kurze Effekte.** Ein Animator am UI markiert
  seine Grafiken in jedem Frame neu, auch wenn sich nichts bewegt, und dann baut der
  ganze Canvas neu. In einem Screen fällt er außerdem bei jedem Einschalten auf seinen
  Startzustand zurück. Er lohnt sich nur für Elemente, die sich ständig bewegen, wie
  den Puls von „Tap to Start“ im Hauptmenü. Kurze Effekte laufen im Code, so der
  Münz-Pop: `PopEffect` am `GameMenu/CoinIcon` vergrößert es für 0,25 s auf 115 % und
  färbt es gelb. Bis v1.4.9 lief er über einen Animator, der das Icon nach der ersten
  Münze von 55 auf 65 wachsen ließ. Seit v1.4.10 ist 65 die Ruhegröße.
- **Unsichtbar heißt nicht kostenlos.** Eine Grafik mit Alpha 0 wird gezeichnet, solange
  „Cull Transparent Mesh“ an ihrem `CanvasRenderer` aus ist, auf dem Handy kostet das
  Füllrate. Seit v1.4.10 sind die unsichtbaren Vollbild-Hintergründe von Hauptmenü,
  Spiel-UI und Hangar weg, und die unsichtbaren Klickflächen der Buttons (`UISprite`
  mit Alpha 0) haben „Cull Transparent Mesh“ an. Klicks nehmen sie trotzdem an
  (geprüft per Raycast, die Tiefe bleibt erhalten). Neue unsichtbare Klickflächen
  genauso einstellen.
- **UI-Umbauten prüfen:** Im Play Mode alle Zustände über die echten Buttons
  durchklicken und pro Zustand die sichtbaren Grafiken auflisten, mit Rechteck auf dem
  Bildschirm, Farbe, Bild und Text, vorher und nachher. Ob ein Klick ankommt, zeigt
  `EventSystem.RaycastAll` in der Mitte jedes Buttons, `onClick.Invoke()` umgeht das
  Raycasting. Neu gebaute Canvases zählt ein `ProfilerRecorder` auf `Canvas.BuildBatch`.
  So in v1.4.10: 16 Zustände gleich, alle 53 Klicks kommen an.

## Eingabe (Input System)

Seit Oktober 2026 (v1.4.2) läuft alle Eingabe über das **Input System Package**
(`com.unity.inputsystem` 1.20.1). Active Input Handling steht auf „Input System
Package (New)" (`activeInputHandler: 1`), der alte Input Manager ist aus. Diese
Einstellung zu ändern erfordert einen Editor-Neustart.

- **Kein `UnityEngine.Input` mehr verwenden** (`Input.GetMouseButton`, `Input.touches`,
  `Input.GetKey` …). Das kompiliert weiterhin, wirft zur Laufzeit aber eine
  `InvalidOperationException`. Neuer Code liest Geräte über `UnityEngine.InputSystem`.
- `MobileInput` liest Touch und Maus über **einen** Codepfad: `Pointer.current` ist
  auf dem Handy der Touchscreen (erster Finger), im Editor die Maus. Die Wisch-Logik:
  6 mm Deadzone, ein Wischer pro Berührung, ausgelöst schon während des Ziehens,
  spätestens beim Loslassen. Bis v1.4.5 ging ein kurzer, schneller Wischer
  verloren, der die Deadzone erst mit seiner letzten Bewegung verließ. Das Messen
  beim Loslassen klappt auch auf dem Handy: Der Touchscreen behält dann die
  letzte Position, das Input System setzt nur Delta und Tap-Zähler zurück.
  Die Richtung entscheidet die längere Achse, nur ein überwiegend waagrechter
  Wischer wechselt also die Spur. Bis v1.4.5 zählte allein das Vorzeichen von x,
  da reichte ein Wisch nach oben mit etwas Seitendrift.
- **Deadzone in Millimetern** (seit v1.4.6, vorher feste 100 px, je nach Display
  etwa 5 bis 9 mm Wischweg): `MobileInput` rechnet die 6 mm in `Awake` über
  `Screen.dpi` in Pixel um. Meldet ein Gerät keine dpi (`Screen.dpi` ist 0),
  nimmt es 420 dpi an, ein typisches Handy, das ergibt knapp 100 px. Im Editor
  gilt die dpi des Monitors, hier 168, also 40 px. Ob sich 6 mm gut anfühlen,
  vor v2.0 auf dem Handy prüfen.
- **Für v1.9.0 vorbereitet:** `MobileInput` meldet auch senkrechte Wischer
  (`HasSwipedUp`, `HasSwipedDown`) und Doppeltipps (`HasDoubleTapped`), nur liest
  sie noch nichts im Spiel. Ein Doppeltipp sind zwei Berührungen, die höchstens
  0,3 s nacheinander aufsetzen, die erste ohne Wischer losgelassen. Er zählt beim
  Aufsetzen der zweiten, und die startet keinen weiteren (wie bei Android).
- **Tasten zum Testen im Play Mode:** Die Pfeiltasten wirken wie Wischer, ←/→
  wechseln also die Spur. Zweimal Leertaste ist ein Doppeltipp, die Leertaste
  ist kein UI-Submit (das ist nur Enter). Gamepad-Steuerung und A/D gibt es
  bewusst nicht (Entscheidung Oktober 2026). Im Editor kommen Tastatur und Maus
  nur an, wenn das Game-Fenster den Fokus hat (Standardverhalten des Input
  Systems) - vorher einmal ins Game-Fenster klicken.
- Das UI läuft über `InputSystemUIInputModule` am EventSystem, mit den
  Standardaktionen aus dem Paket (`DefaultInputActions`), außer „Cancel“ (Escape):
  Die ist seit v1.4.5 abgehängt, siehe „Pause“. Das alte
  `StandaloneInputModule` erkennt mit dem neuen Backend keinen Klick mehr - nicht
  zurücktauschen.
- Eingaben lassen sich im Play Mode per `InputSystem.QueueStateEvent` simulieren
  (so wurde die Umstellung getestet: UI-Klick, Pfeiltasten, Maus-Wischer). Ohne
  Fokus aufs Game-Fenster dafür vorübergehend `InputSystem.settings`
  umstellen (`editorInputBehaviorInPlayMode = AllDeviceInputAlwaysGoesToGameView`,
  `backgroundBehavior = IgnoreFocus`) und danach zurücksetzen. Die Settings sind
  kein Asset, sie leben nur im Speicher. Solange sie umgestellt sind, landet auch
  jeder echte Klick im Editor im Spiel. Touch geht über einen virtuellen
  Touchscreen (`InputSystem.AddDevice<Touchscreen>()`, Ereignisse als
  `TouchState`), den man danach wieder entfernt. Abläufe über mehrere Frames
  (Wischer, Doppeltipp) spielt ein Handler an `Application.onBeforeRender` ab,
  ein Schritt pro Frame.
- **Vorsicht, Auto-Pause beim Simulieren:** Ein Fokuswechsel im Editor pausiert
  den Run, und mitten auf dem Bildschirm liegen dann Settings- und
  Continue-Button. Weitere simulierte Tipps in der Mitte klicken sich durch die
  Menüs. In v1.4.6 verstellte ein Test so Lautstärke und „Use new Sound“. Im Test
  bei einer Pause die restlichen Schritte verwerfen und den Spielstand vorher und
  nachher vergleichen.
- **Klicks für Tests so simulieren, nicht per `onClick.Invoke()` von außen.** Ein
  echter Klick läuft im EventSystem vor den `Update()`-Methoden der Spielskripte,
  im selben Frame (Execution Order -1000, eingetragen in der `.meta` von
  `EventSystem.cs` im Paket `com.unity.ugui`). Der Revive-Bug (v1.4.4) trat nur so
  auf - per `onClick.Invoke()` aus dem Editor sah alles gut aus. Kommen echte
  Eingaben nicht an, weil Unity im Hintergrund läuft, tut es ein eigenes System im
  `PlayerLoop` unter `PreUpdate`, das `ExecuteEvents.Execute` mit
  `pointerClickHandler` auf dem Button aufruft: Es läuft wie ein echter Klick vor
  allen `Update()`-Methoden. So wurde in v1.4.9 der Revive getestet, seit v1.4.11
  tippt `RunTests` genauso.

## Pause

Seit v1.5.0 ist die Pause ein Zustand des Spiels, `GameState.Paused` (siehe
„Spielzustand“). `GameManager.Pause()` wechselt aus dem Run hinein, `Continue()` zurück.
Der Zustand gehört zum `GameManager` und beginnt nach dem Szenen-Reload hinter „Exit“
wieder im Hauptmenü. Von v1.4.5 bis v1.4.13 war die Pause ein eigenes statisches Flag,
`PauseMenu.IsPaused`. Bis v1.4.4 gab es zwei Flags, und eines davon blieb nach „Exit“
aus der Pause auf `true` stehen.

- `Pause()` und `Continue()` sind von überall gefahrlos aufrufbar, nicht nur von
  den Buttons. `Pause()` greift nur im Run (also nicht im Menü, im Hangar oder auf
  dem Todesbildschirm) und nur einmal, `Continue()` nur aus der Pause. Darauf baut
  die Auto-Pause auf.
- **Die Pause hält alles im Frame des Tipps an** (seit v1.5.0): Schiff, Kamera und
  Score bewegen sich in diesem Frame nicht mehr. Bis v1.4.13 liefen sie noch einen
  Frame weiter, weil Unity die Frame-Zeit schon vor dem Tipp berechnet hatte, erst
  der nächste Frame stand still. Im Frame von „Continue“ bewegt sich weiterhin
  nichts, seine Frame-Zeit entstand noch in der Pause. Wischer in der Pause wirken
  nicht, der `PlayerMotor` liest sie nur im Run.
- **Auto-Pause** (seit v1.4.5): Der `GameManager` (bis v1.4.13 das `PauseMenu`)
  pausiert von selbst, sobald die App
  den Fokus verliert (`OnApplicationFocus(false)`) oder in den Hintergrund geht
  (`OnApplicationPause(true)`). Beim Verlassen der App meldet Android beides, eine
  heruntergezogene Benachrichtigungsleiste nur den Fokusverlust, und ohne Pause
  flöge das Schiff dabei weiter. Weiter geht es nur über „Continue“, nie von
  selbst. Im Editor zählt schon ein Klick außerhalb des Game-Fensters als
  Fokusverlust und pausiert den Run.
- **Auto-Pause testen:** Im Editor die beiden Methoden des `GameManager` per
  Reflection aufrufen, in jedem Zustand (Menü, Hangar, Run, Pause, Settings, Tod, nach
  Revive). Ein anderes
  Editor-Fenster per `EditorWindow.FocusWindowIfItsOpen` zu fokussieren, löst
  zwar echt aus, aber verzögert, solange Unity selbst nicht im Vordergrund ist.
  Den echten App-Wechsel gibt es nur auf dem Handy.
- Von Hand pausiert nur der Pause-Button. Eine Zurück-Geste auf Android oder
  Escape am PC gibt es bewusst nicht (Entscheidung Oktober 2026). Auch die
  Standardaktion „Cancel“ am `InputSystemUIInputModule`, die auf Escape lag, ist
  seit v1.4.5 abgehängt, weil kein UI-Element darauf reagierte.
- „Zurück“ in den Settings (`SettingsScreen`) zeigt den Screen des Zustands: Die
  Settings lassen ihn stehen, „Zurück“ führt also ins Pause- oder ins Hauptmenü.
- „Exit“ im Pause-Menü ruft `GameManager.ReturnToMenu`, also einen Szenen-Reload.
  `GameManager.Awake` setzt dabei `Time.timeScale` zurück.
- Die Musik läuft in der Pause bewusst weiter (Entscheidung Oktober 2026), so hört
  man den Lautstärkeregler, wenn man die Settings aus der Pause öffnet. Beim Tod
  pausiert sie dagegen, siehe „Audio-System“.
- **Zum Testen das Schiff anhalten.** Ohne Steuerung kracht es nach 2-3 s ins
  erste Hindernis, und der Test landet auf dem Todesbildschirm. Den Run per Code
  starten und `PlayerMotor.m_speed` per Reflection auf 0 setzen: Der Run bleibt
  aktiv, das Schiff bewegt sich aber nicht. Der Punkte-Multiplikator wird dabei
  negativ (1 plus Geschwindigkeit minus Startgeschwindigkeit), ein Testartefakt.

## Audio-System

Zentral ist `AudioSystem` (DontDestroyOnLoad, Singleton) mit drei AudioSources:
One-Shot-SFX, Motor-Loop, Musik. Alle drei routen in `GameAudio.mixer`
(Master → Music / SFX).

**Clips liegen nicht im AudioSystem, sondern in `AudioBank`-ScriptableObjects** -
`ClassicBank.asset` (Originalsounds 2020) und `NewBank.asset`. Umschalten heißt
Bank wechseln. Wichtig für die Erweiterung:

- **Einen neuen Sound hinzufügen = ein Feld mit Property in `AudioBank.cs`**, dazu
  eine `Play…`-Methode im AudioSystem. Beide Banks bieten den Slot dann automatisch
  an. Nicht zurück zu Einzelfeldern im AudioSystem gehen.
- Ein leerer Slot fällt automatisch auf die andere Bank zurück. `NewBank` enthält
  aktuell nur die zwei neuen Musikstücke; alle SFX kommen deshalb noch aus Classic.
  Das ist gewollt und kein Fehler. Die neuen SFX sind auf **v1.7.0** verschoben
  (Art- und Sound-Remaster); v1.4.0 wurde ohne sie abgeschlossen.

Vier Fallen, die hier schon einmal Bugs verursacht haben:

- **Nichts in der Szene zeigt aufs AudioSystem.** Es überlebt den Reload per
  `DontDestroyOnLoad`, die Kopie aus der neu geladenen Szene zerstört sich in
  `Awake()`. Eine Inspector-Referenz oder ein persistenter OnClick-Aufruf darauf
  zeigt nach dem ersten Reload ins Leere. Alles geht deshalb über
  `AudioSystem.Instance`, und das AudioSystem selbst kennt nichts in der Szene:
  - Die Bedienelemente unter `UI/Settings` (`NewSoundsToggle` „Use new Sound“,
    `VolumeSlider`, `SfxButton`, `MusicButton`) verbindet seit v1.5.0 der
    `SettingsScreen` am selben Screen, in `OnEnable`, und zeigt dabei den Stand
    des AudioSystem. Ein neues Bedienelement für den Ton kommt dorthin. Bis v1.4.13
    hielt das AudioSystem die Referenzen selbst und übernahm sie nach jedem Reload
    von seiner Szenenkopie (`AdoptSceneReferencesFrom`). Geprüft in v1.5.0 über die
    echten Buttons: Öffnen aus Hauptmenü und Pause, alle vier Elemente umschalten,
    Szene neu laden, der neue Stand ist zu sehen und lässt sich zurückschalten.
  - Klicksounds kommen von der Komponente `ButtonClickSound` am jeweiligen Button.
    Die alten Klick-Events zeigten seit Februar 2026 ins Leere. Kauf-Buttons haben
    bewusst keinen Klick, sie spielen eigene Select-/Kauf-Sounds; der Start-Button
    hatte nie einen. Die beiden Stumm-Schalter spielen ihren Klick seit v1.5.0 über
    den `SettingsScreen`, nach dem Umschalten: Wer die SFX ausschaltet, hört
    keinen Klick, wer sie einschaltet, hört ihn. Vorher ergab sich das nur aus der
    Reihenfolge, in der die Listener dazukamen.
  - Auf den Spielverlauf hört es über die statischen Ereignisse des `GameManager`
    (seit v1.5.0, siehe „Spielzustand“). Die brauchen keine Referenz und überleben
    den Reload. Die Kopie aus der neu geladenen Szene meldet sich gar nicht erst an.
- **Musik und Motor regelt nur das AudioSystem,** nach dem Spielzustand (seit
  v1.5.0). Die Methoden dafür sind privat, Musik oder Motor von anderswo zu steuern,
  kompiliert also nicht. Bis v1.4.13 teilten sich `GameManager`,
  `PlayerMotor` und `PauseMenu` diese Aufgabe, und ein Musik-Stopp in
  `PlayerMotor.StartRunning()` würgte früher die gerade gestartete Spielmusik ab.
  Einzelne Sounds (Explosion, Münze, Klicks, Kauf) spielt weiter, wer sie auslöst.
- **Mute ist Pause, nicht Lautstärke 0.** Musik wird pausiert (Position bleibt
  erhalten, Dekodierung stoppt wirklich), SFX werden gar nicht erst abgespielt.
  Beim Tod hält das Spiel die Musik seit v1.4.4 genauso an (`PauseMusic`, heute bei
  `ShipCrashed`), ein Revive setzt sie an derselben Stelle fort (`ResumeMusic` bei
  `ShipRevived`).
  Ein eigenes Flag (`m_isMusicOnHold`) hält beides auseinander: Entstummen auf dem
  Todesbildschirm startet die Musik nicht, ein Revive startet keine
  stummgeschaltete. Zurück ins Menü hebt `PlayMenuMusic` den Hold auf.
- **Der Lautstärkeregler ist nicht linear.** `SetMasterVolume` bildet die
  Reglerposition exponentiell auf einen 40-dB-Bereich ab (`k_VolumeRangeDb`), sonst
  passiert die gesamte hörbare Änderung in den unteren 20 % des Wegs. Wichtig:
  Den Wert zu quadrieren hilft **nicht** - eine Potenzkurve streckt den dB-Bereich
  gleichmäßig und lässt das Ungleichgewicht bestehen. Im Spielstand steht die
  Reglerposition, nicht die Amplitude. Beim Ziehen wird bewusst nicht gespeichert
  (60×/Sekunde Schreibzugriff): Der Wert steht nur in `SaveSystem.Data` und kommt in
  `OnApplicationPause`/`OnApplicationQuit` des `AudioSystem` auf die Platte.

**Import-Einstellungen** (September 2026 nach Unity-Empfehlung gesetzt): Musik auf
*Streaming* + Load In Background, SFX auf *Decompress on Load* mit 22050 Hz und
Vorbis-Qualität 0.6. Das hat den PCM-Speicher von ~75 MB auf ~0,8 MB gesenkt. Neue
Clips entsprechend importieren, sonst landen sie wieder komplett im RAM.

**Ableton-Projekt der neuen SFX:** In `Audio/New/SoundEffects Project/` entstehen
die neuen Sounds als Live-Set (seit Oktober 2026). Es liegt bewusst in `Assets/`,
neben den fertigen Sounds (Entscheidung Oktober 2026).

- **Im Repo** sind `SoundEffects.als` (in LFS, jeder Commit hält einen Arbeitsstand
  fest), `Ableton Project Info/` und später `Samples/` mit eigenen Aufnahmen und
  gesammelten Samples. **Nicht im Repo,** jeweils samt `.meta`: `Backup/` mit
  Abletons eigenen Sicherungen (das Repo ersetzt sie), die `.asd`-Analysedateien, die
  Ableton neben Samples legt, und die `Desktop.ini` für das Ordner-Symbol. Aus
  `Samples/` kommen nur die Samples selbst ins Repo, keine `.meta`: Live legt die
  Ordner darin selbst an, oft leer, und Git hält keine leeren Ordner. Ihre `.meta`
  stünden nach einem Clone ohne Ordner da. Weil nichts im Spiel auf das Live-Projekt
  zeigt, schadet es nicht, dass Unity sie nach einem Clone neu anlegt (seit v1.4.9).
  Die Regeln stehen am Ende der `.gitignore`.
- **Zum Öffnen nach einem Clone** braucht es Ableton Live 11 mit Core Library
  (gespeichert mit 11.3 Standard) und das Plugin Unison Zen Master (VST3). Alle
  Samples des Sets stammen aus der Core Library, das Set verweist nur auf sie (Stand
  Oktober 2026). Kommen Samples von woanders dazu, holt File > Collect All and Save
  sie ins Projekt. Samples aus Core Library und Packs dabei nicht mitkopieren, das
  Repo ist öffentlich.
- **Ins Spiel kommen nur die Exporte:** fertige SFX als WAV nach `Audio/New/`
  exportieren und wie oben importieren. Banks und Code zeigen nie auf Dateien im
  Projektordner.
- **Folge des Platzes in `Assets/`:** Unity legt für jede Datei im Projektordner eine
  `.meta` an und importiert Aufnahmen in `Samples/` als AudioClips. In den Build
  kommen sie nur, wenn etwas auf sie verweist.

## Render Pipeline: URP

Das Projekt lief bis September 2026 auf der Built-in Render Pipeline und wurde auf
**URP 17.6.0** umgestellt. Die Migration ist abgeschlossen: URP Asset liegt unter
`Assets/Settings/UniversalRenderPipeline.asset` (Renderer: `UniversalRenderer.asset`)
und ist in Project Settings > Graphics zugewiesen, alle Materialien sind konvertiert.

**Eine Qualitätsstufe, „Mobile“** (seit v1.4.12). Bis dahin gab es die sechs Stufen
aus Unitys Vorlage für die Built-in Pipeline, „Fastest“ bis „Fantastic“. Genutzt hat
das Spiel nur „Fastest“: Android, der Editor und alle anderen Plattformen standen
darauf. „Mobile“ ist diese Stufe unter neuem Namen und zeigte dasselbe (im Edit Mode
gerendert: Haupt- und Hangar-Kamera bitgleich). Danach kam Texture Quality „Full Res“
dazu, siehe „Texturen“.

- **In URP regelt die Stufe wenig.** Was auf dem Handy Leistung kostet, steht im
  URP-Asset, das die Stufe nutzt: Render Scale, MSAA, HDR, Schatten und Lichter. Die
  Stufe selbst bestimmt hier nur Texture Quality (siehe „Texturen“), anisotropes
  Filtern (aus) und vSync (aus, auf Android ohne Wirkung, siehe „Bildrate“).
- **Werte für Dinge, die das Spiel nicht hat,** stehen auf Unitys mittleren Werten
  statt auf denen von „Fastest“: LOD Bias 1 statt 0,3, Skin Weights 2 Bones statt 1,
  Particle Raycast Budget 256 statt 4. Es gibt keine LODs und keine Modelle mit
  Knochen, und die einzige Partikel-Kollision (Debris der Explosion) rechnet genau,
  ohne Budget (geprüft in v1.4.12: die Szene und alle 120 Prefabs und Modelle, die sie
  nutzt). Mit den alten Werten sähen neue Inhalte ab dem Remaster (v1.7.0) still
  schlechter aus.
- **Ändern** in Project Settings > Quality oder per Code wie das Quality-Fenster:
  `SerializedObject` auf `ProjectSettings/QualitySettings.asset`, danach
  `AssetDatabase.SaveAssets()`. So in v1.4.12.
- **Mehr Stufen erst mit Grund,** etwa wenn nach dem Remaster schwache Handys ruckeln:
  dann je Stufe ein eigenes URP-Asset, und das Spiel wählt die Stufe beim Start nach
  dem Gerät. Ein Grafikmenü im Spiel ist nicht geplant (Entscheidung Oktober 2026).

**HDR ist aus** (seit v1.4.12, `m_SupportsHDR` im URP-Asset, das die
Qualitätsstufe nutzt). Ohne Post-Processing brauchte nichts den größeren
Wertebereich. HDR ließ URP nur in eine Zwischentextur rendern und diese am Ende in
den Bildschirm kopieren (`BlitFinalToBackBuffer`, im Editor gezählt: mit HDR in
jedem Frame, ohne keinmal). Jetzt rendert die Kamera direkt in den Bildschirm, das
spart auf dem Handy die Kopie und ihre Bandbreite. Im Bildvergleich blieben Flammen,
Menü und Hangar gleich, bei der Explosion leuchten die Funken der ersten
Zehntelsekunden etwas schwächer (unter 1 % der Pixel). An beiden Kameras steht HDR
weiter an, das wirkt nur, wenn das Asset es erlaubt. Kommt Post-Processing dazu
(etwa Bloom im Remaster, v1.7.0), braucht es HDR wieder, dann Flammen und Explosion
neu ansehen.

**Schatten und zusätzliche Lichter sind aus** (seit v1.4.12, im URP-Asset unter
Lighting: „Cast Shadows“ des Main Light aus, „Additional Lights“ auf „Disabled“). Das
Spiel hat nur `Environment/DirectionalLight`, und das wirft keine Schatten. Das
Lights-Modul eines Partikelsystems in `BigExplosion` ist zwar an, hat aber kein Licht
zugewiesen und erzeugt keins. Trotzdem rechnete jeder beleuchtete Pixel beides mit:
Für ein Licht ohne Schatten rendert URP eine leere Shadow Map und schaltet
`_MAIN_LIGHT_SHADOWS` trotzdem ein, so kommt es mit weniger Shader-Varianten aus.
`_ADDITIONAL_LIGHTS` startete in jedem Pixel eine Schleife über null Lichter. Jetzt
sind beide Schlüsselwörter aus (nach einem Kamera-Render ausgelesen), Haupt- und
Hangar-Kamera rendern bitgleich wie vorher. Wie viel das auf dem Handy spart, ist
nicht einzeln gemessen, in v1.5.0 brauchte die GPU dort 5,5 bis 7 ms pro Frame (siehe
„Messen auf dem Handy“). Der erste Build danach schrieb neue `m_Prefilter…`-Werte ins
Asset, seit v1.5.0 im Repo: Builds lassen die Shader-Varianten für Schatten und
zusätzliche Lichter jetzt weg (siehe unten). Kommen im Remaster (v1.7.0) Schatten
oder weitere Lichter dazu, beides hier wieder einschalten, sonst bleiben sie
unsichtbar.

**Bilder vorher und nachher** nimmt im Play Mode `unity command capture_game_view
--source screen` auf, über den Bildschirm wie auf dem Handy. Die Aufnahme rendert
aber nicht neu, sie skaliert nur das Game-Fenster auf `--width` und `--height`. Für
echte Handy-Auflösung das Game-Fenster vorher auf eine feste Größe stellen (intern
`GameView.SetCustomResolution`, per Reflection), danach die alte Größe wieder wählen
und die neue löschen (`GameViewSizeGroup.RemoveCustomSize` erwartet den Gesamtindex).
Ohne `--save_path` kommt das Bild als Base64 zurück und lässt sich speichern, wo man
will. Mit `--save_path` legt die CLI jede Datei unter `Assets/` ab. Zeit anhalten
(`Time.timeScale = 0`), UI ausblenden und zuerst zweimal hintereinander aufnehmen:
Beide Bilder müssen gleich sein. So in v1.4.12 für HDR, je ein Bild mit und ohne
`Camera.allowHDR` (720×1280, im 9:16-Fenster also nur leicht hochskaliert), und für
die Texturen in 1080×2400. Ein Kamera-Render in eine eigene Render Texture (`--source
camera`) ignoriert HDR.

**Jeder Player-Build schreibt in zwei URP-Assets:** in `UniversalRenderPipeline.asset`,
welche Shader-Varianten er weglassen darf (`m_Prefilter…`), und in
`UniversalRenderPipelineGlobalSettings.asset` die Liste der Einstellungen, die ins
Spiel mitgehen (`m_RuntimeSettings`). URP schreibt beides bei jedem Build neu, der
Stand in Git ändert an der APK also nichts. Seit v1.4.5 ist der Stand des
Android-Builds committet, vorher standen dort nur Standardwerte. Ändert ein späterer
Build die Dateien erneut, etwa nach Änderungen an Qualitätsstufen oder Renderer, den
neuen Stand in einem eigenen Commit aufnehmen, nicht vermischt mit inhaltlichen
Änderungen.

Shader-Verteilung: 34 Materialien auf `ANIMO/BendWorld`, 13 auf URP/Lit, 7 auf URP
Particles (Lit/Unlit), 2 TextMesh Pro, 1 auf dem eingebauten `Unlit/Texture` (der
Sternenhintergrund, `Materials/Skybox/Background.mat`).

**Always Included Shaders** (Project Settings > Graphics) kommen in jeden Build, auch
wenn nichts sie nutzt. Seit v1.4.12 stehen dort nur noch Unitys Helfer für UI,
Cubemaps und `Hidden/Compositing`. „Legacy Shaders/Diffuse“ aus der Built-in Pipeline
nutzte kein Material, und kein Shader fällt darauf zurück (`BendWorld` fällt auf
URP/Unlit zurück). Die drei Video-Shader sind ebenfalls raus, und „Video“ steht auf
„Referenced“: Sie kommen nur mit, wenn eine Szene ein Video nutzt, das Spiel hat
keins. Die Einstellung allein nimmt sie nicht aus der Liste, das ging nur von Hand.

`Assets/Shaders/BendWorld.shader` ist der handportierte Curved-World-Shader (vorher
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

## Texturen

**Jede Textur erscheint auf dem Handy höchstens in ihrer Importgröße.** Die
Qualitätsstufe „Mobile“ hat seit v1.4.12 Texture Quality „Full Res“. Bis dahin stand
sie auf „Half Res“ (`globalTextureMipmapLimit: 1`, so seit dem ersten Stand des
Repos): Jede Textur mit Mipmaps erschien nur in halber Importgröße, und die oberste
Stufe lag ungenutzt in der APK. Für Full Res sind die 22 betroffenen Texturen halb so
groß importiert. So zeigt das Handy dieselbe Größe wie vorher, und die Texturen
belegen 20 MB weniger. UI-Sprites haben keine Mipmaps, und winzige Texturen wie die
2×2-Paletten der Asteroiden 001 bis 015 hatte Unity von Half Res ausgenommen
(`Texture2D.activeMipmapLimit` war 0). Der Editor nutzt dieselbe Stufe und zeigt
dasselbe wie das Handy.

**Kleiner importiert ist nicht bitgleich mit der halben Stufe.** Unity verkleinert
beim Import in einem Schritt aus der Quelle (Resize Algorithm „Mitchell“), Mipmaps
entstehen per Box-Filter aus der Stufe darüber. „Bilinear“ lag beim Sternenhimmel
(Quelle 1920×1080) noch weiter weg. Gemessen in v1.4.12 im Play Mode in 1080×2400:
Schiffe, Münzstaub und Explosion praktisch gleich (in 24 Bildern zusammen 35 Pixel
über 8 Stufen), in der Rost-Struktur der Münzen feines Rauschen (0,3 % der Pixel,
höchstens 38 Stufen), die Sterne minimal schwächer (0,2 %, höchstens 52).

**Nicht größer importieren als gezeigt** (seit v1.4.12): Die Max-Size richtet sich
danach, wie groß eine Textur höchstens auf dem Bildschirm erscheint. Gemessen in
1080×2400:

- `UI/Images/2020/CoinIcon.png` 512: Das Icon ist im Canvas höchstens 75 Einheiten groß,
  auf einem 3200 px hohen Display rund 300 px. Im Bildvergleich blieb es gleich, die
  Unterschiede zu 2000 px sind Rauschen in der Rost-Struktur.
- Die neun Schiffspaletten 512 ohne Alphakanal, `Asteroid016To025Color.png` 256,
  siehe Paletten.
- `Textures/Coin/CoinColor.jpg` 512: Münzen auf der Strecke nutzen höchstens die
  Mip-Stufe 256, eine Münze direkt vor der Kamera 512.
- Explosion, Rauch und Münzstaub 1024, der Sternenhintergrund 1024×512. Das Handy
  zeigt sie vergrößert: Ein Einzelbild des Feuers (6×6 Felder) hat 171 px, die
  Partikel werden bis zu 480 px groß, der Hintergrund läuft überall auf Mip-Stufe 0.
  Größer importiert wären sie schärfer, das gehört zum Remaster (v1.7.0).
- Hangar und Raumhafen (`HangarColor`, `NerousColor`, je zwei) 512, aus der Explosion
  `Debris` 128 und `shockwave` 256: nur halbiert, damit das Handy dieselbe Größe
  zeigt wie mit Half Res. Ob weniger reicht, ist nicht gemessen.

**Paletten** (Schiffe, Asteroiden-Atlas) sind Felder aus je einer Farbe, getrennt von
dünnen Linien. Bei ihnen zählt nicht die Größe auf dem Bildschirm, sondern wie weit
die UVs von der nächsten Farbgrenze entfernt liegen: Je kleiner die Textur, desto
weiter verwischen bilineares Filtern und ETC2 die Grenzen. Stand v1.4.12 liegt kein
Dreieck über einer Grenze, die Schiffe halten mindestens 40 Quellpixel (von 3000)
Abstand, die Asteroiden 60. Im Hangar änderten sich mit 512 statt 1024 auf dem Handy
höchstens 2 Pixel, mit 256 bis zu 21 an einer Kante. Flächen ohne UV-Abwicklung (UV
0,0 an allen Ecken, etwa bei Aristocrat Skin 1 und den Vagor-Skins) nehmen die Farbe
der vier Eckpixel an, also des schwarzen Rands. Bei kleinen Texturen mischt sich dort
das angrenzende Feld hinein. Ohne Transparenz brauchen Paletten keinen Alphakanal
(Alpha Source „None“), `ANIMO/BendWorld` liest ihn nicht.

**Android-Format ist ETC2** (Player Settings), nicht ASTC: mit Alphakanal 1 Byte pro
Pixel, ohne 0,5. Kantenlängen müssen durch 4 teilbar sein, sonst bleibt eine Textur
unkomprimiert (RGBA32). Das trifft 35 UI-Bilder mit zusammen 10,4 MB (etwa 648×207),
ein Sprite Atlas soll das in v1.5.1 lösen.

**Messen:**

- Größe im Android-Format über `TextureUtil.GetStorageMemorySizeLong` (intern, per
  Reflection), alle Texturen der Szene über `AssetDatabase.GetDependencies` auf
  `Game.unity`. Stand v1.4.12: 73 Texturen, 17,4 MB (vorher 62,6). Die Editor-Zahl
  von `Profiler.GetRuntimeMemorySizeLong` liegt höher, der Editor hält Kopien.
- Welche Mip-Stufe die GPU wählt: die Textur im Play Mode durch eine gleich große
  ersetzen, deren Stufen verschiedene Farben haben (rot, gelb, grün …), mit
  `ignoreMipmapLimit = true`, falls eine Qualitätsstufe die Auflösung senkt. Bei
  geteilten Materialien per `MaterialPropertyBlock`, nie am Material-Asset. Dann je
  ein Bild mit und ohne und die geänderten Pixel nach Farbton zählen.
- Ob eine Textur nach einer Änderung gleich aussieht: die oberste Stufe, die die GPU
  bekommt, per `Graphics.Blit` in eine Render Texture genau ihrer Größe kopieren und
  auslesen (in halber Größe die Stufe darunter), vorher und nachher Pixel für Pixel
  vergleichen. Das geht ohne Play Mode für alle Texturen auf einmal, so in v1.4.12 für
  die 22 halbierten.
- Partikelgrößen: die Zeit anhalten, den Effekt mit `ParticleSystem.Simulate`
  vorspulen und pro Teilchen `GetCurrentSize` durch die Tiefe zur Kamera teilen. Eine
  Aufzeichnung pro Frame verpasst die meisten Frames, wenn Unity im Hintergrund
  läuft.
- Bilder vorher und nachher wie unter „Render Pipeline: URP“. Für Objekte und Effekte
  die Zeit anhalten, sie auf eine eigene Ebene legen und nur diese vor schwarzem Grund
  rendern (`cullingMask`, `clearFlags`), Partikel mit festem `randomSeed` per
  `Simulate` vorspulen. Die Explosion nur bis etwa 1 s vergleichen: Danach prallt ihr
  Debris an den zufällig gespawnten Hindernissen ab, und die Bilder weichen auch ohne
  Änderung ab.

**`.meta` im alten Format** (Stand v1.4.12 noch 45 von 161 Texturen: 38 mit
`serializedVersion: 11`, darunter Schiffe, Münze und Asteroiden, und 7 aus
`Effects/EffectExamples` mit 4 und 5) per Text ändern wie Prefabs (siehe
„Code-Stil“): Über den Importer gespeichert, schreibt Unity die ganze Datei im neuen
Format, rund 60 Zeilen Diff ohne inhaltliche Änderung. Die Max-Size steht im Eintrag
`DefaultTexturePlatform` (die gleichnamige Zeile weiter oben zieht Unity nicht mehr
mit), der Alphakanal unter `alphaUsage`, 0 heißt None. Danach
`AssetDatabase.ImportAsset` mit `ImportAssetOptions.ForceUpdate`: Unity importiert
neu, schreibt die Datei aber nicht um.

## Messen auf dem Handy

Seit v1.5.0 gibt es Messwerte vom Handy, bisher von einem Gerät: Pixel 9 Pro (Android
17, Tensor G4 mit Mali-G715), 960×2142 bei „High resolution“, Display mit 60 bis 120
Hz. Mehr Handys kommen mit den Tests vor v2.0 (siehe „Arbeitsweise“).

**Stand v1.5.0** (Messbuild, Menü, Hangar und Run, dazu ein Run über 10 Minuten):

- 60,0 FPS überall. Am Display kamen 0 bis 0,03 % der Frames zu spät an.
- Der Main Thread arbeitet in Menü und Hangar 5,5 ms, im Run 6,2 bis 7,3 ms, mit dem
  Tempo steigend, von 16,7 ms pro Frame. Die GPU braucht 5,5 bis 7 ms (abgelesen in
  den Läufen mit Vulkan und mit Multithreaded Rendering, siehe unten). Draw Calls:
  Menü 11, Hangar 12, Run 195.
- GC: 48 bis 134 Byte pro Frame, im Run ein inkrementeller Durchlauf pro Minute,
  keiner über 3 ms, Heap höchstens 4,5 MB. Am meisten legt das Input System selbst an,
  dann der `TileManager` (jeder Abschnitt per `Instantiate`, der alte per `Destroy`,
  kein Pool), der Münzstaub und der Score-Text.
- Speicher über 10 Minuten ohne Anstieg. Der Release-Build belegt 313 bis 316 MB
  (PSS), v1.4.5 belegte 365 bis 373 MB, vor allem wegen der Texturen aus v1.4.12. Die
  APK hat 68,2 statt 73,0 MB.
- Nach 10 Minuten GPU 42 bis 43 °C, Gehäuse 34 °C, keine Drosselung.
- Die Font-Textur baut sich im Run bei fast jeder neuen Score-Zahl neu auf, bis 8-mal
  pro Sekunde, je 0,2 bis 0,4 ms (`Font.CacheFontForText`), im Menü nie. Mit TextMesh
  Pro im UI Overhaul (v1.5.1) fällt das weg.

**Die Bildausgabe hat zwei Phasen.** Zeitweise wartet der Main Thread in jedem Frame
2 bis 3,5 ms im Blit auf einen freien Bildpuffer statt in `WaitForTargetFPS`, und ein
Bild braucht vom Abschicken bis aufs Display rund 37 statt 24 ms. FPS bleiben 60. In
den Messungen war das 22 bis 41 % der Zeit so, eingeleitet von einem Frame mit langem
Blit. Frame-Zeiten deshalb nicht direkt vergleichen, sondern die Arbeit: PlayerLoop
minus `WaitForTargetFPS` minus `Graphics.PresentAndSync`.

**Entschieden in v1.5.0** (Oktober 2026), jede Einstellung einzeln gegen denselben
Messbuild gemessen:

| Einstellung | bleibt | gemessen |
|---|---|---|
| Blit Type | „Always“ | „Auto“ spart den Blit, aber das Warten wandert in den Render von URP, und die Bildfolge wird unruhig: 0,56 % der Frames kamen zu spät aufs Display |
| Optimized Frame Pacing | aus | hält die Bildausgabe gleichmäßig bei 25 ms. Unitys Frame-Zeiten und damit `Time.deltaTime` schwanken dafür stärker (99 % unter 22 bis 25 statt 18 ms), ob man das sieht, ist nicht geprüft |
| Multithreaded Rendering | aus | Main Thread im Run 6,4 statt 7,9 ms beschäftigt, aber jedes Bild kommt einen Frame später aufs Display (über 30 statt 24 ms). Zusammen mit Frame Pacing wieder 25 ms |
| Grafik-API | nur OpenGL ES 3 | Vulkan zeigte das Bild fehlerfrei, Main Thread 7,0 statt 7,9 ms, aber wie Multithreaded Rendering einen Frame später (rund 36 ms) |
| Run In Background | an | aus rendert das Spiel bei heruntergezogener Leiste nicht mehr (an: 60 FPS hinter der Leiste), die Auto-Pause greift trotzdem. Gilt aber auch im Editor, siehe unten |
| Frame Timing Stats | an | brauchen der Rendering Debugger („Display Stats“) und die Messung, vor v2.0 entscheiden |

- Frame Pacing, Multithreaded Rendering und Vulkan kommen mit den Tests auf mehr Handys
  vor v2.0 wieder auf den Tisch, am ehesten zusammen. Für Displays mit 90 Hz spricht
  eher etwas für Frame Pacing: Laut einem Unity-Mitarbeiter (2021) schaltet es
  Displays, die das können, auf 60 Hz um. Multithreaded Rendering war für Android
  schon im ersten Stand des Repos aus (`mobileMTRendering` nennt nur iPhone und tvOS),
  Unity hat es in neuen Projekten an.
- **„Run In Background“** steht in den Player Settings für alle Plattformen, auch für
  den Editor: Laut Unity-Forum hält der Editor den Play Mode dann an, sobald er den
  Fokus verliert, und die CLI-Tests laufen oft mit Unity im Hintergrund. Geprüft ist
  das nicht, beim Versuch in v1.5.0 hatte Unity den Fokus, und beide Einstellungen
  liefen gleich. Nur fürs Handy ginge es im Code (`Application.runInBackground`), ob
  Android den Wert zur Laufzeit übernimmt, ist nicht geprüft.

**So misst man** (Stand v1.5.0):

- **Messbuild:** Development Build mit Autoconnect Profiler und der Managed Code
  Variant „Checked“, sonst fehlen Test-Panel und der Haken „Invincible“ (siehe
  „Tests“). Damit die Player Settings auf Release bleiben, ein eigenes Build-Profil
  als Kopie des Android-Profils anlegen, mit einem Player-Settings-Override und darin
  „Checked“. Per `eval` geht das über `BuildProfileDataSource.DuplicateAsset`,
  `BuildProfileModuleUtil.CreatePlayerSettingsFromGlobal` und
  `SerializePlayerSettings` (alle intern, per Reflection,
  `BuildProfile.CreateBuildProfile` scheitert aus `eval`), dann bei aktivem Profil
  `PlayerSettings.SetManagedCodeVariant`, das schreibt in den Override. Bauen mit
  `unity command build --outputPath Builds/<Name>.apk --profileName <Name> --options
  '["Development","ConnectWithProfiler"]' --confirm true`: Das Kommando aktiviert das
  Profil, nimmt Development und Profiler aber nur aus `--options`, als JSON-Array.
- **Danach aufräumen:** `BuildProfile.SetActiveBuildProfile(null)`, das Profil löschen
  und die URP Global Settings zurücksetzen. Der Override ist eine vollständige Kopie
  der Player Settings und veraltet mit der nächsten Änderung dort, etwa der
  `bundleVersion`. Mess-Builds schreiben außerdem `RenderingDebuggerRuntimeResources`
  in die Global Settings, das gehört nicht ins Repo.
- **Profiler über USB:** `adb forward tcp:34999
  localabstract:Unity-com.ANIMOGames.SpaceEscaper`, dann im Editor
  `ProfilerDriver.DirectIPConnect("127.0.0.1:34999")`. `adb` liegt im Android-SDK des
  Editors (`…/PlaybackEngines/AndroidPlayer/SDK/platform-tools/`).
- **Android misst selbst,** auch in Release-Builds: `dumpsys SurfaceFlinger --timestats`
  (wann jedes Bild am Display ankam), `dumpsys meminfo com.ANIMOGames.SpaceEscaper`
  (Speicher als PSS) und `dumpsys thermalservice` (Temperaturen, Drosselung).
- **APK-Größe** aus den Einträgen der APK zählen, nicht aus der Dateigröße:
  Inkrementelle Builds lassen bis 20 MB Lücken in der Datei. Ein Messbuild (123 MB) ist
  nicht mit der Release-APK vergleichbar.
- **Spielen per adb:** `adb shell input tap x y` auf Punkte, die man pro Handy und
  Auflösung aus einem Screenshot abliest (`adb exec-out screencap -p`). Für Runs über
  Minuten den Haken „Invincible“ setzen und in den Entwickleroptionen „Wach bleiben“
  an, das Spiel setzt kein `Screen.sleepTimeout`. Eine Benachrichtigung oben fängt
  Taps ab, vorher `adb shell cmd statusbar collapse`. Ob ein Tap ankam, per Screenshot
  prüfen: In v1.5.0 ging einmal auch ohne Benachrichtigung einer verloren.
- **Vergleichen:** Varianten erst messen, wenn das Handy wieder auf Ruhetemperatur ist
  (am Ladekabel GPU rund 43 °C), und die Basis zur Gegenprobe noch einmal. Die GPU-Zeit
  aus Frame Timing taugt nicht als absolute Zahl: Die GPU taktet herunter, bis ihre
  Arbeit gerade in den Frame passt (im Basis-Build 16 bis 20 ms).
- **Im Log** (`adb logcat -s Unity`) steht beim Start ein `ClassNotFoundException` für
  `com.google.android.play.core.assetpacks.AssetPackManager`: Unity sucht Play Asset
  Delivery, das Spiel nutzt es nicht. Ähnliches meldeten laut Unity auch leere
  Projekte, das Spiel läuft danach normal.

## Sprache

Antworten auf Deutsch. Code, Bezeichner und Commit-Messages auf Englisch
(bestehende Konvention: `add:`, `edit:`, `fix:`, `refactor:`).

Keine langen Gedankenstriche (Geviert- und Halbgeviertstrich, U+2014 und U+2013),
weder in Doku, Commits, PRs, Releases und Tags noch in Antworten. Im Text steht
stattdessen `-`, als Trenner in Titeln `|` (z. B. `v1.4.2 | Input System & Engine
Flame Fix`).
