using UnityEngine;
using UnityEngine.UI;

// SpawnButton: put this on a UI Button and it spawns one object.
// This is the "maps directly to the buttons" piece: each button owns
// exactly one object, so the button layout in the scene IS the spawn menu.
//
// Two ways to map a button, pick one per button:
//   A. Drag a prefab into Object To Spawn. The button spawns that prefab.
//   B. Leave Object To Spawn empty and set Prefab Index to match the
//      CapstoneObjectSpawner's Spawnable Prefabs list (button 0 -> index 0, etc).
// If both are set, the direct prefab wins.
[RequireComponent(typeof(Button))]
public class SpawnButton : MonoBehaviour
{
    [Tooltip("The spawner in the scene. Drag the GameObject with CapstoneObjectSpawner here.")]
    [SerializeField] private CapstoneObjectSpawner spawner;

    [Tooltip("The object this button spawns. Leave empty to use Prefab Index instead.")]
    [SerializeField] private GameObject objectToSpawn;

    [Tooltip("Index into the spawner's Spawnable Prefabs list. Only used when Object To Spawn is empty.")]
    [SerializeField] private int prefabIndex = -1;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (spawner == null)
        {
            Debug.LogWarning($"[SpawnButton] No spawner assigned on button '{gameObject.name}'. Drag the CapstoneObjectSpawner here.");
            return;
        }

        if (objectToSpawn != null)
        {
            spawner.SpawnPrefab(objectToSpawn);
        }
        else if (prefabIndex >= 0)
        {
            spawner.SpawnObject(prefabIndex);
        }
        else
        {
            Debug.LogWarning($"[SpawnButton] Button '{gameObject.name}' has neither a prefab nor an index. Assign one.");
        }
    }
}
