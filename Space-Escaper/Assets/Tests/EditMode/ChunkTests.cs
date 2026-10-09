using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SpaceEscaper.Tests
{
    public class ChunkTests
    {
        private const string k_ChunkFolder = "Assets/Prefabs/Chunks";
        private const string k_AsteroidFieldFolder = "Assets/Prefabs/AsteroidFields";
        private const string k_ObstacleTag = "Obstacle";
        private const string k_CoinTag = "Coin";

        // PlayerMotor crashes the ship only on colliders tagged Obstacle. Any other solid
        // collider stops the ship without a crash, and it hangs there while the score runs on.
        [Test]
        public void EverySolidColliderInAChunkIsAnObstacle()
        {
            List<string> untaggedColliders = new List<string>();
            foreach (Collider collider in GetColliders(k_ChunkFolder))
            {
                if (!collider.isTrigger && !collider.CompareTag(k_ObstacleTag))
                {
                    untaggedColliders.Add(GetPath(collider.transform));
                }
            }

            Assert.That(untaggedColliders, Is.Empty);
        }

        [Test]
        public void EveryTriggerInAChunkIsACoin()
        {
            List<string> strayTriggers = new List<string>();
            foreach (Collider collider in GetColliders(k_ChunkFolder))
            {
                bool isCoin = collider.CompareTag(k_CoinTag) && collider.GetComponent<Coin>() != null;
                if (collider.isTrigger && !isCoin)
                {
                    strayTriggers.Add(GetPath(collider.transform));
                }
            }

            Assert.That(strayTriggers, Is.Empty);
        }

        // The asteroid fields are scenery beside the track. Without colliders they cost the
        // physics nothing.
        [Test]
        public void AsteroidFieldsHaveNoColliders()
        {
            List<string> colliders = new List<string>();
            foreach (Collider collider in GetColliders(k_AsteroidFieldFolder))
            {
                colliders.Add(GetPath(collider.transform));
            }

            Assert.That(colliders, Is.Empty);
        }

        private static List<Collider> GetColliders(string folder)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            Assert.That(prefabGuids, Is.Not.Empty, "No prefabs in " + folder);

            List<Collider> colliders = new List<Collider>();
            foreach (string guid in prefabGuids)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                colliders.AddRange(prefab.GetComponentsInChildren<Collider>(true));
            }

            return colliders;
        }

        private static string GetPath(Transform transform)
        {
            string path = transform.name;
            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
            {
                path = parent.name + "/" + path;
            }

            return path;
        }
    }
}
