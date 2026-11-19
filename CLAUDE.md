# Space Escaper

3-Lane Endless Runner für Android, Unity **6000.6.0f1**, URP 17.6.0.
Ursprünglich 2020 in Unity 2019.4 gebaut und im Play Store veröffentlicht - aktuell
ein Legacy-Projekt, das schrittweise modernisiert wird. Hobbyprojekt, kein Zeitdruck,
Arbeit passiert in unregelmäßigen Sessions.

## Aufbau

Repo-Root ≠ Unity-Projekt: das Unity-Projekt liegt in `Space-Escaper/`.

- `Space-Escaper/Assets/Scripts/` - 43 Skripte, ~3900 Zeilen, alle im Namespace
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
  nur bauen, wenn danach gefragt wird. Die letzte Stichprobe war v1.5.0, v1.5.1 ist nur
  im Editor geprüft, die nächste kommt vielleicht mit v1.7.0 (Entscheidung 18.11.2026).

## Code-Stil

Seit v1.4.5 folgen alle Skripte dem Unity-Styleguide „Use a C# style guide for
clean and scalable game code“ (Unity-6-Ausgabe, 2025, PDF über
unity.com/resources/c-sharp-style-guide-unity-6). Wo er die Wahl lässt, gilt hier:

- **Namespace** `SpaceEscaper` für alle Skripte. In `unity command script eval` deshalb
  `SpaceEscaper.GameManager` usw. schreiben. Veraltete APIs lehnt `script eval` als
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
darin und gratis, alle anderen mit Preis, jedes mit eindeutiger ID,
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
Argument findet. Seit v1.5.1 hat die Szene keinen Legacy-`Text` mehr, alle Texte sind
TextMesh Pro (siehe „Texte“ unter „UI“). Ebenfalls seit v1.5.1 prüft `SpriteAtlasTests`,
dass jedes UI-Bild der Szene in einem Sprite Atlas liegt und der Packer dafür an ist,
dass jeder Atlas mit in den Build kommt, keiner dreht oder eng packt und keiner ein Bild
hält, das das Spiel nicht nutzt (siehe „Bilder liegen im Sprite Atlas“ unter „UI“).
Ebenso seit v1.5.1 legt `LayoutTests` die Screens so aus, wie zehn Displays sie zeigen,
von Tablets in 4:3 bis zum schmalen Außendisplay eines Klapphandys, fünf davon mit
Kameraloch oder Balken: Alles Sichtbare und jede Tippfläche liegt in der Safe Area, kein
Text läuft aus seinem Rechteck, und jedes Bedienelement nimmt Tipps auf mindestens 48 dp
an (siehe „Layout für alle Seitenverhältnisse“ unter „UI“). Was eine Maske abschneidet,
zählt dabei nicht (seit Schritt 3b von v1.5.1). Die Settings passen auf jedem der zehn
Displays ohne Scrollen unter ihren Titel (ebenso seit 3b).
Seit v1.5.2 prüft `CreditsTests`, dass oben in der README dieselben Credits stehen wie im
Screen der Credits: dieselben Abschnitte mit denselben Einträgen in derselben Reihenfolge
(Wunsch Moritz, 18.11.2026, siehe „Credits“ unter „UI“).
Seit v1.5.0 prüft `StaticFieldTests`, dass
sich jedes statische Feld des Spiels beim Start des Play Mode zurücksetzt (siehe
„Code-Stil“): Der Test füllt alle, Enums mit ihrem letzten Wert (seit v1.5.1), ruft die
Methoden mit `SubsystemRegistration` auf wie Unity und erwartet alle
wieder leer. Danach stellt er die alten Werte wieder her, der Editor liest sie auch.

Seit v1.4.11 gibt es außerdem einen PlayMode-Test in `Assets/Tests/PlayMode/`
(`RunTests`). Er spielt einen Run über die Buttons des Spiels: Play, geradeaus bis
zum ersten Hindernis, dann prüft er Todesbildschirm und Spielstand und seit v1.5.0,
dass der Score dort eine Sekunde lang stehen bleibt. Seit v1.5.1 prüft er dort auch
Strecke und Zeit des Runs (gegen den Weg und die Zeit, die das Schiff geflogen ist) und
„NEW HIGHSCORE“: Sein Spielstand beginnt mit Highscore 5, den der Run schlägt. Dann tippt er Revive
und prüft, dass das Schiff 25 Einheiten weiterfliegt, ohne wieder zu crashen. Mit
dem Revive-Bug aus v1.4.4 crashte es im Frame des Revive gleich wieder (siehe
„Bekannte Altlasten“). Seit v1.4.13 rückt die Welt dabei alle 20 statt alle 4800
Einheiten zurück (siehe „Floating Origin“ unter „Aufbau der Szene“), zweimal vor
dem Crash und einmal kurz nach dem Revive, während die Explosion noch brennt. In
jedem Frame prüft er, dass nichts springt: Das Schiff bleibt nahe am Ursprung,
Schiff und Kamera bewegen sich stetig, alles andere behält seinen Platz in der
Welt, die Gruppen der Szene bleiben im Ursprung, und die Abschnitte liegen
lückenlos hintereinander bis zur nächsten Spawn-Position. Ein zweiter Test
(`DeathScreenStartsANewRunOrOpensTheHangar`, seit v1.5.1) tippt nach einem Crash „Play
again“ und prüft, dass nach dem Neuladen gleich ein neuer Run läuft, ohne Münzen und seit
v1.5.2 nach den beiden Ladeframes ohne Zeit und Strecke (siehe „Play again“ unter
„Spielzustand“), dann nach dem nächsten Crash „Hangar“.

- **Ausführen:** im Editor unter Window > General > Test Runner, Reiter EditMode
  oder PlayMode. Per CLI die EditMode-Tests mit `unity command test run`. Sie dauern
  wenige Sekunden, also vor jedem Commit, der `SaveData`, `SaveSystem`, `RunCoins`, die
  Szene, UI-Bilder, den Atlas oder eine Methode ändert, die ein Button aufruft. Grün ist
  ein Lauf bei Exit-Code 0, bei 6 ist ein Test rot (gesehen in v1.5.2 mit dem
  Pipeline-Paket 1.0.0, davor 8), `--format json` gibt eine Zeile pro Test aus. Den
  PlayMode-Test nur asynchron:
  `unity command test run --mode playmode --async-tests true`, dann
  `unity command test status --detail summary` abfragen, bis es nach `running`
  `completed` meldet. Bis der neue Lauf läuft, zeigt es noch das Ergebnis des letzten.
  Synchron lehnt die CLI PlayMode-Tests ab, ein Domain Reload beim Start des Play Mode
  verlöre die Anfrage. `--mode all --async-tests true` führt beide Modi in einem Lauf
  aus (in v1.5.2 79 Tests in rund 21 s). Mit dem Wechsel in den Play Mode dauern die
  beiden PlayMode-Tests rund 19 s, also
  vor Commits, die Run, Tod, Revive oder das Spawnen ändern. Meldet ein asynchroner
  PlayMode-Lauf 0/0, greift der Umweg `TestFrameworkFix` nicht mehr (siehe „Play Mode
  ohne Domain Reload“ unter „Unity-Besonderheiten“). Die CLI legt eine
  `TestResults.xml` neben den Spielstand, die darf weg. Ist die offene Szene
  ungespeichert, fragt der Test Runner vorher per Dialog, ob er sie speichern soll.
  Bis jemand klickt, steht Unity, und die CLI bricht nach ihrer Zeitgrenze ab.
- **Szenen im Test:** Während des Laufs tauscht der Test Runner die offenen Szenen
  gegen eine leere und stellt sie danach wieder her. `SceneTests` öffnet
  `Game.unity` deshalb selbst daneben und schließt sie am Ende wieder. Ist sie schon
  geladen, prüft der Test sie, wie sie ist, und lässt sie offen. Ungespeicherte Änderungen
  sieht er trotzdem nicht: Der Test Runner lädt die Szene beim Wiederherstellen von der
  Platte (gesehen in v1.5.1). Wer einen Test mit einer geänderten Szene rot sehen will,
  speichert sie und legt danach den alten Stand zurück.
- **Layout im Test:** `LayoutTests` legt eine Kopie der UI in eine Vorschau-Szene
  (`EditorSceneManager.NewPreviewScene`), so bleibt `Game.unity` unberührt, auch wenn
  sie offen ist. Der Root-Canvas der Kopie steht auf World Space, mit der Größe, die der
  `CanvasScaler` auf dem Display ergäbe, und `SafeArea.Fit` bekommt dessen Safe Area. In
  der Kopie ist jeder Screen an, denn TextMesh Pro rechnet einen Text erst, wenn er
  einmal an war. Die Displays stehen oben im Test, die Safe Areas mit Kameraloch stammen
  aus dem Device Simulator. Layout-Gruppen legt der Test vor dem Prüfen aus wie das Spiel
  vor dem Zeichnen (`LayoutRebuilder.ForceRebuildLayoutImmediate`, seit v1.5.1), sonst
  stünden ihre Texte noch in den Rechtecken der Vorlage. Eine `RectMask2D` zeichnet ihre
  Kinder nur in ihrem Rechteck und nimmt Tipps auf sie nur dort an, der Test schneidet
  sichtbare Rechtecke und Tippflächen deshalb an jeder Maske darüber ab.
- **Der PlayMode-Test spielt echt, aber stumm und ohne den echten Spielstand.** Ein
  Tod schreibt Münzen und Highscore, deshalb lenkt `SetUp` den Spielstand mit
  `SaveSystem.LoadFrom` in einen eigenen Ordner um, im Editor
  `%LOCALAPPDATA%\Temp\ANIMO Games\Space Escaper\PlayModeTests\`. Der beginnt leer
  wie eine Neuinstallation, nur mit Highscore 5. Stumm ist der Test über
  `AudioListener.volume` 0 (seit v1.5.1, bis dahin über den Lautstärkeregler): Musik und
  Sounds laufen durch denselben Code wie im Spiel, man hört sie nur nicht. Ein Regler auf
  0 hielte die Musik an und spielte keine Sounds, das prüfte weniger. Den echten
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
  - **In den Settings** steht seit v1.5.0 dafür auch ein Schalter „Invincible“. Der
    `SettingsScreen` legt ihn in `Awake` als Kopie der Zeile „Classic Mode“ an, die
    Layout-Gruppe ihres Abschnitts setzt ihn als letzte Zeile unter GAME (seit v1.5.1), in
    der Szene steht er nicht. Auf dem Handy ist er leichter zu treffen
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
  diese beiden. In v1.5.1 fand `NoLegacyTextIsInTheScene` vor dem Umbau alle 9
  Legacy-Texte. `SpriteAtlasTests` fand vor dem Atlas den alten Packer-Modus und danach
  alle 36 UI-Bilder, mit eingebauten Fehlern ein ungenutztes Bild im Atlas, einen Atlas
  außerhalb des Builds, Drehen und enges Packen. `LayoutTests` fand vor dem Umbau auf
  20:9 die Titel „PAUSED“ und „SETTINGS“ (je 3 Einheiten links draußen), auf schmaleren
  Displays und unter Kameralöchern mehr, dazu sechs zu kleine Tippflächen. Mit
  eingebauten Fehlern fand er einen Text, der umbricht, einen ohne Umbruch, der zu breit
  ist, „EXIT“ im Hangar außerhalb der `SafeArea` (auf allen vier Displays mit Kameraloch)
  und einen Pokal mit 50 statt 60. Die Texte fand er erst, seit die Kopie alle Screens
  einschaltet, vorher prüfte er nur die des Hauptmenüs. In Schritt 3b von v1.5.1 konnte
  `StaticFieldTests` den neuen statischen Enum im `GameManager` erst nicht füllen und fand
  ohne die beiden neuen Zeilen in `ResetStaticState` genau `GameManager.s_reloadTarget` und
  `GameMenuScreen.s_hasShownSwipeHint`. `LayoutTests` meldete vor dem Abschneiden an Masken
  das Ende der Settings-Liste unter der Safe Area, danach mit einer Liste, die selbst unter
  die Safe Area reicht, den INFO-Abschnitt auf vier Displays. `TheSettingsFitWithoutScrolling`
  war mit den Abständen aus Version 6 auf 16:9, dem offenen Klapphandy und beiden Tablets
  rot (748,8 Einheiten hoch bei 676 bis 688 Platz). Der Test für „Play again“
  scheiterte, solange der `GameManager` nach dem Neuladen nichts tat. In v1.5.2 war
  `CreditsTests` gegen die alte README rot (0 statt 18 Zeilen) und mit einer README, in der
  bei Moritz „Composer“ fehlte (die Zeile an Index 1). Der Test für „Play again“ war in
  v1.5.2 erst nur in einem von zwei Läufen rot (Strecke 1 statt unter 1). Seit er Zeit und
  Strecke 0 nach den beiden Ladeframes verlangt, war er gegen den alten Code in jedem Lauf
  rot (0,081 s), mit `IsShipFlying` in drei Läufen hintereinander grün.
- Die Tests brauchen eigene Assemblies, siehe „Assembly Definitions“ unter
  „Unity-Besonderheiten“.

## Ordnerstruktur

Seit v1.4.5 ist `Assets/` nach Asset-Typ sortiert, wie es Unitys Leitfaden zur
Projektorganisation und die Unity-Templates vorsehen. Im Root von `Assets/` liegen
nur Ordner.

| Ordner | Inhalt |
|---|---|
| `Audio/` | Clips in `Classic/` und `New/`, die beiden Banks, der Mixer, in `New/` das Ableton-Projekt der neuen SFX |
| `Branding/` | App-Icon, Splash-Logos und Splash-Hintergrund (Player Settings), Banner, weitere Logos |
| `Data/` | ScriptableObjects mit Spieldaten, nach Gruppe (`Ships/`: ein `ShipData` pro Schiff und der `ShipCatalog`) |
| `Editor/` | Werkzeuge für den Editor: das Test-Menü und seine Assembly Definition |
| `Materials/` | Materialien, nach Gruppe |
| `Models/` | `.fbx` und `.blend`-Quellen samt ihren Texturen, nach Gruppe |
| `Prefabs/` | nach Gruppe, dazu `Chunks/` (Streckenabschnitte), `AsteroidFields/` (Hintergrund) und `UI/` (Bausteine der Screens, siehe „UI“) |
| `Scenes/` | `Game.unity` |
| `Scripts/` | alle Skripte in Ordnern nach Bereich (siehe unten), im Root nur `SpaceEscaper.asmdef` und `AssemblyInfo.cs` |
| `Settings/` | URP-Asset, Renderer, Global Settings, Volume Profile, Build Profiles |
| `Shaders/` | `BendWorld.shader` |
| `Tests/` | `EditMode/` und `PlayMode/`, je mit den Tests und ihrer Assembly Definition |
| `Textures/` | Texturen der Unity-Materialien, nach Gruppe |
| `UI/` | `Images/` nach Herkunft (siehe unten), `Fonts/` (Schriften, ihre Lizenztexte und ihre Font Assets für TextMesh Pro), `UIAtlas.spriteatlasv2` (der Sprite Atlas der UI-Bilder, siehe „UI“) |

- **Gruppen heißen in allen Typ-Ordnern gleich:** `Asteroids`, `Coin`, `Hangar`,
  `Ships`, `Spaceport` und `Skybox`. Was zum
  Schiff gehört, liegt also in `Models/Ships/`, `Materials/Ships/`,
  `Textures/Ships/` und `Prefabs/Ships/`. `Animations/` gibt es seit Schritt 4 von
  v1.5.1 nicht mehr, sein letzter Inhalt war der Animator von „Tap to Start“ (siehe
  „Keine Animatoren für Übergänge oder kurze Effekte“ unter „UI“). Ein neuer Clip bekäme
  den Ordner wieder, mit Gruppen wie oben.
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
  Bilder von v1.0 bis v1.2, glänzend orange auf 2048er-Leinwänden. `2021/` hatte die 39
  aus dem UI-Umbau von v1.3, flach und weiß, seit Schritt 4 von v1.5.1 nur noch die neun
  Skin-Bilder in `2021/Shop/`. `2026/` hat die 27 Symbole, Platten, Rahmen, Leuchten, das
  Logo und die Münze aus v1.5.1, die Preisschilder aus v1.4.1 sind seit Schritt 4 weg. Seit
  v1.5.2 liegen dort auch das Schiff am Ende der Credits (`CreditsShip`, das Bild von
  Aristocrat Skin 1 aus `2021/Shop/` ohne seine Platte) und die Lizenztexte der Symbole,
  `FeatherMIT.txt` und `LucideISC.txt`, wie die OFL-Texte neben den Schriften, und für die
  Achievements die Symbole ihrer Beispiele (`NutIcon`, `ZapIcon`, `FlagIcon`, `BookOpenIcon`,
  `NavigationIcon`, `ShoppingBagIcon`, nach ihrer Form benannt, das Buch brauchen auch die
  Logbücher der Collectibles) und der Ring um ihren Anteil (`ProgressRing`), für die
  Collectibles das Schloss ihrer Plätze (`LockIcon`) und die Symbole zweier Abschnitte
  (`LeafIcon`, `StarCardIcon`). Die
  Münze ist die aus `2020/`, auf die Mutter zugeschnitten (siehe „Nicht größer importieren
  als gezeigt“ unter „Texturen“). Das Spiel nutzt 47: die neun aus `2021/Shop/` und alle
  aus `2026/`. Die aus `2020/` nutzt es nicht, sie bleiben für einen Classic Mode (v1.9.1,
  siehe „Bekannte Altlasten“). Neue Bilder kommen in den Ordner ihres Jahres.
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
| `UI` | Canvas mit `ScreenManager`, darin je ein Canvas pro Screen mit seiner Komponente: `DeathMenu` (`DeathMenuScreen`), `MainMenu` (`MainMenuScreen`), `GameMenu` (`GameMenuScreen`), `PauseMenu` (`PauseMenuScreen`), `Settings` (`SettingsScreen`), `Credits` (`CreditsScreen`), `Achievements` (`AchievementsScreen`), `Collectibles` (`CollectiblesScreen`), `Shop` (`ShopScreen`), in jedem ein Kind `SafeArea`, siehe „UI“ |

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
- **Animationsclips** heißen `Ziel_Zustand` (bis v1.5.1 `TapToStart_Pulse`),
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
  Die CLI bricht einen Aufruf nach seiner Zeitgrenze ab (`--timeout`, bei
  `script eval` 30 s), die Arbeit läuft aber weiter. Danach warten, bis
  `script eval` wieder antwortet, und das Ergebnis selbst prüfen.
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
  References, für `SceneTests`, seit v1.5.1 auch mit `Unity.TextMeshPro`, für
  `LayoutTests`) und seit v1.4.11 `SpaceEscaper.Tests.PlayMode`. Die
  PlayMode-Assembly gilt für alle Plattformen, nur dann führt der Test Runner sie
  unter PlayMode. Das Test-Menü liegt seit v1.4.11 in `SpaceEscaper.Editor`
  (`Editor/`, nur für den Editor). `Scripts/AssemblyInfo.cs` öffnet ihm und den
  PlayMode-Tests `internal` (`InternalsVisibleTo`), derzeit für
  `SaveSystem.LoadFrom` und `SaveSystem.Delete`, den PlayMode-Tests seit v1.4.13
  auch für `FloatingOrigin.Distance`, `TileManager.Tiles` und
  `TileManager.NextSpawnZ`, seit v1.5.0 für die Buttons der Screens
  (`MainMenuScreen.StartButton` usw.). Seit v1.5.1 öffnet sie auch den EditMode-Tests
  `internal`, für `SafeArea.Fit`. Test-Assemblies kommen an
  `Assembly-CSharp` nicht heran. Neue Skripte in `Scripts/` landen von selbst in
  `SpaceEscaper`. **Nutzt ein Skript ein weiteres Paket**, gehört dessen Assembly in
  die References der `SpaceEscaper.asmdef`, sonst findet der Compiler es nicht.
  Derzeit stehen dort `Unity.InputSystem`, `Unity.RenderPipelines.Core.Runtime` (für
  `TestPanel`), `Unity.TextMeshPro` (seit v1.5.1, für die Texte der Screens) und
  `UnityEngine.UI`. Szene
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
- **Unity-CLI:** Sie steuert den laufenden Editor über das Paket `com.unity.pipeline` im
  Manifest, seit v1.5.1 in Version 1.0.0. Meldet die CLI, dass das Paket veraltet ist,
  hebt `unity pipeline upgrade` es an. Das ändert Manifest und Lock-Datei und gehört in
  einen eigenen Commit. Unity übernimmt es erst nach `unity command package resolve` oder
  wenn sein Fenster den Fokus bekommt. Seit 1.0.0 stehen die Befehle in Gruppen
  (`unity command script eval`, `test run`, `editor status` …), die Parameter in
  kebab-case. `unity command` listet die Gruppen, `unity command <Gruppe>` deren Befehle,
  `--help` dahinter die Parameter. `script eval` gibt den Rückgabewert direkt zurück,
  ein Fehler endet mit Exit-Code 6. Den Play Mode starten und beenden
  `unity command editor.playmode start` und `stop`, ob er läuft, zeigt `editor status`
  (`playMode`).
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
  - Mit Domain Reload läuft der Code genauso, die Einstellung lässt sich jederzeit
    zurückstellen.
- **UI-Texte bleiben auch skaliert scharf,** seit sie TextMesh Pro sind (v1.5.1,
  siehe „Texte“ unter „UI“): Die Schrift kommt aus einem Signed Distance Field. Der
  Legacy-`Text` davor wurde in seiner Schriftgröße gerastert und dann hochgezogen,
  deshalb war der Toggle „Use new Sound“ von v1.4.0 bis v1.4.4 auf Skalierung 2 mit
  Schriftgröße 10 matschig. Größer heißt trotzdem Schriftgröße und Rect-Größe,
  Skalierung 1 lassen, sonst passen Rechteck und sichtbarer Text nicht zusammen.
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
- **Test-APKs** baut die Unity-CLI asynchron: `unity command build run --output-path
  Builds/<Name>.apk --confirm true`, danach `unity command build status` abfragen, bis
  es `completed` meldet. Vorher den Play Mode beenden. Die Einstellungen kommen aus
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
  adaptives Icon mit getrennten Ebenen kommt mit dem neuen Branding (v1.9.2).
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

- Ungenutzte Materialien und Modelle liegen noch im Projekt, etwa Doppelgänger mit
  `Unused` im Namen. Aufräumen gehört zum Remaster (v1.7.0), die UI hat Schritt 4 von
  v1.5.1 aufgeräumt: 30 Bilder aus `2021/`, die drei Preisschilder in `2026/`, den
  Animator von „Tap to Start“ und die alten Schriften. **Was zum Look von 2020 gehört,
  bleibt** (Entscheidung 18.11.2026): Ein Classic Mode (v1.9.1) soll später das Original
  von 2020 zeigen, dessen UI, Sounds, Musik, Objekte und Schiffe, aber mit dem neuen Code
  und den heutigen Preisen. Das Remaster legt neue Modelle, Materialien, Texturen, Effekte
  und Sounds deshalb neben die alten, statt sie zu ersetzen. Die Texte im Spiel setzte
  2020 Neuropol X (`Branding/FontsUsed.txt`), deren kostenlose Lizenz keine Apps abdeckt.
  Sie ist seit Schritt 4 gelöscht, im Classic Mode nehmen die Texte eventuell die
  heutigen Schriften. Bitgleiche Duplikate sind seit
  v1.4.5 gelöscht (39 in `Assets/UI/`, 4 beim Sortieren der Ordner), alle
  UI-Bilder liegen in `UI/Images/`. Von ihren 85 nutzt das Spiel 47, die übrigen 38 sind
  die von 2020 (siehe „Ordnerstruktur“). Gleiche Texturen
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
  Highscore im Spielstand. Der Pokal im Hauptmenü bleibt trotzdem **bewusst sichtbar**
  (Entscheidung Oktober 2026), seit v1.5.1 als Symbol neben dem Highscore
  (`UI/MainMenu/SafeArea/Stats/TrophyIcon`) statt als Button ohne Funktion
  (Entscheidung 17.11.2026). Mit Google Play Games (v2.0) wird er wieder ein Button zur
  Rangliste.

## Spielstand (SaveData)

Seit v1.4.7 steht der ganze Spielstand in einer Klasse, `SaveData`: Münzen,
Highscore, geflogenes Schiff, gekaufte Schiffe und die Audio-Einstellungen
(Entscheidung Oktober 2026). `SaveSystem` lädt ihn einmal beim Start, vor der ersten
Szene, und schreibt ihn als `SaveData.json` nach `Application.persistentDataPath`.
Alle Skripte lesen und ändern ihn über `SaveSystem.Data`, PlayerPrefs nutzt das
Spiel nicht mehr.

- **Gespeichert wird sofort** bei Tod, Kauf, Schiffswahl und den Audio-Schaltern,
  dazu beim Pausieren und Beenden der App, für die Regler von Musik und Effekten (siehe
  „Audio-System“). Bis dahin stehen Änderungen nur im Speicher.
- **Gesamtlautstärke weg** (seit Schritt 3b von v1.5.1): Statt `m_masterVolume` stehen
  `m_musicVolume` und `m_sfxVolume` im Spielstand, beide neu mit 1. Ein alter Stand
  behält seine Gesamtlautstärke als ungelesenen Schlüssel, Musik und Effekte stehen bei
  ihm also wieder auf 100 % (ohne Umwandlung, es gibt nur Test-Spielstände).
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
  Revive hebt das Einkommen pro Run grob um 50-70 % - nach dessen Umbau (v1.9.3)
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
  Münzen, Highscore 0, dem Startschiff, den neuen Sounds, Musik und SFX an und beiden
  Reglern auf 100 %.
- **Die alten PlayerPrefs** liegen in der Registry unter
  `HKCU\Software\Unity\UnityEditor\ANIMO Games\Space Escaper`, daneben noch der
  Schlüssel des App-Namens bis v1.4.4, `Space-Escaper`.

## Shop

Seit v1.4.8 beschreibt ein `ShipData`-Asset pro Schiff, was der Shop wissen muss:
Familie, Preis, Modell und Antriebsflamme. Die Assets liegen in
`Data/Ships/`, der `ShipCatalog` daneben listet alle Schiffe in der Reihenfolge des
Hangars: ARISTOCRAT, FREETER und VAGOR (`ShipFamily`) mit je drei Skins, das
Startschiff vorn. Bis v1.4.7 hing der Shop an Kind-Indizes, mit 9 Kauf-Buttons, 11
Bildern, einer Preisliste im `GameManager` und jedem Modell zweimal in Szene und
Prefab.

- **Der Hangar liest die Daten:** Jeder Skin-Button trägt einen `ShipButton` mit
  seinem `ShipData` und meldet es beim Tipp, der `ShopScreen` zeigt dann dieses Schiff.
  Die Pfeile zeigen das erste Schiff der
  Familie davor oder danach (`ShipCatalog.GetFirstShipOfFamily`). Ein einziger
  Button, `UI/Shop/SafeArea/SelectOrBuyButton`, kauft oder wählt das gezeigte Schiff
  (`SelectOrBuyShownShip`). Seine Komponente `SelectOrBuyButton` zeigt den Preis aus dem
  `ShipData` mit Münze, SELECT oder SELECTED in der Leuchtfarbe (seit v1.5.1, bis dahin
  Bilder). Reichen die Münzen nicht, ist er halb durchsichtig (Canvas Group), ein Tipp
  kauft dann nichts.
- **Was der Hangar sonst zeigt** (seit v1.5.1): die Münzen oben rechts, den Namen der
  Familie über den Skins und an jedem Skin-Button Ecken, solange sein Schiff auf dem
  Ständer steht, und einen Haken, sobald es gekauft ist, grün unter dem geflogenen Schiff,
  hellgrau unter den anderen (Wunsch Moritz, 18.11.2026, `ShipButton.ShowState`). Der
  `ShopScreen` setzt das für alle neun Buttons, auch die der ausgeschalteten Familien, bei
  jedem gezeigten Schiff, jedem Kauf und jeder Wahl.
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
  `Data/Ships/` mit ID, Familie, Preis, Modell, Hangar-Größe und Flamme, das `ShipData` in den `ShipCatalog` eintragen und einen Skin-Button mit
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

**Der Preis ist Text** (seit v1.5.1): Der Shop zeigt `ShipData.Price`, ein neuer Preis
im `ShipData` reicht. Bis dahin waren Preisschilder Bilder (`2026/PriceTag…`, davor die
Originale von v1.3 in `2021/`), das Feld `ShipData.PriceTag` ist weg. Die Bilder sind
seit Schritt 4 von v1.5.1 gelöscht.

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
- **„Play again“ und „Hangar“ auf dem Todesbildschirm** (seit v1.5.1, `PlayAgain` und
  `ReturnToHangar`, nur aus `Dead`) laden die Szene neu wie `ReturnToMenu` und gehen
  dann gleich weiter wie ein Tipp im Hauptmenü: `GameManager.Start` ruft `Play` oder
  `OpenHangar`. Was nach dem Neuladen kommt, merkt sich das statische Feld
  `s_reloadTarget`, es überlebt das Neuladen. In `Start` sind alle `Awake` und `OnEnable`
  der Szene gelaufen und noch kein `Update`, der Run startet also wie aus dem Menü, mit
  dem Schwenk der Kamera hinter das Schiff.
  - **Ladeframes:** Nach „Play again“ fliegt der Run erst ab dem dritten Frame der neuen
    Szene (`GameManager.IsShipFlying`, seit v1.5.2), Schiff, Kamera und Score stehen bis
    dahin. Die ersten beiden Frames bringen die Ladezeit als `Time.deltaTime` mit, gemessen
    im Editor 75 bis 114 ms je Frame, und im zweiten wiederholt Unity den Wert des ersten.
    Auf dem Handy sind es bis zu `Time.maximumDeltaTime` (0,33 s) je Frame. Bis dahin zählte
    der Run beide mit und begann rund 2 Einheiten und 0,2 s weit, sichtbar wurde das erst,
    als die Szene mit den Screens von v1.5.2 länger lud.
- **Jeder Wechsel** zeigt den Screen des Zustands (`ScreenManager.Show(GameState)`) und
  setzt `Time.timeScale`, 0 nur in der Pause.
- **Settings, Credits, Achievements und Collectibles sind kein Zustand,** sondern Screens
  über Hauptmenü oder Pause, Achievements und Collectibles nur über dem Hauptmenü. Der
  Zustand bleibt dabei stehen, „Zurück“ in den Settings, den Achievements und den
  Collectibles zeigt wieder seinen Screen, „Zurück“ in den Credits die Settings.
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
| `RunScore` | `RunStarted` | den Highscore merken, den der Run schlagen kann |
| | `ShipCrashed` | Münzen des Runs gutschreiben, Highscore, speichern |
| `PlayerShip` | `RunStarted` | Flamme an |
| | `ShipCrashed`, `ShipRevived` | Modell und Flamme aus und wieder an |
| `Spaceport` | `RunStarted` | schaltet sich 3 s später aus |
| `Hangar` | `HangarOpened`, `HangarClosed` | Hangar mit Hangar-Kamera oder Hauptkamera (bis v1.4.13 `CameraSwitch`, das Gebäude schaltete der `GameManager`) |
| `TileManager` | `ShipRevived` | neue Abschnitte, nur mit `m_respawnsOnRevive` (die Strecke) |

- **Den Zustand lesen** `PlayerMotor` (fliegt nur in `Run`), `CameraMotor` (folgt in
  `Run`, bremst in `Dead`, steht sonst) und `RunScore` (zählt nur in `Run`), für den Run
  seit v1.5.2 über `GameManager.IsShipFlying`, das die Ladeframes von „Play again“
  auslässt (siehe oben). Dazu die Zurück-Buttons im `SettingsScreen`, `AchievementsScreen`
  und `CollectiblesScreen` und `RunTests`. Die übrigen Screens brauchen ihn nicht: Der `ScreenManager` schaltet sie
  ein, und sie zeigen beim Einschalten, was gerade gilt (siehe „UI“).
- **Münzen** melden sich über das statische Ereignis `Coin.Collected` (seit v1.5.0,
  bis dahin `GameManager.CollectCoin`), `RunScore` zählt sie. Den Multiplikator rechnet
  `RunScore` aus dem Tempo (`PlayerMotor.SpeedGain`), bis v1.4.13 schickte der
  `PlayerMotor` ihn bei jeder Tempo-Stufe.
- **Neues, das auf den Spielverlauf reagiert,** meldet sich an einem Ereignis an (in
  `OnEnable`, ab in `OnDisable`) oder liest `State`, statt einen eigenen Schalter zu
  führen. Was mit dem Schiff mitläuft und `Time.deltaTime` zählt, liest `IsShipFlying`. Ein neuer Übergang ist eine Methode im `GameManager`: Zustand prüfen,
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
(die Anzeige im Run), `PauseMenu`, `Settings`, `Credits`, `Achievements` und
`Collectibles` (alle drei seit v1.5.2), `Shop` und `DeathMenu`. Der Root-Canvas
`UI` trägt den `CanvasScaler` (Referenz 450 × 800, „Expand“, siehe „Layout für alle
Seitenverhältnisse“) und den `ScreenManager`. Bis v1.4.9 lagen alle Screens auf einem
Canvas, und sechs Animatoren schoben die gerade nicht gebrauchten aus dem Bild.

- **Es ist immer genau ein Screen an.** `ScreenManager.Show(GameState)` zeigt den
  Screen des Spielzustands (siehe „Spielzustand“), `ShowSettings()` die Settings,
  `ShowCredits()` die Credits, `ShowAchievements()` die Achievements und
  `ShowCollectibles()` die Collectibles. Alle schalten ihn
  per `SetActive` ein und alle anderen aus,
  ohne Übergang (Entscheidung Oktober 2026, Übergänge kommen mit v1.5.4, siehe README).
  Beim Start zeigt er das Hauptmenü, egal welcher Screen beim Bearbeiten an blieb.
  Gespeichert ist die Szene mit nur dem Hauptmenü an, so sieht sie im Editor aus wie beim
  Start.
  Zum Bearbeiten einen Screen einschalten, vor dem Speichern wieder nur das Hauptmenü.
  Das prüft seit v1.5.0 `SceneTests`: Ein anderer Screen wachte sonst mit der Szene
  auf, vielleicht bevor bereit ist, was er zeigt, etwa der `ShopScreen` vor dem
  `PlayerShip`, und erst danach schaltete der `ScreenManager` ihn aus.
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
  setzte der `GameManager` alle Texte): Hauptmenü (Münzen, Highscore), Pause (Zeit,
  Score, Münzen, Highscore und Strecke des Runs, seit v1.5.1) und Todesbildschirm (Zeit,
  Score, „NEW HIGHSCORE“, Münzen, Strecke, Revive-Button nur, solange der Run ihn noch hat)
  setzen ihre Texte beim Einschalten, aus dem Spielstand und dem `RunScore`. Die
  Spiel-UI folgt dem `RunScore` in `LateUpdate`, also mit dem Score dieses Frames, und
  setzt einen Text nur, wenn sich seine Zahl ändert. Steigen die Münzen, poppt das
  Icon. Der `ShopScreen` führt den Shop selbst (siehe „Shop“).
- **Zeit, Strecke und Highscore des Runs** zählt der `RunScore` (seit v1.5.1): die Zeit
  nur im Run, die Strecke als Tempo mal Frame-Zeit statt aus der Position, die der
  Floating Origin zurückrückt (eine Einheit ist ein Meter), und den Highscore vom Start
  des Runs. Den braucht „NEW HIGHSCORE“, denn ab dem ersten Crash steht im Spielstand
  schon der Score dieses Runs. Der allererste Run hat keinen Highscore zu schlagen.
- **Einblendungen im Run** (seit v1.5.1): Alle 100 Punkte zeigt die Spiel-UI 2,5 s lang
  die Leiste bis zum Highscore (`m_highscoreBarStep` im Inspector), hat der Run ihn
  geschlagen, einmal „NEW HIGHSCORE!“. Im ersten Run nach dem Start der App steht „SWIPE
  TO CHANGE LANES“ über dem Schiff, bis zum ersten Wischer oder 6 s lang. Die Zeiten
  laufen über `Time.time`, sie stehen in der Pause also still. Gespeichert sind alle drei
  ausgeschaltet, `LayoutTests` prüft sie trotzdem, er schaltet in seiner Kopie alles ein.
- **Credits** (seit v1.5.2, Vorlage Version 6 der Mockups): geöffnet über die Zeile
  „Credits“ unter INFO in den Settings, die ganze Zeile ist der Button, ihr Label dunkelt
  beim Tippen ab wie der Schalter der Zeilen darüber. „Zurück“ zeigt die Settings. Der Kopf
  ist der der Settings, darunter rollt `CreditsRoll` den Inhalt mit 60 Einheiten pro Sekunde
  durch einen Ausschnitt (`Credits/SafeArea/Roll`), in unskalierter Zeit, denn die Settings
  öffnen auch aus der Pause. Die Rolle beginnt unter dem Ausschnitt und dort wieder, sobald
  alles oben hinaus ist, auf 20:9 nach rund 68 s.
  - **Wischen:** Der Ausschnitt ist eine `ScrollRect` ohne Grenzen (Unrestricted). Ein
    Wischer schiebt die Rolle, solange der Finger liegt, steht sie, danach läuft sie von dort
    weiter. Springt sie, ist nichts von ihr im Ausschnitt: vom Ende an den Anfang und nach
    einem Zug über den Anfang hinaus zurück auf ihn.
  - **Weiche Ränder:** Die `RectMask2D` des Ausschnitts blendet oben und unten aus. Unity
    blendet über die halbe `softness` aus, 190 ergeben also 95 Einheiten, im Mockup sind es
    Verläufe von 80 und 110. Mit 80 wie im Mockup war das Ausblenden kaum zu sehen.
  - **Inhalt** (Moritz, 18.11.2026): Logo, „A GAME BY ANIMO GAMES“, die Abschnitte CURRENT
    TEAM, PREVIOUS CONTRIBUTORS, THIRD-PARTY ASSETS und MADE WITH, je Eintrag Name und
    Rolle, dann die Lizenzen und „THANK YOU FOR PLAYING“ mit dem Schiff. Die Abschnitte
    stehen genauso oben in der README, eine Änderung braucht also beide (`CreditsTests`).
    Liberation Sans steht dabei, weil TextMesh Pro mit ihr fehlende Zeichen zeichnet (siehe
    „Texte“), die Sounds von 2020 brauchen vorerst keine Nennung (Moritz, 18.11.2026).
  - **Lizenzen:** die vollen Texte, die die Schriften (SIL Open Font License) und die Symbole
    (Feather: MIT, Lucide: ISC) in der App verlangen, mit den Copyright-Zeilen der vier
    Schriften. Sie stehen nur im Spiel und in den Dateien neben Schriften und Symbolen, je
    ein Text pro Absatz, so lässt die Maske die Absätze außerhalb des Ausschnitts weg. Gebaut
    wurden sie aus genau diesen Dateien. Von der Lizenz von Lucide gilt nur der ISC-Teil, die
    angehängte MIT-Lizenz betrifft die Symbole, die Lucide von Feather übernommen hat,
    Pokal und Blatt sind nicht darunter.
  - **Gemessen** im Editor: Draw Calls nur des UI 4 bis 5, je nach Stelle der Rolle. Der
    Canvas baut beim Rollen in jedem Frame neu, im Mittel 0,006 ms.
- **Achievements** (seit v1.5.2, Vorlage Version 6 der Mockups): geöffnet über die Medaille
  neben LAUNCH im Hauptmenü, „Zurück“ zeigt den Screen des Zustands, also das Hauptmenü. Bis
  die Achievements selbst kommen (gleich nach den Screens von v1.5.2, Moritz, 19.11.2026),
  zeigt der Screen die sechs Beispiele der Mockups in
  ihren Zuständen dort, und nichts darauf tut etwas (Moritz, 19.11.2026: „sample achievments
  einbauen, die noch nicht funktionieren müssen“). Sie stehen fest in der Szene, kein Code
  füllt sie. Die Zahl der abholbaren Belohnungen an der Medaille kommt erst mit den
  Achievements.
  - **Kopf:** der der Settings, der lange Titel wie im Mockup in 20 statt 24. Rechts zeigt ein
    Ring den Anteil, der erledigt ist (`Achievements/SafeArea/Progress`): `ProgressRing` (4
    Pixel pro Einheit) in 16 % Weiß und darüber dasselbe Bild in Grün, radial gefüllt im
    Uhrzeigersinn von oben (`Image.Type.Filled`). Die Füllung endet gerade, zwei kleine
    `Plate` geben ihr die runden Enden des Mockups, die zweite sitzt am Ende der Füllung.
  - **Liste:** `Achievements/SafeArea/Scroll` wie die Liste der Settings, eine senkrechte
    `ScrollRect` auf „Clamped“ mit `RectMask2D`, 114 unter der Safe Area bis an ihren unteren
    Rand, nach dem letzten Eintrag 24 Luft. Auf 20:9 passt sie, auf 16:9 und den Tablets lässt
    sie sich 44 Einheiten ziehen.
  - **Einträge** in den drei Zuständen der Mockups: abholbar in Bernstein mit dem grünen
    CLAIM-Button darunter (COIN COLLECTOR), unterwegs mit der Belohnung rechts und darunter
    der Leiste mit dem Stand (SPEED DEMON, LONG HAUL, ARCHIVIST), erledigt mit grünem Haken
    (FIRST FLIGHT, NEW WINGS). Je Eintrag eine Layout-Gruppe, Grund und Rahmen liegen als
    `Background` und `Border` außerhalb des Layouts wie in den Settings (siehe „Maße aus den
    Mockups“). CLAIM nimmt keine Tipps an und behält sein Aussehen (nicht `interactable`,
    ohne Übergang).
  - **Gemessen** im Editor: Draw Calls nur des UI 6, der Canvas baut nicht neu, solange die
    Liste steht.
- **Collectibles** (seit v1.5.2, Layout aus Version 6 der Mockups): geöffnet über das Paket
  neben LAUNCH im Hauptmenü, „Zurück“ zeigt den Screen des Zustands, also das Hauptmenü. Die
  Collectibles selbst kommen erst später (Moritz, 19.11.2026: „keine actuals ... sonder nur so
  platzhalter aalso ausgegrautet boxen mit soon“), der Screen zeigt deshalb nur ihre Plätze.
  Der Kopf ist der der Achievements, sein Ring leer, „0%“ ausgegraut. Darunter je Abschnitt
  (LOGBOOKS, STAR-SYSTEM CARDS, PLANTS) Symbol, Name und „0 / 5“, „0 / 3“ oder „0 / 2“,
  ausgegraut wie alles, was später kommt. Die Karten sind drei statt zwei wie im Mockup und
  stehen über den Plants statt darunter (Moritz, 19.11.2026). Je Abschnitt folgt eine Reihe
  Kacheln, eine pro Collectible: wie die eines noch nicht gefundenen im Mockup, mit Schloss,
  aber ohne „Not found“ darunter, und mit „SOON“ an der Ecke, wo das Mockup „NEW“ zeigt.
  Außer Zurück nimmt nichts Tipps an. Die Abschnitte passen auf jedes Display, eine Liste zum
  Scrollen braucht es nicht. Gemessen im Editor: Draw Calls nur des UI 3, der Canvas baut nie
  neu.
- **Texte sind TextMesh Pro** (`TextMeshProUGUI`, seit v1.5.1, bis dahin Legacy-`Text`).
  Die Screens halten sie als `TMP_Text` und schreiben Zahlen mit `SetText("{0}", zahl)`:
  TextMesh Pro formatiert dann in einen eigenen Puffer, ohne neuen String. Nur im Editor
  legt es dabei zusätzlich einen String für den Inspector an (`#if UNITY_EDITOR` im
  Paket), Allokationen also auf dem Handy messen, nicht im Editor. Kommazahlen bekommen
  dort immer einen Punkt, egal welche Sprache das Handy hat. Ein neuer Legacy-`Text`
  fällt in `SceneTests` auf.
  - **Schriften** liegen als Font Asset neben ihrer Schriftdatei und deren Lizenztext
    in `UI/Fonts/` (seit Schritt 3 von v1.5.1): `TitleSDF` (Audiowide) für Titel,
    Buttons und Zahlen, `TextSDF` und `TextBoldSDF` (Exo 2 Regular und Bold) für kleine
    Texte, `TextSDF` verweist für Fett auf `TextBoldSDF`. Audiowide heißt als Font Asset
    neutral: Die Schrift hat einen Reserved Font Name, und ein Font Asset gilt nach der
    SIL Open Font License als geänderte Fassung, die nicht unter diesem Namen angeboten
    werden darf. Alle drei sind statisch und haben 117 Zeichen für Englisch und Deutsch:
    ASCII, Umlaute und ß, deutsche und englische Anführungszeichen, `…`, `·`, `×`, `©`,
    `®`, `°`, `€`, `•` und das geschützte Leerzeichen, Exo 2 dazu das große ẞ. Gerendert
    mit SDF16 aus Sampling-Größe 90 und Padding 9 in einen Atlas von 1024 × 1024, Shader
    `TextMeshPro/Mobile/Distance Field`, ohne Kerning: Die Schriften brächten 5.000 bis
    12.600 Paare mit, die Assets wären 4 bis 8 statt 2 MB groß, und in den Wörtern der
    Screens verschiebt es höchstens 6 % eines Buchstabens. Der Atlas ist nicht lesbar,
    liegt also nur auf der GPU, und die Schriftdatei selbst kommt nicht mehr in den Build.
    Ein fehlendes Zeichen zeichnet TextMesh Pro aus `LiberationSans SDF`, der
    Standardschrift in den TMP Settings, so das große ẞ in Audiowide. `TitleSDFShadow`
    ist ein Material-Preset mit Schatten (Underlay) für den Score im Run. Neuropol X und
    Good Times, die Schriften bis v1.5.0, sind seit Schritt 4 von v1.5.1 samt Font Assets
    gelöscht, ihre kostenlose Lizenz deckt keine Apps ab.
  - **Buchstabenabstand wie in den Mockups:** `characterSpacing` zählt in Hundertsteln
    der Schriftgröße, also 100 × Abstand / Größe. Dazu kommt ein rechter Innenrand
    (`margin`) von einem Abstand: Der Browser setzt ihn auch nach dem letzten Buchstaben,
    TextMesh Pro lässt ihn aus der bevorzugten Breite, und in einer Layout-Gruppe ragte
    der Text sonst um einen Abstand aus seinem Rechteck.
  - **Ein Zeichen dazunehmen:** im Inspector des Font Assets „Update Atlas Texture“.
    Das öffnet den Font Asset Creator mit den gespeicherten Einstellungen. Dort das
    Zeichen an die Custom Characters hängen, „Generate Font Atlas“ und über das
    bestehende Asset speichern, so bleiben GUID und alle Verweise.
  - **Überlauf:** Die Texte stehen auf „Overflow“. Bei „Truncate“ verschwindet eine
    Zeile ganz, sobald sie nach den Zeilenmaßen von TextMesh Pro nicht in ihr Rechteck
    passt. Ob ein Text überläuft, meldet `TMP_Text.isTextOverflowing`.
  - **Umgestellt in v1.5.1:** Alle 9 Texte stehen an derselben Stelle und sind gleich
    groß, gemessen im Play Mode an den Glyphen (Mitten höchstens 0,5 Einheiten
    verschoben). TextMesh Pro setzt die Laufweiten ungerundet, „Volume“ wurde dadurch
    3 Einheiten breiter, und „Use new Sound“ brauchte 217 Einheiten, in seinem Rechteck
    von 214 brach es um. Sein Label reicht deshalb bis an den rechten Rand des Toggles
    (224). `ModifierText` lag bei x 472 außerhalb des Bildes (gefunden in v1.4.10).
    Seit Schritt 3a steht er als Multiplikator „×1.4“ in einem Chip neben dem Score,
    `LayoutTests` nimmt ihn nicht mehr aus.
- **Bilder liegen im Sprite Atlas** `UI/UIAtlas.spriteatlasv2` (seit v1.5.1): alle
  UI-Bilder, die die Szene nutzt, seit v1.5.2 47 auf einer Seite von 2048 × 1024 in
  ETC2, 2,0 MiB. Beim Einführen waren es 36 alte Bilder auf 2048 × 2048, 4,0 MiB.
  Einzeln lagen 35 davon unkomprimiert im Speicher, zusammen 10,7 MiB (siehe
  „Texturen“). Die Bilder eines Screens kommen damit in einem Draw Call, gemessen im
  Editor nur fürs UI, beim Einführen: Menü 2 statt 7, Hangar 1 statt 7, Settings 6 statt
  9, Run 3 statt 4, Pause 2 statt 5, Tod 3 statt 6. Mit dem neuen Design, gemessen zum
  Abschluss von v1.5.1: Menü 4, Hangar 4, Settings 8, Run 4 (mit der Leiste zum Highscore
  7, mit „NEW HIGHSCORE!“ 6), Pause 3, Tod 3. In v1.5.2 kamen die Credits mit 4 bis 5 dazu,
  je nach Stelle der Rolle, die Achievements mit 6 und die Collectibles mit 3, das Menü hat
  ohne seine beiden „SOON“ nur noch 3. Seit Schritt 3a sind alle Wörter Text, und eigene
  Draw Calls sind die Texte (ein Material pro Schrift) und was kein Bild aus dem Atlas zeigt.
  - **Ein neues Bild** kommt einzeln in den Atlas (im Inspector unter „Objects for
    Packing“), ein nicht mehr genutztes heraus. Keine Ordner: In `2020/` liegen
    ungenutzte Bilder für den Classic Mode, die er mitpacken würde. Was fehlt oder zu viel ist,
    meldet `SpriteAtlasTests`.
  - **Einstellungen:** nicht drehen und nicht eng packen, denn ein UI-Bild zeigt das
    Rechteck seines Sprites (ein neuer Atlas hat beides an). 4 Pixel Abstand, so teilen
    sich zwei Sprites nie einen 4×4-Block von ETC2. Alpha Dilation an, ohne Mipmaps,
    Kompressorqualität „Best“. Durchsichtige Ränder schneidet Unity beim Packen ab
    (648×207 wird 634×193), das UI gleicht das beim Zeichnen aus.
  - **Qualität:** ETC2 ist verlustbehaftet, einzeln waren die Bilder verlustfrei.
    Gemessen an den Pixeln, die die GPU bekommt: im Schnitt 0,6 von 255 Stufen
    Abweichung, 0,5 % der Pixel weichen um mehr als 16 ab. Titel und Buttons sind
    praktisch gleich. Sichtbar wird es nur an den neun Schiffsbildern im Hangar, und erst
    vierfach vergrößert: leichte Treppen an weichen Kanten. „Best“ statt „Normal“ senkte
    die Abweichung nur von 0,77 auf 0,63 und kostet allein beim Packen Zeit.
  - **Packen:** Der Sprite Packer steht auf „Sprite Atlas V2 - Enabled“ (Project
    Settings > Editor, bis v1.5.0 V1 „Enabled for Builds“, ohne Atlas). Unity packt damit
    auch im Editor, der Play Mode zeigt den Atlas wie das Handy. Neu gepackt wird nur,
    wenn sich der Atlas oder eines seiner Bilder ändert, mit „Best“ in gut 2 s. Beim
    Start des Play Mode packt Unity nicht neu, er dauert 0,1 s.
- **Layout für alle Seitenverhältnisse** (seit v1.5.1): Der `CanvasScaler` steht auf
  „Expand“ mit Referenz 450 × 800. Das UI ist damit nie schmaler als 450 und nie
  niedriger als 800 Einheiten: Handys, die schmaler sind als 16:9, bekommen mehr Höhe
  (20:9: 450 × 1000), Tablets mehr Breite (4:3: 600 × 800). Bis v1.5.0 skalierte er nach
  der Höhe (Referenz 800 × 800), das UI war immer 800 hoch und auf schmalen Handys nur
  noch 360 (20:9) bis 289 Einheiten breit (Außendisplay eines Klapphandys). Auf dem
  Pixel 9 Pro waren „SETTINGS“ und „PAUSED“ deshalb links angeschnitten, im Hangar
  standen „EXIT“ und die Pfeile am Rand. Auf 16:9 sieht alles aus wie vorher (Bilder
  aller Screens in 1080 × 1920 verglichen), auf schmaleren Handys ist das UI kleiner als
  bis v1.5.0, auf 20:9 um 20 %.
  - **Jedes Element hängt an dem, wozu es gehört.** Was an einer Kante steht, liegt in
    der `SafeArea` seines Screens und hängt an seiner Ecke, oben 24 Einheiten unter ihr
    (seit Schritt 3a von v1.5.1): Highscore und Münzen oben links, Zahnrad und Pause
    oben rechts, Zurück oben links und die Titel oben mittig, LAUNCH, Hangar-Button,
    Kaufen und die Buttons des Todesbildschirms unten, das Sheet der Pause am unteren
    Rand. Die Abschnitte der Settings sowie Titel und Score des Todesbildschirms hängen
    oben. Was über der 3D-Welt liegt, hängt direkt am Screen, an einem Anker auf seiner
    Höhe relativ zum Bildschirm: das Logo über dem Raumhafen, Skins und Pfeile im
    Hangar. Die Kameras halten ihr senkrechtes
    Sichtfeld (75° und 60°), die Welt wächst also mit der Bildhöhe, und diese Elemente
    bleiben über derselben Stelle. Die Skins liegen dafür bei y = 0 in ihren Familien,
    ihr Container hängt auf ihrer Höhe.
  - **Safe Area:** Das Spiel zeichnet bis unter das Kameraloch („Render outside safe
    area“ in den Player Settings), das bleibt so. Die Komponente `SafeArea` zieht das
    gleichnamige Kind jedes Screens auf `Screen.safeArea`: beim Einschalten und, solange
    der Screen gezeigt wird, sobald sich Safe Area oder Bildschirmgröße ändern
    (Klapphandy, Gerätewechsel im Device Simulator). Hintergründe, die den ganzen
    Bildschirm decken, liegen daneben und reichen unter die Kamera. Außerhalb des Play
    Mode tut sie nichts, gespeichert ist die `SafeArea` über den ganzen Screen. Sie ist
    das letzte Kind ihres Screens, so liegen die Buttons an den Kanten über den
    Elementen der 3D-Welt, wie vorher.
  - **Tippflächen** sind mindestens 60 Einheiten groß: 48 dp, wie Android sie
    empfiehlt, auf einem Handy mit 360 dp Breite, so schmal sind die schmalsten
    verbreiteten. Auf dem Außendisplay eines Klapphandys (320 dp) bleiben davon 43 dp.
    Die kleinen Buttons (Zahnrad, Pause, Zurück, Prefab `IconButton`) zeigen 60 × 60 und
    nehmen Tipps über ein unsichtbares Kind `HitArea` auf 68 × 68 an, das sind auch dort
    48 dp. Die Zeilen mit Schalter sind 60 hoch, die Regler nehmen Tipps über ihre
    `HitArea` auf 60 an, auch wo sie im Layout nur 44 einnehmen (Settings).
  - **Die Settings passen ohne Scrollen** (seit Schritt 3b von v1.5.1, Wunsch Moritz):
    Mit den Abständen von Version 6 waren sie 750 Einheiten hoch, unter dem Titel bleiben
    auf 16:9 und Tablets aber nur 676 bis 700. Mit weniger Luft sind sie 633 hoch, mit
    „Invincible“ in Editor und Test-Builds 693, das passt auch in ein Game-Fenster in 9:16.
    Weniger Luft heißt: 16 statt 28 unter dem Zurück-Button, die Abschnitte 10 statt 16
    auseinander und innen mit weniger Rand, die Zeilen von GAME direkt aufeinander, die
    Versionszeile 44 statt 60 hoch (sie nimmt keine Tipps an). Die Regler nehmen im Layout
    44 statt 60, ihre `HitArea` und der Weg ihres Knopfs (`HandleSlideArea`) bleiben 60 hoch
    und reichen 8 über sie hinaus. Schriften und Tippflächen sind so groß wie vorher. Das
    prüft `LayoutTests.TheSettingsFitWithoutScrolling` auf allen zehn Displays, ohne
    „Invincible“, die Zeile entsteht erst im Spiel. Eine neue Zeile braucht also Platz von
    anderswo. Die Abschnitte liegen trotzdem in `Settings/SafeArea/Scroll`, einer
    senkrechten `ScrollRect` mit `RectMask2D` bis an den unteren Rand der Safe Area, auf
    „Clamped“: Eine Liste, die passt, bewegt sich nicht, nur mit „Invincible“ auf dem
    Tablet mit Balken unten (676 Platz) scrollt sie ein Stück. Ein unsichtbares Bild
    darauf nimmt die Wischer zwischen den Bedienelementen an.
  - **Ein neues Element** kommt nach diesen Regeln in die `SafeArea` oder direkt an den
    Screen. Ob es auf allen Displays passt, prüft `LayoutTests` (siehe „Tests“). Ansehen
    im Device Simulator (Window > General > Device Simulator), er hat Handys mit
    Kameraloch, ein Klapphandy und zwei Tablets. Auf dem Handy ist das neue Layout noch
    nicht nachgesehen.
- **Look aus den Mockups** (seit Schritt 3 von v1.5.1, Vorlage ist Version 6 der
  Mockups): Grund `#050607`, Leuchtfarbe `#57ff5c` mit Schatten `#2b7f2e` und Text
  `#062b08` darauf, Sternenlicht `#ffad3d`, weiße Platten mit Text `#1b1a20` und
  Schatten `#6f7380`. Das ist das Farbschema „Giftig“: In v1.5.3 sollen sich sechs
  Farbschemata in den Settings wählen lassen (README), bis dahin stehen die Farben
  fest an jedem Element.
  - **Platten** sind ein einziges 9-Slice-Bild, `2026/Plate` (ein weißer Kreis mit 64
    Pixel Rand): Der Radius ist 64 / `pixelsPerUnitMultiplier`. Mit großem Faktor
    zeichnet es auch Rechtecke aus dem Atlas, etwa die Abdunklungen, so bleiben sie im
    Draw Call des Atlas. Rahmen sind `PlateOutline` (2 Einheiten bei Radius 18) und
    `PlateOutlineThin` (1 Einheit), ein Schatten ist eine zweite Platte 4 rechts und 6
    tiefer.
  - **Prefabs** in `Prefabs/UI/`: `Button` (weiße Platte mit Symbol und Text),
    `PrimaryButton` (Variante in der Leuchtfarbe), `IconButton` (siehe Tippflächen),
    `OnOffButton` (an oder aus mit Zustand, Komponente `OnOffButton`) und `SwitchRow`
    (Titel, Beschreibung und Schalter, die ganze Zeile ist das Toggle, Komponente
    `ToggleSwitch`). Text und Symbol bekommt jede Instanz selbst.
  - **Transparenz im linearen Farbraum:** Das Projekt rendert linear, Unity mischt
    halbtransparentes UI deshalb im linearen Licht, der Browser der Mockups in sRGB: 7 %
    Weiß über Schwarz sieht hier aus wie 30 %. Halbtransparente Farben bekommen deshalb
    das Alpha, das über ihrem Untergrund aussieht wie im Mockup, etwa 0,014 statt 0,07
    für die Kacheln und 0,90 statt 0,72 für die Abdunklung des Todesbildschirms. Die
    weichen Ränder von Logo und Leuchten (`SpaceEscaperLogo`, `PlateGlow`, `PlateGlowAmber`)
    sind pixelweise so umgerechnet, für dunklen Weltraum als Untergrund. Der Untergrund
    zählt: Die Einblendungen im Run liegen vor dem dunklen Himmel, mit dem Grau der Welt
    gerechnet sah die Füllung von „NEW HIGHSCORE!“ viel zu orange aus.
  - **Leuchten:** `PlateGlow` ist gefüllt und für das Grün von LAUNCH gebacken (60 %),
    `PlateGlowAmber` (seit Schritt 3b) leuchtet nur außerhalb der Platte, wie ein
    `box-shadow` im Browser, gebacken für Bernstein mit 45 %. Unter einer durchsichtigen
    Platte wie der Leiste zum Highscore schiene ein gefülltes Leuchten durch. Beide halten
    einen Schein von 30 Einheiten um eine Platte mit Radius 20 bei Faktor 2: Faktor
    40 / Radius passt ihn an eine Platte an, er reicht dann 1,5 Radien hinaus.
  - **Was erst später kommt, ist ausgegraut** (seit Schritt 3b): Vibration, Sprache und
    Datenschutz in den Settings und der Inhalt der Collectibles (siehe oben), bis zu ihren
    Screens in v1.5.2 auch Credits, Achievements und Collectibles.
    Ausgegraut heißt feste Farben, wie CSS `opacity: 0.4` auf dem Untergrund aussähe, keine
    Canvas Group: Mit halber Deckkraft scheinen übereinanderliegende Teile durcheinander
    durch, etwa der Schatten durch die Platte. Daneben steht ein graues „SOON“, nicht
    ausgegraut, über einem Button mit einem Ring in der Farbe des Grunds wie die Zahlen der
    Mockups. Die Buttons sind nicht `interactable`, ohne Übergang.
  - **Maße aus den Mockups:** Die Mockups setzen Texte meist mit normaler Zeilenhöhe,
    Audiowide mit 1,275 Schriftgrößen, Exo 2 mit 1,2. Ein Text von Exo 2 hat seine
    Grundlinie deshalb 0,1 Schriftgrößen tiefer als in einem Kasten mit Zeilenhöhe 1. Das
    Bauen der Screens rechnet mit Kästen der Zeilenhöhe 1, Exo 2 rückt dort um 0,1
    Schriftgrößen nach unten. Absolut gesetzte Kinder eines Kastens mit Rand zählen in CSS
    ab dessen Innenkante, im Sheet der Pause also 2 Einheiten tiefer.
  - **In Layout-Gruppen** bekommt ein Text die Höhe, die TextMesh Pro für ihn will, eine
    Zeile so hoch wie die normale Zeilenhöhe im Browser. Oben ausgerichtet (`Top`) sitzt er
    dann wie dort, „Midline“ zentriert stattdessen seine Glyphen: Die Zahlen der Achievements
    saßen damit eine halbe Einheit zu hoch (gesehen in v1.5.2). Ein Bild als Grund einer
    Layout-Gruppe liegt als Kind außerhalb des Layouts (`LayoutElement` mit „Ignore Layout“):
    Ein 9-Slice-Bild meldet seine Ränder als Wunschgröße, ohne den Faktor
    `pixelsPerUnitMultiplier`, bei `Plate` 122 Einheiten. Auf demselben Objekt wie die Gruppe
    gilt der größere Wert, ein Eintrag der Achievements war so erst 122 statt 82 hoch.
  - **Bilder** entstehen aus HTML im Browser: Die Symbole sind die SVGs der Mockups
    (Feather und Lucide, ein paar eigene) als weiße PNGs von 128 Pixeln, im Spiel
    eingefärbt. Das Logo ist Bruno Ace SC als Bild mit 3 Pixeln pro Einheit, die Schrift
    selbst liegt nicht im Projekt. `SkinFrame`, die Ecken um den gezeigten Skin, ist ein
    9-Slice mit 2 Pixeln pro Einheit. Nur die Münze und das Schiff der Credits sind kein
    HTML: Die Münze ist die rostige Mutter von 2020, enger zugeschnitten (siehe „Texturen“),
    ihr Rechteck zeigt also die ganze Mutter. Das Schiff (`CreditsShip`, seit v1.5.2) ist das
    Bild von Aristocrat Skin 1 aus `2021/Shop/` ohne seine Platte, 122 × 124 Pixel.
- **Ein Canvas pro Screen,** damit eine Änderung nur ihren eigenen Screen neu baut. Im
  Run baut der Canvas nur noch neu, wenn sich Score oder Münzen ändern (gemessen in 2
  von 30 Frames, vor Schritt 3b) und seit v1.5.1, wenn eine Einblendung kommt oder geht,
  in Pause, Tod und Hangar gar nicht, in den Settings beim Ziehen eines Reglers, in den
  Credits in jedem Frame, solange sie rollen, in den Achievements nur beim Ziehen der Liste,
  in den Collectibles nie. Bis
  v1.4.9 baute der ganze UI-Canvas in jedem Frame neu, außer in der Pause. Die
  Screen-Canvases erben
  Render Mode und Skalierung vom Root. Ihr eigener Render Mode steht trotzdem auf
  Overlay, falls ein Screen je aus `UI` herauswandert.
- **Jeder Screen braucht seinen eigenen `GraphicRaycaster`.** Ein Raycaster trifft nur
  die Grafiken seines Canvas, ohne ihn reagiert kein Button des Screens. Der Root hat
  seit v1.4.10 keinen mehr, direkt unter ihm liegt nichts Klickbares.
- **Ein neuer Screen:** Objekt unter `UI`, auf den ganzen Canvas gestreckt (Anker 0
  bis 1, Abstände 0), mit `Canvas`, `GraphicRaycaster` und seiner eigenen
  `…Screen`-Komponente für die Buttons, als letztes Kind eine `SafeArea` (ebenso
  gestreckt, mit der gleichnamigen Komponente) für alles Bedienbare. Im `ScreenManager`
  bekommt er ein Feld und einen Platz in `m_screens`, dazu einen Fall in `GetScreen`, wenn er
  zu einem Zustand gehört, sonst eine eigene `Show…`-Methode wie `ShowSettings`. Dass
  Canvas, Raycaster und `m_screens` stimmen, prüft `SceneTests`.
- **Keine Animatoren für Übergänge oder kurze Effekte.** Ein Animator am UI markiert
  seine Grafiken in jedem Frame neu, auch wenn sich nichts bewegt, und dann baut der
  ganze Canvas neu. In einem Screen fällt er außerdem bei jedem Einschalten auf seinen
  Startzustand zurück. Seit v1.5.1 hat das UI keinen Animator mehr: Das Leuchten um
  LAUNCH pulsiert per `PulseEffect` über das Alpha seines `CanvasRenderer`, die Farbe der
  Grafik bleibt dabei unberührt. Kurze Effekte laufen ebenso im Code, so der Münz-Pop:
  `PopEffect` an der Münze der Spielanzeige (`GameMenu/SafeArea/Hud/CoinRow/CoinIcon`)
  vergrößert sie für 0,25 s auf 115 % und färbt sie gelb. Bis v1.4.9 lief er über einen
  Animator, der das Icon nach der ersten Münze von 55 auf 65 wachsen ließ. Danach war 65
  die Ruhegröße, seit Schritt 3a von v1.5.1 sind es 28.
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
  Raycasting. Ein Screen, der im selben Frame eingeschaltet wurde, nimmt noch keine
  Raycasts an, seine Grafiken haben bis zum ersten Zeichnen keine Tiefe (`depth` -1),
  also einen Frame später prüfen. Neu gebaute Canvases zählt ein `ProfilerRecorder` auf
  `Canvas.BuildBatch`.
  So in v1.4.10: 16 Zustände gleich, alle 53 Klicks kommen an. Zum Abschluss von v1.5.1
  kamen alle 41 Tipps eines Durchlaufs durch alle Zustände an, nach jedem Wechsel des
  Screens in einem neuen Aufruf von `script eval`. In v1.5.2 prüfte die Achievements ein
  Durchlauf mit 6 Tipps aus dem Hauptmenü (einer auf CLAIM) und einem Wischer, die
  Collectibles einer mit 6 Tipps (ein Finger auf einer Kachel trifft nichts), die Credits
  einer mit 11 Tipps aus Hauptmenü und Pause, dem Neubeginn am Ende und einem Wischer:
  `ExecuteEvents` mit `initializePotentialDrag`, `beginDragHandler`, `dragHandler` und
  `endDragHandler` auf dem Ausschnitt, die Strecke in Pixeln ist die in Einheiten mal
  `scaleFactor` des Root-Canvas. Texte vergleicht man an
  ihren Glyphen: TextMesh Pro rechnet sie aus `textInfo.characterInfo` (Ursprung,
  Grundlinie, Skalierung und Maße der Glyphe, die Eckpunkte dort enthalten das Padding
  des Atlas), ein Legacy-`Text` hatte sie in `cachedTextGenerator.verts`. Draw Calls
  nur des UI: im Play Mode alle Kameras auf `cullingMask` 0 mit einfarbigem Hintergrund
  stellen und je Screen `UnityStats.drawCalls` lesen (`batches` hat Unity 6.6 dort nicht),
  so in v1.5.1 für den Atlas und zum Abschluss. Leiste und Banner im Run schaltet die
  Spielanzeige gleich wieder aus, wenn nur der Screen gezeigt wird: Für die Messung ihre
  Endzeit im `GameMenuScreen` vorstellen, bei stehender Zeit bleiben sie dann an.
  Andere Seitenverhältnisse: das Game-Fenster im Play Mode auf eine feste Auflösung
  stellen (siehe „Bilder vorher und nachher“ unter „Render Pipeline: URP“) oder auf den
  Device Simulator umschalten, nur dort gibt es eine Safe Area. Aus `eval` geht beides
  per Reflection: `GameView.SetCustomResolution`, `PlayModeView.SwapMainWindow`
  (Game-Fenster zum Simulator und zurück, wie das Dropdown im Reiter) und
  `DeviceSimulatorMain.deviceIndex`, auch im laufenden Play Mode. `ScreenCapture` nimmt
  dort in der Auflösung des Geräts auf. Vorsicht beim Vergleichen: Wechselt die
  Auflösung, während ein Screen an ist, behalten seine Texte die Kantenglättung der
  alten Auflösung, bis sich das Layout um sie ändert. Aus- und Einschalten reicht nicht
  (gesehen in v1.5.1 im Hauptmenü, die Zahlen wurden weich). Erst die Auflösung setzen,
  dann den Screen zeigen.

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
  man den Musikregler, wenn man die Settings aus der Pause öffnet. Beim Tod
  pausiert sie dagegen, siehe „Audio-System“.
- Die Pause zeigt den Run bisher (Zeit, Score, Münzen, Highscore, Strecke) und hat die
  Schalter für Musik und Effekte (seit v1.5.1, bis Schritt 3a standen sie in den
  Settings). Die Zahlen setzt der `PauseMenuScreen` beim Einschalten, der Run steht still.
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
  - Die Bedienelemente für den Ton verbindet seit v1.5.0 der Screen, auf dem sie stehen,
    in `OnEnable`, und zeigt dabei den Stand des AudioSystem: unter `UI/Settings` der
    `SettingsScreen` die Regler `MusicSlider` und `SfxSlider` (seit v1.5.1, bis dahin
    eine Gesamtlautstärke) und `ClassicModeToggle` (bis v1.5.0 „Use new Sound“, seit
    v1.5.1 umgekehrt: an heißt die Sounds von 2020), unter `UI/PauseMenu` der
    `PauseMenuScreen` die Schalter `MusicButton` und `SfxButton` (seit v1.5.1, bis dahin in
    den Settings). Ein neues Bedienelement für den Ton kommt in einen der beiden. Bis v1.4.13
    hielt das AudioSystem die Referenzen selbst und übernahm sie nach jedem Reload
    von seiner Szenenkopie (`AdoptSceneReferencesFrom`). Geprüft in v1.5.0 über die
    echten Buttons: Öffnen aus Hauptmenü und Pause, alle vier Elemente umschalten,
    Szene neu laden, der neue Stand ist zu sehen und lässt sich zurückschalten.
  - Klicksounds kommen von der Komponente `ButtonClickSound` am jeweiligen Button.
    Die alten Klick-Events zeigten seit Februar 2026 ins Leere. Kauf-Buttons haben
    bewusst keinen Klick, sie spielen eigene Select-/Kauf-Sounds; der Start-Button
    hatte nie einen. Die beiden Stumm-Schalter spielen ihren Klick seit v1.5.0 über
    ihren Screen (seit v1.5.1 die Pause), nach dem Umschalten: Wer die SFX ausschaltet, hört
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
  stummgeschaltete. Zurück ins Menü hebt `PlayMenuMusic` den Hold auf. Ein Regler ganz
  unten wirkt wie Stummschalten (seit v1.5.1): Die Musik pausiert, Effekte und Motor
  schweigen, beides ohne den Schalter umzulegen.
- **Die Regler sind nicht linear.** `SetMusicVolume` und `SetSfxVolume` (seit v1.5.1,
  bis dahin eine Gesamtlautstärke über `AudioListener.volume`) bilden die
  Reglerposition exponentiell auf einen 40-dB-Bereich ab (`k_VolumeRangeDb`), sonst
  passiert die gesamte hörbare Änderung in den unteren 20 % des Wegs. Wichtig:
  Den Wert zu quadrieren hilft **nicht** - eine Potenzkurve streckt den dB-Bereich
  gleichmäßig und lässt das Ungleichgewicht bestehen. Die Lautstärke geht an die
  Gruppen Music und SFX in `GameAudio.mixer`, über deren freigegebene Parameter
  `MusicVolume` und `SfxVolume`, ganz unten mit -80 dB. So gilt sie für jede Quelle in
  der Gruppe, nicht nur für die drei des AudioSystem. Den Spielstand übernimmt der Mixer
  in `AudioSystem.Start`, nicht in `Awake`. Im Spielstand steht die
  Reglerposition, nicht die Amplitude. Beim Ziehen wird bewusst nicht gespeichert
  (60×/Sekunde Schreibzugriff): Der Wert steht nur in `SaveSystem.Data` und kommt in
  `OnApplicationPause`/`OnApplicationQuit` des `AudioSystem` auf die Platte. Freigegeben
  sind die Parameter per Text: `m_ExposedParameters` in `GameAudio.mixer` nennt je
  Parameter die GUID der Lautstärke seiner Gruppe (`m_Volume` des
  `AudioMixerGroupController`) und einen Namen. Nach `ImportAsset` mit `ForceUpdate` kennt
  Unity sie, ohne die Datei umzuschreiben.

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

**Bilder vorher und nachher** nimmt im Play Mode `unity command capture game_view` auf
(`--source screen` ist der Standard), über den Bildschirm wie auf dem Handy. Die Aufnahme rendert
aber nicht neu, sie skaliert nur das Game-Fenster auf `--width` und `--height`. Für
echte Handy-Auflösung das Game-Fenster vorher auf eine feste Größe stellen (intern
`GameView.SetCustomResolution`, per Reflection), danach die alte Größe wieder wählen
und die neue löschen (`GameViewSizeGroup.RemoveCustomSize` erwartet den Gesamtindex).
Ohne `--output-path` kommt das Bild als Base64 zurück. Mit `--output-path` (absolut
oder relativ zum Projektordner) speichert die CLI es dort, außerhalb von `Assets/` ohne
`.meta`. Zeit anhalten
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
Particles (Lit/Unlit), 2 TextMesh Pro (dazu das Material in jedem Font Asset), 1 auf
dem eingebauten `Unlit/Texture` (der Sternenhintergrund,
`Materials/Skybox/Background.mat`).

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

- `UI/Images/2026/CoinIcon.png` hat 128 px (seit v1.5.1): Die Münze ist im Canvas
  höchstens 30 Einheiten groß, mit dem Pop im Run 32, auf einem 3200 px hohen Display
  rund 100 px. Das Bild ist die Mutter aus `2020/CoinIcon.png`, auf ihren Umriss
  zugeschnitten (2 px Rand) und über die Fläche gemittelt verkleinert, im linearen Licht.
  Das alte Bild hat 2000 px, die Mutter darin nur 1067 × 924. Im UI füllte sie ihr
  Rechteck deshalb nur zur Hälfte und war kaum zu sehen.
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
Pixel, ohne 0,5. In den Player Settings steht als Format für Android zwar nur „ETC“
(`PlayerSettings.Android.textureCompressionFormats`), die Texturen bekommen aber ETC2,
ohne Alphakanal ETC2 RGB und mit ETC2 RGBA (geprüft in v1.5.1 an allen Texturen der
Szene). Kantenlängen müssen durch 4 teilbar sein, sonst bleibt eine Textur
unkomprimiert (RGBA32). Das traf bis v1.5.0 35 UI-Bilder mit zusammen 10,4 MiB (etwa
648×207). Seit v1.5.1 liegen sie im Sprite Atlas, dessen Seite sich komprimieren lässt
(siehe „Bilder liegen im Sprite Atlas“ unter „UI“). Ein UI-Bild außerhalb des Atlas wäre
wieder unkomprimiert.

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
  die 22 halbierten. Ein Sprite im Atlas liegt dort im Rechteck `textureRect` der
  gepackten Kopie aus `SpriteAtlas.GetSprites`. Gegenüber dem Original ist es um
  `textureRect` minus `textureRectOffset` verschoben, weil Unity durchsichtige Ränder
  abschneidet. So in v1.5.1 für alle 36 UI-Bilder.
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
  kein Pool), der Münzstaub und der Score-Text. Der Score-Text seit v1.5.1 nicht mehr
  (siehe „Texte“ unter „UI“), auf dem Handy noch nicht nachgemessen.
- Speicher über 10 Minuten ohne Anstieg. Der Release-Build belegt 313 bis 316 MB
  (PSS), v1.4.5 belegte 365 bis 373 MB, vor allem wegen der Texturen aus v1.4.12. Die
  APK hat 68,2 statt 73,0 MB.
- Nach 10 Minuten GPU 42 bis 43 °C, Gehäuse 34 °C, keine Drosselung.
- Die Font-Textur baut sich im Run bei fast jeder neuen Score-Zahl neu auf, bis 8-mal
  pro Sekunde, je 0,2 bis 0,4 ms (`Font.CacheFontForText`), im Menü nie. Seit v1.5.1
  sind alle Texte TextMesh Pro: Im Editor lief `Font.CacheFontForText` in einem Run von
  800 Frames kein einziges Mal mehr, vorher 126-mal in 53 Frames.

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
  `unity command build run --output-path Builds/<Name>.apk --profile-name <Name> --options
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
