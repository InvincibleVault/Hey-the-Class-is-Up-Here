using System.Collections.Generic;
using UnityEngine;

// CapstoneObjectSpawner: the backend for the 3D prototype's object spawning.
// One spawner lives in the scene. Every UI button maps to one object
// through SpawnButton (or through the prefab list + SpawnObject index),
// so adding a new spawnable object never means touching this code.
//
// Setup:
//   1. Put this on an empty GameObject in the scene (e.g. "Spawner").
//   2. Drag a spawn point Transform into Spawn Point (leave it empty to
//      spawn at the spawner's own position).
//   3. Either fill Spawnable Prefabs in order and call SpawnObject(index)
//      from a button, or give each SpawnButton its own prefab directly.
public class CapstoneObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Where spawned objects appear. Leave empty to use this GameObject's position.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Optional parent for spawned objects, keeps the Hierarchy tidy. Leave empty for scene root.")]
    [SerializeField] private Transform spawnedParent;

    [Tooltip("Random offset applied to each spawn position, per axis. Zero means exact spawn point.")]
    [SerializeField] private Vector3 positionJitter = Vector3.zero;

    [Tooltip("If true, each spawned object gets a random Y rotation.")]
    [SerializeField] private bool randomizeRotationY = false;

    [Header("Objects")]
    [Tooltip("Prefabs in button order. Button 0 spawns index 0, button 1 spawns index 1, and so on.")]
    [SerializeField] private List<GameObject> spawnablePrefabs = new List<GameObject>();

    // Spawn by button index. Safe to wire straight into a Button's OnClick
    // event (Unity passes the int argument through the inspector).
    public void SpawnObject(int index)
    {
        if (index < 0 || index >= spawnablePrefabs.Count)
        {
            Debug.LogWarning($"[CapstoneObjectSpawner] No prefab at index {index}. Check the Spawnable Prefabs list.");
            return;
        }

        SpawnPrefab(spawnablePrefabs[index]);
    }

    // Spawn a specific prefab. This is what SpawnButton calls when a button
    // carries its own object reference.
    public void SpawnPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[CapstoneObjectSpawner] Tried to spawn a null prefab. Assign one on the button or in the list.");
            return;
        }

        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        position += new Vector3(
            Random.Range(-positionJitter.x, positionJitter.x),
            Random.Range(-positionJitter.y, positionJitter.y),
            Random.Range(-positionJitter.z, positionJitter.z)
        );

        Quaternion rotation = prefab.transform.rotation;
        if (randomizeRotationY)
        {
            rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        GameObject spawned = Instantiate(prefab, position, rotation);
        if (spawnedParent != null)
        {
            spawned.transform.SetParent(spawnedParent, true);
        }
    }

    // Removes everything this spawner created. Useful for a "Clear" button
    // or for resetting the prototype scene between runs.
    public void ClearSpawned()
    {
        if (spawnedParent != null)
        {
            foreach (Transform child in spawnedParent)
            {
                Destroy(child.gameObject);
            }
            return;
        }

        Debug.LogWarning("[CapstoneObjectSpawner] No Spawned Parent set, so ClearSpawned has nothing to clear. Set one to use this.");
    }
}
