using System.Runtime.CompilerServices;

// The test menu and the PlayMode tests reach into the save, see SaveSystem.LoadFrom
// and SaveSystem.Delete. The PlayMode tests also set FloatingOrigin.Distance and read
// TileManager.Tiles and TileManager.NextSpawnZ.
[assembly: InternalsVisibleTo("SpaceEscaper.Editor")]
[assembly: InternalsVisibleTo("SpaceEscaper.Tests.PlayMode")]
