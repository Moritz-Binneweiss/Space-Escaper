using System.Collections.Generic;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Spawns the field tiles ahead of the ship and recycles the ones it has passed.
    /// Works like <see cref="TileManager"/>, with its own tiles and spacing.
    /// </summary>
    public class FieldManager : MonoBehaviour
    {
        // How far the ship has to be past the start of the oldest tile before that
        // tile is recycled into a new one ahead.
        private const float k_RecycleDistance = 50f;

        [Tooltip("Tile 0 is the start tile and only spawned first. The others are picked at random.")]
        [SerializeField] private GameObject[] m_tilePrefabs;
        [Tooltip("Z position where the next tile spawns. The value set here is where the first one goes.")]
        [SerializeField] private float m_nextSpawnZ;
        [SerializeField] private float m_tileLength = 40f;
        [SerializeField] private int m_numberOfTiles = 4;
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
            if (m_player.position.z - k_RecycleDistance > oldestTileStart)
            {
                SpawnRandomTile();
                DeleteOldestTile();
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
