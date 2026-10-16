#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;

namespace SpaceEscaper.Tests
{
    // Works around a bug in the Test Framework 1.8.0. Without a domain reload, PlayMode tests run
    // only once per compile, and every later run finds no test at all.
    internal static class TestFrameworkFix
    {
        private const string k_ProviderTypeName = "UnityEngine.TestTools.Utils.PlayerTestAssemblyProvider";
        private const string k_CacheFieldName = "m_LoadedAssemblies";

        // The Test Framework keeps the loaded test assemblies until the next domain reload. When Play
        // Mode starts it empties the list, but it only loads the assemblies while there is no list,
        // so it stays empty. Without a list it loads them again, as after a domain reload.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DropCachedTestAssemblies()
        {
            Type provider = typeof(UnityTestAttribute).Assembly.GetType(k_ProviderTypeName);
            FieldInfo cache = provider?.GetField(k_CacheFieldName, BindingFlags.Static | BindingFlags.NonPublic);
            if (cache == null)
            {
                Debug.LogWarning("The Test Framework has changed. Check whether PlayMode tests still run more "
                    + "than once without a domain reload, and remove TestFrameworkFix if they do.");
                return;
            }

            cache.SetValue(null, null);
        }
    }
}
#endif
