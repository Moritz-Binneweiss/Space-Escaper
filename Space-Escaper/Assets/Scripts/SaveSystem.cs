using System;
using System.IO;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Loads the save once at startup and writes it back as one JSON file in
    /// Application.persistentDataPath.
    /// </summary>
    public static class SaveSystem
    {
        private const string k_FileName = "SaveData.json";
        private const string k_TemporaryFileName = "SaveData.json.tmp";

        private static SaveData s_data;
        private static string s_directory;

        /// <summary>
        /// The save of this session. Changes stay in memory until <see cref="Save"/>.
        /// </summary>
        public static SaveData Data => s_data;

        private static string FilePath => Path.Combine(s_directory, k_FileName);

        private static string TemporaryFilePath => Path.Combine(s_directory, k_TemporaryFileName);

        /// <summary>
        /// Writes the save to disk. Cheap enough for every purchase or death, not for
        /// every frame.
        /// </summary>
        public static void Save()
        {
            // Written to a second file first and moved over the old one afterwards,
            // so an app killed mid-write cannot leave half a save behind.
            try
            {
                File.WriteAllText(TemporaryFilePath, s_data.ToJson());
                File.Delete(FilePath);
                File.Move(TemporaryFilePath, FilePath);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                Debug.LogWarning($"Could not write the save file: {exception.Message}");
            }
        }

        /// <summary>
        /// Loads the save from another folder and writes it there from then on. Tests
        /// that play the game use it to leave the real save alone. The next start of
        /// the game loads from Application.persistentDataPath again.
        /// </summary>
        internal static void LoadFrom(string directory)
        {
            s_directory = directory;

            // Only the temporary file is left if the app was killed between deleting
            // the old save and moving the new one in. It is complete by then.
            string path = File.Exists(FilePath) ? FilePath : TemporaryFilePath;
            if (!File.Exists(path))
            {
                s_data = new SaveData();
                return;
            }

            try
            {
                s_data = SaveData.FromJson(File.ReadAllText(path));
            }
            catch (Exception exception) when (exception is IOException
                || exception is UnauthorizedAccessException
                || exception is ArgumentException)
            {
                // ArgumentException is what JsonUtility throws for broken JSON. The
                // game starts with a new save instead of not at all.
                Debug.LogWarning($"Could not read the save file, starting a new save: {exception.Message}");
                s_data = new SaveData();
            }
        }

        // Before the first scene loads, so every Awake can read the save.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Load()
        {
            LoadFrom(Application.persistentDataPath);
        }
    }
}
