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
        [Tooltip("Replaces all tiles on a revive, so the ship does not hit what killed it. Only the track needs it.")]
        [SerializeField] private bool m_respawnsOnRevive;
        [SerializeField] private Transform m_player;

        private readonly List<GameObject> m_activeTiles = new List<GameObject>();

        // For tests, which check that the tiles and the spawn point line up end to end.
        internal IReadOnlyList<GameObject> Tiles => m_activeTiles;
        internal float NextSpawnZ => m_nextSpawnZ;

        private void OnEnable()
        {
            FloatingOrigin.WorldShifted += FloatingOrigin_WorldShifted;
            GameManager.ShipRevived += GameManager_ShipRevived;
        }

        private void OnDisable()
        {
            FloatingOrigin.WorldShifted -= FloatingOrigin_WorldShifted;
            GameManager.ShipRevived -= GameManager_ShipRevived;
        }

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

        private void RespawnTiles()
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

        // The tiles move back with the world, the spawn point is only a number.
        private void FloatingOrigin_WorldShifted(Vector3 offset)
        {
            m_nextSpawnZ += offset.z;
        }

        // Comes with the tap on the revive button, before the ship flies on in that frame.
        private void GameManager_ShipRevived()
        {
            if (m_respawnsOnRevive)
            {
                RespawnTiles();
            }
        }
    }
}
