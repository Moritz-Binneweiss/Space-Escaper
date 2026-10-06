using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public float zSpawn = 100;
    public float tileLength = 60;
    public int numberOfTiles = 4;
    private List<GameObject> activeTiles = new List<GameObject>();
    public Transform playerTransform;

    // Start is called before the first frame update
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfTiles; i++)
        {
            if (i == 0)
                SpawnTile(0);
            else
                SpawnTile(Random.Range(1, tilePrefabs.Length));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTransform.position.z - 55 > zSpawn - (numberOfTiles * tileLength))
        {
            SpawnTile(Random.Range(1, tilePrefabs.Length));
            DeleteTile();
        }
    }

    public void SpawnTile(int tileIndex)
    {
        GameObject go = Instantiate(
            tilePrefabs[tileIndex],
            transform.forward * zSpawn,
            transform.rotation
        );
        activeTiles.Add(go);
        zSpawn += tileLength;
    }

    private void DeleteTile()
    {
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
    }

    public void RespawnTile()
    {
        // Called by the revive button, right after the ship starts running again.
        // Destroy only takes effect at the end of the frame, so the ship hit the
        // obstacle that had just killed it and crashed a second time in the same
        // frame. Deactivating first takes the colliders out immediately.
        foreach (GameObject tile in activeTiles)
            tile.SetActive(false);

        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);

        zSpawn -= 150;

        for (int i = 0; i < numberOfTiles; i++)
        {
            SpawnTile(Random.Range(1, tilePrefabs.Length));
        }
    }
}
