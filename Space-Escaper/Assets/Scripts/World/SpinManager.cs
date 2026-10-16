using System.Collections.Generic;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Turns every active Spinner once per frame.
    /// </summary>
    public class SpinManager : MonoBehaviour
    {
        // Static, so a spinner can join no matter whether it or the manager wakes up first.
        private static readonly List<Spinner> s_spinners = new List<Spinner>();

        // The editor starts Play Mode without a domain reload, so static fields would keep their
        // values from the last one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            s_spinners.Clear();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < s_spinners.Count; i++)
            {
                s_spinners[i].Turn(deltaTime);
            }
        }

        public static void Add(Spinner spinner)
        {
            s_spinners.Add(spinner);
        }

        public static void Remove(Spinner spinner)
        {
            s_spinners.Remove(spinner);
        }
    }
}
