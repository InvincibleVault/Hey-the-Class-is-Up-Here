using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("The prefab or object to spawn")]
    public GameObject prefabToSpawn;

    [Tooltip("Reference to the player or camera. If left empty, Camera.main will be used automatically.")]
    public Transform playerTransform;

    [Tooltip("Distance in front of the player to spawn the object")]
    public float spawnDistance = 1.5f;

    [Tooltip("Vertical height offset relative to the player/camera")]
    public float heightOffset = 0f;

    [Tooltip("If true, ignores camera pitch/tilt so the object spawns level on the ground plane")]
    public bool stayLevel = false;

    [Tooltip("If true, spawned object matches the player's facing direction")]
    public bool matchPlayerRotation = true;

    [Header("Debug")]
    [Tooltip("Press this key in play mode to test spawning (e.g., Space)")]
    public KeyCode debugSpawnKey = KeyCode.Space;

    private void Start()
    {
        // Fallback to Main Camera if no player transform is explicitly assigned
        if (playerTransform == null && Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        // Quick testing in the editor
        if (debugSpawnKey != KeyCode.None && Input.GetKeyDown(debugSpawnKey))
        {
            SpawnObject();
        }
    }

    /// <summary>
    /// Spawns the prefab directly in front of the player.
    /// Can be called from UI Button OnClick events or other scripts.
    /// </summary>
    public void SpawnObject()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("ObjectSpawner: No prefab assigned to spawn!");
            return;
        }

        // If playerTransform is still null, try finding Camera.main
        Transform referenceTransform = playerTransform;
        if (referenceTransform == null && Camera.main != null)
        {
            referenceTransform = Camera.main.transform;
        }

        if (referenceTransform == null)
        {
            Debug.LogError("ObjectSpawner: No player or main camera found to spawn in front of!");
            return;
        }

        Vector3 forwardDirection = referenceTransform.forward;

        if (stayLevel)
        {
            // Flatten forward vector to keep spawn level with the floor/horizon
            forwardDirection.y = 0;
            forwardDirection.Normalize();
        }

        Vector3 spawnPosition = referenceTransform.position + (forwardDirection * spawnDistance) + (Vector3.up * heightOffset);
        
        Quaternion spawnRotation = matchPlayerRotation 
            ? Quaternion.LookRotation(forwardDirection) 
            : Quaternion.identity;

        Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
    }
}
