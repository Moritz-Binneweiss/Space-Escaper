using System.Runtime.CompilerServices;

// The test menu and the PlayMode tests reach into the save, see SaveSystem.LoadFrom
// and SaveSystem.Delete.
[assembly: InternalsVisibleTo("SpaceEscaper.Editor")]
[assembly: InternalsVisibleTo("SpaceEscaper.Tests.PlayMode")]
