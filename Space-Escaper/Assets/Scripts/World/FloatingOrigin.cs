using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Moves the ship and the world back to the origin each time the ship has flown a set
    /// distance. Floats lose precision far from the origin, and a run has no end. Everything
    /// moves by the same offset in the same frame, so the picture stays the same.
    /// </summary>
    public class FloatingOrigin : MonoBehaviour
    {
        [Tooltip("How far the ship flies from the origin before it and the world move back by this distance.")]
        [SerializeField] private float m_distance = 4800f;
        [Tooltip("The ship whose distance from the origin counts.")]
        [SerializeField] private Transform m_ship;

        private readonly List<Transform> m_groups = new List<Transform>();

        // For tests, which move the world back every few units instead of every few thousand.
        internal float Distance
        {
            get => m_distance;
            set => m_distance = value;
        }

        /// <summary>
        /// Raised after the world has moved by the offset, for positions kept as numbers
        /// rather than in a transform.
        /// </summary>
        public static event Action<Vector3> WorldShifted;

        // The editor starts Play Mode without a domain reload, so static fields would keep their
        // values from the last one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            WorldShifted = null;
        }

        private void Awake()
        {
            // The groups of the scene are empty objects at the origin. They stay there and
            // their children move instead: the camera follows the ship inside its group and
            // would otherwise build up a local position as large as the distance flown. The
            // tiles are empty objects too, so the groups are noted before the first one spawns.
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                if (root.GetComponentCount() == 1)
                {
                    m_groups.Add(root.transform);
                }
            }
        }

        private void LateUpdate()
        {
            if (m_ship.position.z >= m_distance)
            {
                ShiftWorld(Vector3.back * m_distance);
            }
        }

        private void ShiftWorld(Vector3 offset)
        {
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                // The UI is drawn on the screen, not in the world.
                if (root.TryGetComponent(out Canvas _))
                {
                    continue;
                }

                if (m_groups.Contains(root.transform))
                {
                    foreach (Transform child in root.transform)
                    {
                        Move(child, offset);
                    }
                }
                else
                {
                    Move(root.transform, offset);
                }
            }

            // Colliders, the ship's CharacterController among them, take their new place
            // right away and not only at the next physics step.
            Physics.SyncTransforms();
            OnWorldShifted(offset);
        }

        private static void OnWorldShifted(Vector3 offset)
        {
            WorldShifted?.Invoke(offset);
        }

        private static void Move(Transform target, Vector3 offset)
        {
            target.position += offset;

            // Particles simulated in world space stay where they are when their system
            // moves, like the smoke trails of the explosion debris.
            foreach (ParticleSystem system in target.GetComponentsInChildren<ParticleSystem>())
            {
                if (system.main.simulationSpace == ParticleSystemSimulationSpace.World)
                {
                    MoveParticles(system, offset);
                }
            }
        }

        private static void MoveParticles(ParticleSystem system, Vector3 offset)
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[system.particleCount];
            int count = system.GetParticles(particles);
            for (int i = 0; i < count; i++)
            {
                particles[i].position += offset;
            }

            system.SetParticles(particles, count);
        }
    }
}
