using System.Runtime.CompilerServices;

// The test menu and the PlayMode tests reach into the save, see SaveSystem.LoadFrom
// and SaveSystem.Delete. The PlayMode tests also set FloatingOrigin.Distance, read
// TileManager.Tiles and TileManager.NextSpawnZ, and tap the buttons the screens hand
// them, like MainMenuScreen.StartButton.
[assembly: InternalsVisibleTo("SpaceEscaper.Editor")]
[assembly: InternalsVisibleTo("SpaceEscaper.Tests.PlayMode")]
