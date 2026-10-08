using System.Collections.Generic;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Spawns tiles ahead of the ship and recycles the ones it has passed. The scene
    /// uses it twice: for the track sections and for the asteroid fields in the
    /// background.
    /// </summary>
    public class TileManager : MonoBehaviour
    {
        // How far the spawn point moves back on a revive. The fresh tiles then
        // start a short distance ahead of the ship.
        private const float k_RespawnSetback = 150f;

        [Tooltip("Tile 0 is the start tile and only spawned first. The others are picked at random.")]
        [SerializeField] private GameObject[] m_tilePrefabs;
        [Tooltip("Z position where the next tile spawns. The value set here is where the first one goes.")]
        [SerializeField] private float m_nextSpawnZ = 100f;
        [SerializeField] private float m_tileLength = 60f;
        [SerializeField] private int m_numberOfTiles = 4;
        [Tooltip("How far the ship flies past the start of the oldest tile before that tile is recycled ahead.")]
        [SerializeField] private float m_recycleDistance = 55f;
        [SerializeField] private Transform m_player;

        private readonly List<GameObject> m_activeTiles = new List<GameObject>();

        private void Start()
        {
            SpawnTile(0);
            for (int i = 1; i < m_numberOfTiles; i++)
            {
                SpawnRandomTile();
            }
        }

        private void Update()
        {
            float oldestTileStart = m_nextSpawnZ - m_numberOfTiles * m_tileLength;
            if (m_player.position.z - m_recycleDistance > oldestTileStart)
            {
                SpawnRandomTile();
                DeleteOldestTile();
            }
        }

        /// <summary>
        /// Replaces all tiles with fresh ones. The revive button calls it for the
        /// track, right after the ship starts running again.
        /// </summary>
        public void RespawnTiles()
        {
            // Destroy only takes effect at the end of the frame. Without
            // deactivating first, the ship would hit the obstacle that just killed
            // it and crash again in the same frame.
            foreach (GameObject tile in m_activeTiles)
            {
                tile.SetActive(false);
            }

            while (m_activeTiles.Count > 0)
            {
                DeleteOldestTile();
            }

            m_nextSpawnZ -= k_RespawnSetback;
            for (int i = 0; i < m_numberOfTiles; i++)
            {
                SpawnRandomTile();
            }
        }

        private void SpawnRandomTile()
        {
            SpawnTile(Random.Range(1, m_tilePrefabs.Length));
        }

        private void SpawnTile(int prefabIndex)
        {
            Vector3 position = transform.forward * m_nextSpawnZ;
            GameObject tile = Instantiate(m_tilePrefabs[prefabIndex], position, transform.rotation);
            m_activeTiles.Add(tile);
            m_nextSpawnZ += m_tileLength;
        }

        private void DeleteOldestTile()
        {
            Destroy(m_activeTiles[0]);
            m_activeTiles.RemoveAt(0);
        }
    }
}
