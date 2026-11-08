using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SpawnButton: Component attached to a UI Button that spawns an object when clicked or poked.
/// Also marks the spawned object as the active object so controller buttons (e.g. 'A')
/// continue spawning this object.
/// </summary>
[RequireComponent(typeof(Button))]
public class SpawnButton : MonoBehaviour
{
    [Tooltip("The spawner in the scene. Drag the GameObject with CapstoneObjectSpawner here.")]
    [SerializeField] private CapstoneObjectSpawner spawner;

    [Tooltip("The object this button spawns. Leave empty to use Prefab Index instead.")]
    [SerializeField] private GameObject objectToSpawn;

    [Tooltip("Index into the spawner's Spawnable Prefabs list. Only used when Object To Spawn is empty.")]
    [SerializeField] private int prefabIndex = -1;

    private Button m_Button;

    private void Awake()
    {
        m_Button = GetComponent<Button>();
        if (m_Button != null)
        {
            m_Button.onClick.AddListener(OnButtonClicked);
        }

        if (spawner == null)
        {
            spawner = FindFirstObjectByType<CapstoneObjectSpawner>();
        }
    }

    public void Spawn()
    {
        OnButtonClicked();
    }

    private void OnButtonClicked()
    {
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<CapstoneObjectSpawner>();
            if (spawner == null)
            {
                Debug.LogWarning($"[SpawnButton] No spawner assigned on button '{gameObject.name}' and none found in the scene.");
                return;
            }
        }

        if (objectToSpawn != null)
        {
            spawner.SetActivePrefab(objectToSpawn);
            spawner.SpawnPrefab(objectToSpawn);
        }
        else if (prefabIndex >= 0)
        {
            spawner.SetActiveIndex(prefabIndex);
            spawner.SpawnObject(prefabIndex);
        }
        else
        {
            Debug.LogWarning($"[SpawnButton] Button '{gameObject.name}' has neither a prefab nor an index. Assign one.");
        }
    }
}
