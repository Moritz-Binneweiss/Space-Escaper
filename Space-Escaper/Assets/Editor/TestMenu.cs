using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpaceEscaper.Editor
{
    /// <summary>
    /// The "Space Escaper" menu for testing: a new install, coins, all ships and a ship
    /// that does not crash. The save entries work in and outside Play Mode.
    /// </summary>
    [InitializeOnLoad]
    public static class TestMenu
    {
        private const string k_Menu = "Space Escaper/";
        private const string k_ResetSaveItem = k_Menu + "Reset Save (New Install)";
        private const string k_AddCoinsItem = k_Menu + "Add 1000 Coins";
        private const string k_UnlockShipsItem = k_Menu + "Unlock All Ships";
        private const string k_InvincibleItem = k_Menu + "Invincible";
        private const string k_InvincibleKey = "SpaceEscaper.Invincible";
        private const int k_CoinsToAdd = 1000;

        // More than 10 above the save entries, so a separator sits between them.
        private const int k_InvincibleItemPriority = 20;

        // The menu switch holds for every Play Mode of this editor session. In Play Mode,
        // the test panel in the Rendering Debugger can change it for the running one.
        private static bool IsInvincible => EditorApplication.isPlaying
            ? PlayerMotor.IsInvincible
            : SessionState.GetBool(k_InvincibleKey, false);

        // Entering Play Mode reloads all scripts, which resets the switch in PlayerMotor.
        // So it is kept in the editor session and set again after every reload.
        static TestMenu()
        {
            PlayerMotor.IsInvincible = SessionState.GetBool(k_InvincibleKey, false);
            if (PlayerMotor.IsInvincible && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.Log($"Invincible: the ship flies through obstacles ({k_InvincibleItem}).");
            }
        }

        [MenuItem(k_ResetSaveItem, false, 0)]
        private static void ResetSave()
        {
            if (!EditorUtility.DisplayDialog("Reset Save", "Delete the save? The next start is like a new install.",
                "Delete", "Cancel"))
            {
                return;
            }

            if (EditorApplication.isPlaying)
            {
                // A new install starts the game anew. Leaving Play Mode saves once more, so
                // the save is only deleted after that.
                EditorApplication.playModeStateChanged += DeleteSaveInEditMode;
                EditorApplication.ExitPlaymode();
                return;
            }

            DeleteSave();
        }

        [MenuItem(k_AddCoinsItem, false, 1)]
        private static void AddCoins()
        {
            ChangeSave(saveData => saveData.AddCoins(k_CoinsToAdd));
            Debug.Log($"Added {k_CoinsToAdd} coins, the save now has {SaveSystem.Data.Coins}.");
        }

        [MenuItem(k_UnlockShipsItem, false, 2)]
        private static void UnlockAllShips()
        {
            string[] catalogGuids = AssetDatabase.FindAssets("t:" + nameof(ShipCatalog));
            string catalogPath = AssetDatabase.GUIDToAssetPath(catalogGuids[0]);
            List<string> shipIds = AssetDatabase.LoadAssetAtPath<ShipCatalog>(catalogPath).GetShipIds();

            ChangeSave(saveData =>
            {
                foreach (string shipId in shipIds)
                {
                    // Bought for nothing, so the coins stay as they are.
                    saveData.TryBuyShip(shipId, 0);
                }
            });
            Debug.Log($"Unlocked all {shipIds.Count} ships.");
        }

        [MenuItem(k_InvincibleItem, false, k_InvincibleItemPriority)]
        private static void SwitchInvincible()
        {
            bool isInvincible = !IsInvincible;
            SessionState.SetBool(k_InvincibleKey, isInvincible);
            PlayerMotor.IsInvincible = isInvincible;
        }

        [MenuItem(k_InvincibleItem, true)]
        private static bool ValidateSwitchInvincible()
        {
            Menu.SetChecked(k_InvincibleItem, IsInvincible);
            return true;
        }

        // Outside Play Mode the save comes from disk, inside it is the one the game plays
        // with. There the scene then reloads as after "Exit", so the main menu shows the
        // new coins and the hangar the new ships.
        private static void ChangeSave(Action<SaveData> change)
        {
            if (!EditorApplication.isPlaying)
            {
                SaveSystem.LoadFrom(Application.persistentDataPath);
            }

            change(SaveSystem.Data);
            SaveSystem.Save();

            if (EditorApplication.isPlaying && GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMenu();
            }
        }

        private static void DeleteSaveInEditMode(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            EditorApplication.playModeStateChanged -= DeleteSaveInEditMode;
            DeleteSave();
        }

        private static void DeleteSave()
        {
            SaveSystem.LoadFrom(Application.persistentDataPath);
            SaveSystem.Delete();
            Debug.Log("Deleted the save, the next Play Mode starts like a new install.");
        }
    }
}
