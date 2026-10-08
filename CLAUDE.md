# Space Escaper

3-Lane Endless Runner für Android, Unity **6000.6.0f1**, URP 17.6.0.
Ursprünglich 2020 in Unity 2019.4 gebaut und im Play Store veröffentlicht - aktuell
ein Legacy-Projekt, das schrittweise modernisiert wird. Hobbyprojekt, kein Zeitdruck,
Arbeit passiert in unregelmäßigen Sessions.

## Aufbau

Repo-Root ≠ Unity-Projekt: das Unity-Projekt liegt in `Space-Escaper/`.

- `Space-Escaper/Assets/Scripts/` - 19 Skripte, ~2050 Zeilen, alle im Namespace
  `SpaceEscaper` (Stil siehe „Code-Stil“). Einstiegspunkte:
  `GameManager.cs` (Menü, Shop, Score, Death - macht sehr viel), `PlayerMotor.cs`,
  `AudioSystem.cs`, `MobileInput.cs`, `TileManager.cs` (Spawning),
  `SaveSystem.cs` / `SaveData.cs` (Spielstand), `ShipData.cs` / `ShipCatalog.cs`
  (Schiffe).
- `Space-Escaper/Assets/Tests/EditMode/` - EditMode-Tests, siehe „Tests“.
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

- **Unity ist die Wahrheit beim Kompilieren.** VS-Code-/OmniSharp-Warnungen sagen nichts
  darüber aus, ob das Projekt in Unity baut. Nicht auf Basis von Editor-Warnungen "reparieren".
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
  (`m_isRunning`, `HasSwipedLeft`), Methoden auch (`OpenShop` statt `ShopOn`).
  Keine Abkürzungen außer in Mathe (`k_VolumeRangeDb`).
- **Keine öffentlichen Felder.** Was im Inspector stehen soll, ist
  `[SerializeField] private` (Attribut in derselben Zeile), andere Klassen lesen
  über Properties (`public bool IsRunning => m_isRunning;`).
- **Strings als Konstanten** oben in der Klasse: Animator-Trigger, Tags,
  Szenenname, Dateiname des Spielstands. Die Werte selbst sind Daten und bleiben,
  wie sie sind (etwa der Trigger `Allive` oder `SaveData.json`), sonst brechen
  Animator und Spielstände.
- **Formatierung:** Allman-Klammern, 4 Leerzeichen, Klammern auch um einzelne
  Anweisungen, eine Deklaration pro Zeile, `private` immer ausgeschrieben, `switch`
  mit `default`, Zeilen höchstens 120 Zeichen, UTF-8 ohne BOM, LF. Die
  `.editorconfig` im Repo-Root hält das für die IDE fest.
- **Reihenfolge in der Klasse:** Konstanten, serialisierte Felder, private Felder,
  Properties, Unity-Methoden (`Awake`, `Start`, `Update` …), öffentliche Methoden,
  private Methoden.
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
- **Methoden mit OnClick-Aufruf:** Die Szene speichert den Methodennamen als Text,
  ein umbenannter Button tut sonst einfach nichts. Betroffen sind
  `GameManager.Play`, `ReturnToMenu`, `RequestRevive`, `OpenShop`, `CloseShop`,
  `ShowPreviousFamily`, `ShowNextFamily`, `ShowShip(ShipData)` und `SelectOrBuyShownShip`,
  `PauseMenu.Pause` und `Continue`, `SettingsManager.OpenFromMainMenu`,
  `OpenFromPauseMenu` und `Close`, `CameraSwitch.SwitchToMainCamera` und
  `SwitchToShopCamera` sowie `TileManager.RespawnTiles`. Beim Umbenennen
  `m_MethodName` in `Game.unity` mitziehen, am besten im Editor per
  `SerializedObject`.
- **Felder in `SaveData`:** Ihre Namen sind die Schlüssel in der Spielstand-Datei,
  siehe „Spielstand“.
- **Absichern:** vor dem Umbau alle serialisierten Werte und OnClick-Aufrufe
  dumpen und hinterher vergleichen. So lief v1.4.5: 627 Werte, alle gleich.

## Tests

Seit v1.4.7 gibt es EditMode-Tests mit dem Unity Test Framework (1.8.0, im
Manifest als direkte Abhängigkeit). Sie liegen in `Assets/Tests/EditMode/` und
prüfen die Regeln des Spielstands ohne Szene: Neuinstallation, älterer Spielstand,
unbekanntes Schiff, Kauf, Münzen nach einem Revive, Highscore, Audio-Einstellungen
und Versionsnummer. Seit v1.4.8 prüfen sie auch die Schiffsdaten
(`ShipCatalogTests`): jedes `ShipData` genau einmal im Katalog, das Startschiff
darin und gratis, alle anderen mit Preis und Preisschild, jedes mit eindeutiger ID,
Modell, Hangar-Größe und Antriebsflamme, jede Familie mit Schiffen.

- **Ausführen:** im Editor unter Window > General > Test Runner, Reiter EditMode,
  oder per `unity command run_tests --mode EditMode`. Dauert wenige Sekunden,
  also vor jedem Commit, der `SaveData`, `SaveSystem` oder `RunCoins` ändert. Die
  CLI legt dabei eine `TestResults.xml` neben den Spielstand, die darf weg. Ist die
  offene Szene ungespeichert, fragt der Test Runner vorher per Dialog, ob er sie
  speichern soll. Bis jemand klickt, steht Unity, und die CLI bricht nach 30 s ab.
- **Testbar ist, was keine Szene braucht.** Regeln gehören deshalb in einfache
  Klassen wie `SaveData` und `RunCoins`, nicht in MonoBehaviours. Was nur mit der
  Szene geht (dass `GameManager` nach einem Kauf sofort speichert, Eingaben,
  Auto-Pause), bleibt beim Test im Play Mode, siehe „Bewegung testen“ und „Eingabe“.
- **Ein neuer Test sollte einmal rot gewesen sein.** Den Bug dafür kurz wieder
  einbauen und prüfen, dass der Test ihn findet. So in v1.4.7: Mit dem alten
  Revive-Bug zeigte der Test 13 statt 8 Münzen, ohne den Rückfall aufs Startschiff
  scheiterten alle vier ungültigen Schiffe.
- Die Tests brauchen eigene Assemblies, siehe „Assembly Definitions“ unter
  „Unity-Besonderheiten“.

## Ordnerstruktur

Seit v1.4.5 ist `Assets/` nach Asset-Typ sortiert, wie es Unitys Leitfaden zur
Projektorganisation und die Unity-Templates vorsehen. Im Root von `Assets/` liegen
nur Ordner.

| Ordner | Inhalt |
|---|---|
| `Animations/` | Animator Controller und Clips, nach Gruppe |
| `Audio/` | Clips in `Classic/` und `New/`, die beiden Banks, der Mixer, in `New/` das Ableton-Projekt der neuen SFX |
| `Branding/` | App-Icon, Splash-Logos und Splash-Hintergrund (Player Settings), Banner, weitere Logos |
| `Data/` | ScriptableObjects mit Spieldaten, nach Gruppe (`Ships/`: ein `ShipData` pro Schiff und der `ShipCatalog`) |
| `Materials/` | Materialien, nach Gruppe |
| `Models/` | `.fbx` und `.blend`-Quellen samt ihren Texturen, nach Gruppe |
| `Prefabs/` | nach Gruppe, dazu `Chunks/` (Streckenabschnitte) und `AsteroidFields/` (Hintergrund) |
| `Scenes/` | `Game.unity` |
| `Scripts/` | alle Skripte, flach, dazu `SpaceEscaper.asmdef` |
| `Settings/` | URP-Asset, Renderer, Global Settings, Volume Profile, Build Profiles |
| `Shaders/` | `BendWorld.shader` |
| `Tests/` | `EditMode/` mit den Tests und ihrer Assembly Definition |
| `Textures/` | Texturen der Unity-Materialien, nach Gruppe |
| `UI/` | `Images/` (Sprites, Shop-Sprites in `Images/Shop/`), `Fonts/`, `Mockups.png` |

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
- `Scripts/` bleibt flach, solange es so wenige Skripte sind. Unterordner kommen
  mit dem Aufteilen des GameManagers in v1.5.0.
- Fremd-Pakete bleiben an ihrem Platz, Asset-Store-Updates erwarten sie dort.

## Aufbau der Szene

Seit v1.4.5 ist `Game.unity` nach Rollen gruppiert. Die Gruppen sind leere
Objekte im Ursprung (Position 0, keine Rotation, Skalierung 1), ihre Kinder
behalten dadurch ihre Weltposition.

| Root-Objekt | Inhalt |
|---|---|
| `Systems` | `GameManager` (dazu `PauseMenu`, `SettingsManager`, `CameraSwitch`), `TileManager`, `FieldManager`, `EventSystem` |
| `AudioSystem` | bleibt im Root, `DontDestroyOnLoad` wirkt nur auf Root-Objekte |
| `Playership` | bleibt im Root, weil es sich jeden Frame bewegt |
| `Cameras` | `MainCamera`, `ShopCamera` |
| `Environment` | `DirectionalLight`, `Spaceport`, `Hangar`, `ShopShips` |
| `UI` | Canvas mit `DeathMenu`, `MainMenu`, `GameMenu`, `PauseMenu`, `Settings`, `Shop` |

- **Neue Objekte** kommen in die passende Gruppe. Eine neue Gruppe gehört in den
  Ursprung, sonst verschiebt sie alle Kinder mit.
- **Was sich ständig bewegt, bleibt flach.** Unity aktualisiert Transforms pro
  Hierarchie, bewegte Kinder in großen Hierarchien kosten mehr. Deshalb liegt
  `Playership` im Root, und die Streckenabschnitte spawnen dort ebenfalls.
- **Spawning:** `Systems/TileManager` und `Systems/FieldManager` tragen dieselbe
  Komponente `TileManager` (seit v1.4.9, vorher zwei fast gleiche Klassen). Die
  eine legt die Streckenabschnitte vor das Schiff (`Chunk…`, 60 Einheiten lang, 4
  auf einmal), die andere die Asteroidenfelder im Hintergrund (`AsteroidField…`, 40
  lang, 8 auf einmal). Wann ein passierter Abschnitt nach vorn wandert, legt
  `m_recycleDistance` fest: 55 bei der Strecke, 50 bei den Feldern. Nur die Strecke
  setzt der Revive-Button neu, siehe „Bekannte Altlasten“.
- **Die Reihenfolge im UI ist die Zeichenreihenfolge:** Spätere Geschwister
  liegen oben. Sie blieb beim Umbau, wie sie war. Vor einem Umsortieren die
  Übergänge prüfen, in denen zwei Menüs gleichzeitig sichtbar sind (Tod, Pause,
  Settings).
- **Code hängt kaum an der Hierarchie:** Skripte finden einander über
  Inspector-Referenzen, `FindAnyObjectByType` und den Tag `Player`, nicht über
  Pfade. Ausnahmen sind die Reihenfolge der Kinder im `SkinButtonContainer`, eins
  pro Familie (siehe „Shop“), und Clips, die über einen Pfad animieren (siehe
  „Benennung von Dateien und Objekten“).
- **Tags:** `Player` (Schiff), `Obstacle` (Crash) und `MainCamera` (Unity-Standard)
  werden gebraucht, `Coin` steht an der Münze, wird aber noch nicht gelesen.
  `Pause`, `Shop`, `TileManager`, `Audio` und `Shootable` las kein Code, sie sind
  seit v1.4.5 entfernt, ebenso das deaktivierte `SkinChange` an der
  `ShopCamera` und ein ungenutzter `CharacterController` am `TileManager`.
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
  ausgeschrieben, keine Kürzel wie ACT, FTR oder VGR.
- **Animationsclips** heißen `Ziel_Zustand` (`GameMenu_Show`, `DeathMenu_Alive`),
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
  `AnimationUtility` umhängen, nicht per Text. In der Szene betrifft das nur
  `UI/GameMenu/CoinIcon` (Clip `GameMenu_Show`), alle anderen Clips animieren ihr
  eigenes Objekt.
- **Objekte in Prefabs per Text umbenennen** (`m_Name` der GameObjects, `value:`
  unter `propertyPath: m_Name`), aus demselben Grund wie unter „Code-Stil“. Ältere
  Prefabs liegen auf der Platte noch mit CRLF, Git normalisiert das beim Commit.

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
  Änderung an den anderen kommt erst mit einem neuen FBX-Export ins Spiel.
- Assets, die nicht in einer Szene oder einem Prefab referenziert sind, landen nicht im
  Build - Aufräumen in `Assets/` ist also Repo-Hygiene, keine Build-Größen-Optimierung.
- **Assembly Definitions** (seit v1.4.7): Alle Skripte liegen in der Assembly
  `SpaceEscaper` (`Scripts/SpaceEscaper.asmdef`) statt in `Assembly-CSharp`, die
  Tests in `SpaceEscaper.Tests.EditMode`. Test-Assemblies kommen an
  `Assembly-CSharp` nicht heran. Neue Skripte in `Scripts/` landen von selbst in
  `SpaceEscaper`. **Nutzt ein Skript ein weiteres Paket** (etwa TextMesh Pro in
  v1.5.1), gehört dessen Assembly in die References der `SpaceEscaper.asmdef`, sonst
  findet der Compiler es nicht. Derzeit stehen dort `Unity.InputSystem` und
  `UnityEngine.UI`. Szene und Prefabs finden ihre Skripte über deren GUID, der Umzug
  hat nichts gebrochen (geprüft: kein fehlendes Skript, alle 34 OnClick-Aufrufe
  finden ihre Methode).
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
  Displays, rundet Unity: 90 Hz ergibt 45 FPS, 144 Hz 72 FPS. „Optimized Frame
  Pacing“ ist aus, damit würde Unity immer abrunden. Im Editor gilt das Ziel nur
  fürs Game-Fenster.
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
- **Bewegung testen:** Im Play Mode das Schiff auf y = 100 heben
  (`CharacterController` dafür kurz aus), dann trifft es nichts und fliegt normal
  weiter. Der erste zufällige Streckenabschnitt beginnt bei z = 60, seine
  Hindernisse reichen teils bis 57 zurück. Beim Starttempo ist das Schiff nach gut
  4 s dort, bis dahin also heben oder die Collider der Hindernisse davor
  ausschalten. `Time.captureFramerate` legt die Frame-Zeit fest, egal wie schnell der
  Editor gerade läuft, und ein Handler an `Application.onBeforeRender` schreibt
  die Position pro Frame mit. So wurde der Spurwechsel in v1.4.6 bei 30, 60 und
  144 FPS gemessen. Für einen Vergleich vor und nach einem Umbau `Random.InitState`
  vor einem Szenen-Reload und noch einmal beim Start des Runs setzen, dann spawnen
  die `TileManager` dieselben Abschnitte (so in v1.4.9: alle 62 Ereignisse gleich).
  Eine Auto-Pause verschiebt dabei nur die Bildnummern. Ein Run im Test schreibt
  beim Tod Münzen und Highscore in den Spielstand. Ihn direkt vor jedem Lauf sichern
  und danach genau diese Sicherung zurücklegen, keine ältere: Dazwischen kann jemand
  im Editor gespielt haben (so ging in v1.4.9 ein Spielstand verloren).
- **Test-APKs** baut die Unity-CLI asynchron: `unity command build --outputPath
  Builds/<Name>.apk --confirm true`, danach `build_status` abfragen, bis es
  `completed` meldet. Vorher den Play Mode beenden. Die Einstellungen kommen aus
  den Player Settings (IL2CPP für ARMv7 und ARM64, Debug-Signatur), ein Build
  dauert rund 6 Minuten. `Space-Escaper/Builds/` ist von Git ignoriert, der Ordner
  `…_BackUpThisFolder_ButDontShipItWithYourGame` neben der APK enthält nur
  Debug-Symbole. Ein Build ändert zwei URP-Assets, siehe „Render Pipeline: URP“.
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

## Bekannte Altlasten (bewusst, noch offen)

- Ungenutzte Bilder, Materialien und Modelle liegen noch im Projekt, etwa die alten
  Preisschilder oder Doppelgänger mit `Unused` im Namen. Aufräumen gehört zum UI
  Overhaul (v1.5.1) und zum Remaster (v1.7.0). Bitgleiche Duplikate sind seit
  v1.4.5 gelöscht (39 in `Assets/UI/`, 4 beim Sortieren der Ordner), alle
  UI-Bilder liegen in `UI/Images/`. Gleiche Texturen in `Models/` und `Textures/`
  sind Absicht: die einen gehören zu den `.blend`-Quellen, die anderen zu den
  Unity-Materialien.
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
  ruft `Revive()` ohne Gegenleistung durch. Der Revive-Button ruft per OnClick
  zusätzlich `RespawnTiles` am `TileManager` der Strecke (`Systems/TileManager`)
  auf - diese Verbindung existiert nur in der Szene, nicht im Code. `RespawnTiles`
  deaktiviert die alten Abschnitte, bevor es sie löscht: `Destroy` greift erst am
  Frame-Ende, das Schiff fliegt aber im selben Frame wieder los. Ohne das krachte es
  beim Revive sofort ins selbe Hindernis (zweite Explosion, Spiel-UI weg, Schiff
  unsichtbar; behoben in v1.4.4).
- Highscore-Leaderboard entfiel mit Google Play Games; es gibt nur noch den lokalen
  Highscore im Spielstand. Der Pokal-Button im Hauptmenü (`UI/MainMenu/LeaderboardButton`) bleibt
  trotzdem **bewusst sichtbar**, auch ohne Funktion (Entscheidung Oktober 2026) -
  nicht ausblenden oder löschen. Sein OnClick-Aufruf zeigte auf eine nicht mehr
  existierende Methode und ist entfernt; der Button spielt nur den Klicksound.

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
  bekannten IDs übergibt `GameManager.LoadShipSelection` aus dem `ShipCatalog`. Bis
  v1.4.0 war der Default 0, das zeigte auf `FlameContainer` statt auf ein Schiff und
  ließ das Spiel beim Start und beim Tod abstürzen. Lokal fiel das nie auf, weil der
  Spielstand gesetzt war - **Neuinstallation gezielt testen** (siehe unten).
- Ein Kauf (`SaveData.TryBuyShip`) zieht die Münzen ab und schaltet das Schiff frei,
  beides oder nichts. Danach speichert `GameManager` sofort.
- Der Score wird überall abgeschnitten angezeigt und gespeichert (`(int)m_score`),
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
  Spielstand im Editor deshalb verschwinden.
- **Neuinstallation im Editor testen:** Play Mode beenden und `SaveData.json` samt
  einer eventuellen `SaveData.json.tmp` wegschieben. Der nächste Start beginnt mit 0
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

- **Der Hangar liest die Daten:** Die Skin-Buttons übergeben per OnClick ihr
  `ShipData` an `GameManager.ShowShip`, die Pfeile zeigen das erste Schiff der
  Familie davor oder danach (`ShipCatalog.GetFirstShipOfFamily`). Ein einziger
  unsichtbarer Button, `UI/Shop/SelectOrBuyButton`, kauft oder wählt das gezeigte
  Schiff (`SelectOrBuyShownShip`). Sein Kind `Image` zeigt Select, Selected oder das
  Preisschild aus dem `ShipData` und lässt Klicks zum Button durch.
- **Modelle sind Prefabs** in `Prefabs/Ships/` (`AristocratSkin1.prefab` …), mit
  ihrem Platz auf dem Schiff: Versatz und Skalierung stecken in der Wurzel. Der
  `GameManager` setzt das Modell des geflogenen Schiffs unter `Playership/Ship` ein,
  das des gezeigten unter `Environment/ShopShips`, dort in der Mitte und auf
  `ShipData.HangarScale`. Beim Tod schaltet er das Modell aus, beim Revive wieder an.
  Die VAGOR-Skins haben eine etwas andere Skalierung als ihre Familie (Skin 1 2,6 ×
  3,2 × 3,2, Skin 2 und 3 2,7 × 3 × 3), so übernommen aus dem Stand vor v1.4.8.
- **IDs:** Jedes `ShipData` hat eine ID (`AristocratSkin1` …), unter der der
  Spielstand das Schiff speichert. Eine geänderte ID verliert das Schiff in
  bestehenden Spielständen. Das Startschiff legt `ShipCatalog.StarterShip` fest, die
  Reihenfolge im Katalog ist nur die des Hangars.
- **Ein neues Schiff:** Modell-Prefab in `Prefabs/Ships/` anlegen, ein `ShipData` in
  `Data/Ships/` mit ID, Familie, Preis, Preisschild-Bild, Modell, Hangar-Größe und
  Flamme, das `ShipData` in den `ShipCatalog` eintragen und einen Skin-Button mit
  OnClick `GameManager.ShowShip` und dem `ShipData` in die Gruppe der Familie legen.
  Die `ShipCatalogTests` prüfen danach die Daten.
- `m_skinButtonContainer` hat ein Kind pro Familie, in der Reihenfolge von
  `ShipFamily`.
- `m_currentShip` ist das *geflogene* Schiff, `m_shownShip` das gerade im Hangar
  *angesehene*. Flammen und alles im Spiel gehören an `m_currentShip` - die
  Verwechslung zeigte früher eine falsche, neben dem Schiff schwebende Flamme.

**Antriebsflammen** sind Prefabs in `Prefabs/Ships/`: `FlameAristocrat`,
`FlameFreeter` und `FlameVagor` mit den Düsen `FlameLeft` und `FlameRight`. Jedes
`ShipData` verweist auf die Flamme seiner Familie, die Skins teilen sie sich. Der
`GameManager` setzt sie neben das Modell unter `Playership/Ship` und schaltet sie
für jedes Schiff gleich, nur über das GameObject: aus im Menü und im Hangar, an vom
Start des Runs bis zum Crash und wieder nach einem Revive. Die Emission bleibt
immer an. Bis v1.4.7 lagen die Flammen im `Playership.prefab`, und
`PlayerMotor.m_engineFlame` schaltete beim ARISTOCRAT zusätzlich die Emission. Ein
versehentlicher Emission-Override in der Szene ließ deshalb den FREETER von v1.4.0
bis v1.4.2 ohne sichtbaren Antrieb fliegen, während er beim ARISTOCRAT nicht
auffiel. Weil die Flammen jetzt erst zur Laufzeit entstehen, kann die Szene sie
nicht mehr überschreiben.

**Preisschilder sind Bilder, keine Texte.** Den Preis im `ShipData` allein zu ändern
reicht nicht - der Shop zeigt ihn aus `Assets/UI/Images/PriceTagAristocrat250.png`,
`PriceTagFreeter750.png` und `PriceTagVagor1250.png`. Aufbau wie bei den Originalen
(`PriceTagAristocrat3500.png`, `PriceTagFreeter6000.png`, `PriceTagVagor8000.png`,
pixelgleich nachgeprüft): `ButtonBlank.png` als Platte, `CoinIcon.png` als Icon,
Zahl in `UI/Fonts/NeuropolXRegular.ttf` mit 49 px Ziffernhöhe ab y = 71, Icon und
Zahl als Gruppe mittig. Die Original-Schilder mit den alten Preisen liegen unverändert
daneben und sind in der Szene nicht mehr referenziert. Sie bleiben bewusst liegen,
entschieden wird beim UI Overhaul (v1.5.1) - bis dahin nicht löschen.

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
  im selben Frame. Der Revive-Bug (v1.4.4) trat nur so auf - per
  `onClick.Invoke()` aus dem Editor sah alles gut aus. Kommen echte Eingaben nicht
  an, weil Unity im Hintergrund läuft, tut es ein eigenes System im `PlayerLoop`
  unter `PreUpdate`, das `ExecuteEvents.Execute` mit `pointerClickHandler` auf dem
  Button aufruft: Es läuft wie ein echter Klick vor allen `Update()`-Methoden. So
  wurde in v1.4.9 der Revive getestet.

## Pause

Seit v1.4.5 gibt es genau einen Pause-Zustand: `PauseMenu.IsPaused`, statisch
und nur von `PauseMenu` gesetzt. `PauseMenu.Awake` setzt ihn zurück, sonst überlebt
er den Szenen-Reload hinter „Exit“. Bis v1.4.4 gab es zwei Flags, und eines davon
blieb nach „Exit“ aus der Pause auf `true` stehen.

- `Pause()` und `Continue()` sind von überall gefahrlos aufrufbar, nicht nur von
  den Buttons. `Pause()` greift nur bei `GameManager.IsRunActive` (Run gestartet
  und Schiff lebt, also nicht im Menü, im Hangar oder auf dem Todesbildschirm) und
  nur einmal, `Continue()` nur aus der Pause. Darauf baut die Auto-Pause auf.
- **Auto-Pause** (seit v1.4.5): `PauseMenu` pausiert von selbst, sobald die App
  den Fokus verliert (`OnApplicationFocus(false)`) oder in den Hintergrund geht
  (`OnApplicationPause(true)`). Beim Verlassen der App meldet Android beides, eine
  heruntergezogene Benachrichtigungsleiste nur den Fokusverlust, und ohne Pause
  flöge das Schiff dabei weiter. Weiter geht es nur über „Continue“, nie von
  selbst. Im Editor zählt schon ein Klick außerhalb des Game-Fensters als
  Fokusverlust und pausiert den Run.
- **Auto-Pause testen:** Im Editor die beiden Methoden per Reflection aufrufen,
  in jedem Zustand (Menü, Run, Pause, Settings, Tod, nach Revive). Ein anderes
  Editor-Fenster per `EditorWindow.FocusWindowIfItsOpen` zu fokussieren, löst
  zwar echt aus, aber verzögert, solange Unity selbst nicht im Vordergrund ist.
  Den echten App-Wechsel gibt es nur auf dem Handy.
- Von Hand pausiert nur der Pause-Button. Eine Zurück-Geste auf Android oder
  Escape am PC gibt es bewusst nicht (Entscheidung Oktober 2026). Auch die
  Standardaktion „Cancel“ am `InputSystemUIInputModule`, die auf Escape lag, ist
  seit v1.4.5 abgehängt, weil kein UI-Element darauf reagierte.
- `SettingsManager.Close()` liest den Zustand, um zu entscheiden, ob „Zurück“ ins
  Pause- oder ins Hauptmenü führt.
- „Exit“ im Pause-Menü ruft `GameManager.ReturnToMenu`, also einen Szenen-Reload.
  `GameManager.Awake` setzt dabei `Time.timeScale` zurück.
- Die Musik läuft in der Pause bewusst weiter (Entscheidung Oktober 2026), so hört
  man den Lautstärkeregler, wenn man die Settings aus der Pause öffnet. Beim Tod
  pausiert sie dagegen, siehe „Audio-System“.
- **Zum Testen das Schiff anhalten.** Ohne Steuerung kracht es nach 2-3 s ins
  erste Hindernis, und der Test landet auf dem Todesbildschirm. Den Run per Code
  starten und `PlayerMotor.m_speed` per Reflection auf 0 setzen: Der Run bleibt
  aktiv, das Schiff bewegt sich aber nicht. Der Punkte-Multiplikator wird dabei
  nach 5 s negativ (Geschwindigkeit minus Startgeschwindigkeit), ein Testartefakt.

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

- **UI-Referenzen und DontDestroyOnLoad.** Die Bedienelemente liegen unter
  `UI/Settings` in der Szene (`NewSoundsToggle` „Use new Sound“, `VolumeSlider`,
  `SfxButton`, `MusicButton`), das AudioSystem überlebt aber den Reload. Deshalb übergibt die
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
  Seit v1.4.4 hält `GameManager.HandleDeath` die Musik beim Tod genauso an
  (`PauseMusic`), ein Revive setzt sie an derselben Stelle fort (`ResumeMusic`).
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
Particles (Lit/Unlit), 2 TextMesh Pro, 1 Skybox.

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

## Sprache

Antworten auf Deutsch. Code, Bezeichner und Commit-Messages auf Englisch
(bestehende Konvention: `add:`, `edit:`, `fix:`, `refactor:`).

Keine langen Gedankenstriche (Geviert- und Halbgeviertstrich, U+2014 und U+2013),
weder in Doku, Commits, PRs, Releases und Tags noch in Antworten. Im Text steht
stattdessen `-`, als Trenner in Titeln `|` (z. B. `v1.4.2 | Input System & Engine
Flame Fix`).
