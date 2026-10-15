#if UNITY_EDITOR || UNITY_ENABLE_CHECKS
using UnityEngine;
using UnityEngine.Rendering;

namespace SpaceEscaper
{
    /// <summary>
    /// A "Space Escaper" panel in the Rendering Debugger with switches for testing. Only
    /// in the editor and in builds with checks (managed code variant Checked or Debug),
    /// the ones that have the Rendering Debugger. A three-finger double tap opens it on a
    /// phone.
    /// </summary>
    public static class TestPanel
    {
        private const string k_PanelName = "Space Escaper";

        [RuntimeInitializeOnLoadMethod]
        private static void AddPanel()
        {
            // Replaces the panel of an earlier Play Mode, in case it survived without a
            // domain reload, so the switch is not there twice.
            DebugUI.Panel panel = DebugManager.instance.GetPanel(k_PanelName, true, 0, true);
            panel.children.Add(new DebugUI.BoolField
            {
                displayName = "Invincible",
                tooltip = "The ship flies through obstacles instead of crashing.",
                getter = () => PlayerMotor.IsInvincible,
                setter = value => PlayerMotor.IsInvincible = value,
            });
        }
    }
}
#endif
