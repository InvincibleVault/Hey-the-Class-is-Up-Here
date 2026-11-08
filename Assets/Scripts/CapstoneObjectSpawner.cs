using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using MetaQuest3S.Input;

public enum ControllerSpawnButton
{
    A_Or_Primary,
    RightTrigger,
    LeftTrigger,
    X_Button,
    AnyPrimaryOrTrigger
}

/// <summary>
/// CapstoneObjectSpawner: Manages spawning 3D objects in the capstone VR prototype.
/// Supports spawning through:
///  1. In-VR UI buttons (ray click or physical poke) via SpawnButton.
///  2. VR controller buttons (e.g. Meta Quest 3S 'A' button, Trigger, or 'X').
///  3. Keyboard shortcuts in Editor Play Mode (Space = active, 1/2/3 = prefab, C = clear).
/// </summary>
public class CapstoneObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Where spawned objects appear. Leave empty to use this GameObject's position.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Optional parent for spawned objects, keeps the Hierarchy tidy. Leave empty for scene root.")]
    [SerializeField] private Transform spawnedParent;

    [Tooltip("Random offset applied to each spawn position, per axis. Zero means exact spawn point.")]
    [SerializeField] private Vector3 positionJitter = new Vector3(0.05f, 0.02f, 0.05f);

    [Tooltip("If true, each spawned object gets a random Y rotation.")]
    [SerializeField] private bool randomizeRotationY = true;

    [Header("Objects")]
    [Tooltip("Prefabs in button order. Button 0 spawns index 0, button 1 spawns index 1, and so on.")]
    [SerializeField] private List<GameObject> spawnablePrefabs = new List<GameObject>();

    [Tooltip("Index of the currently active prefab to spawn when pressing controller buttons or Spacebar.")]
    [SerializeField] private int activePrefabIndex = 0;

    [Header("Controller Button Push (Meta Quest 3S / XR)")]
    [Tooltip("Enable spawning objects when pressing a controller button (e.g. 'A').")]
    [SerializeField] private bool enableControllerButtonSpawn = true;

    [Tooltip("Which controller button triggers object spawning.")]
    [SerializeField] private ControllerSpawnButton controllerButton = ControllerSpawnButton.A_Or_Primary;

    [Tooltip("Play haptic rumble when an object is spawned.")]
    [SerializeField] private bool enableHaptics = true;

    [Header("Editor Keyboard Shortcuts (Play Mode)")]
    [Tooltip("Enable testing in Editor Play Mode using keyboard (Space = spawn active, 1/2/3 = spawn specific, C = clear).")]
    [SerializeField] private bool enableKeyboardShortcuts = true;

    private bool m_WasXrButtonPressed = false;

    public int ActivePrefabIndex => activePrefabIndex;
    public Transform SpawnPoint => spawnPoint;

    public void SetActiveIndex(int index)
    {
        if (index >= 0 && index < spawnablePrefabs.Count)
        {
            activePrefabIndex = index;
        }
    }

    public void SetActivePrefab(GameObject prefab)
    {
        if (prefab == null) return;
        int idx = spawnablePrefabs.IndexOf(prefab);
        if (idx >= 0)
        {
            activePrefabIndex = idx;
        }
    }

    private void Update()
    {
        // 1. Controller button press
        if (enableControllerButtonSpawn && CheckControllerButtonDown())
        {
            SpawnActiveObject();
        }

        // 2. Editor keyboard shortcuts
        if (enableKeyboardShortcuts)
        {
            CheckKeyboardShortcuts();
        }
    }

    // Spawn the currently selected object (activePrefabIndex)
    public void SpawnActiveObject()
    {
        if (spawnablePrefabs == null || spawnablePrefabs.Count == 0)
        {
            Debug.LogWarning("[CapstoneObjectSpawner] No prefabs in Spawnable Prefabs list.");
            return;
        }

        if (activePrefabIndex < 0 || activePrefabIndex >= spawnablePrefabs.Count)
        {
            activePrefabIndex = 0;
        }

        SpawnPrefab(spawnablePrefabs[activePrefabIndex]);
    }

    // Spawn by button index. Safe to wire straight into a Button's OnClick event.
    public void SpawnObject(int index)
    {
        if (index < 0 || index >= spawnablePrefabs.Count)
        {
            Debug.LogWarning($"[CapstoneObjectSpawner] No prefab at index {index}. Check the Spawnable Prefabs list.");
            return;
        }

        activePrefabIndex = index;
        SpawnPrefab(spawnablePrefabs[index]);
    }

    // Spawn a specific prefab.
    public void SpawnPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[CapstoneObjectSpawner] Tried to spawn a null prefab. Assign one on the button or in the list.");
            return;
        }

        int idx = spawnablePrefabs.IndexOf(prefab);
        if (idx >= 0)
        {
            activePrefabIndex = idx;
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

        TriggerHaptics();
    }

    // Removes everything this spawner created.
    public void ClearSpawned()
    {
        if (spawnedParent != null)
        {
            foreach (Transform child in spawnedParent)
            {
                Destroy(child.gameObject);
            }
            TriggerHaptics(amplitude: 0.3f, duration: 0.08f);
            return;
        }

        Debug.LogWarning("[CapstoneObjectSpawner] No Spawned Parent set, so ClearSpawned has nothing to clear.");
    }

    // Controller input polling
    private bool CheckControllerButtonDown()
    {
        // 1. Meta Quest 3S central input manager
        if (Quest3SInputManager.Instance != null)
        {
            switch (controllerButton)
            {
                case ControllerSpawnButton.A_Or_Primary:
                    if (Quest3SInputManager.GetButtonDown(Quest3SButton.A)) return true;
                    break;
                case ControllerSpawnButton.X_Button:
                    if (Quest3SInputManager.GetButtonDown(Quest3SButton.X)) return true;
                    break;
                case ControllerSpawnButton.RightTrigger:
                    if (Quest3SInputManager.GetButtonDown(Quest3SButton.RightTrigger)) return true;
                    break;
                case ControllerSpawnButton.LeftTrigger:
                    if (Quest3SInputManager.GetButtonDown(Quest3SButton.LeftTrigger)) return true;
                    break;
                case ControllerSpawnButton.AnyPrimaryOrTrigger:
                    if (Quest3SInputManager.GetButtonDown(Quest3SButton.A) ||
                        Quest3SInputManager.GetButtonDown(Quest3SButton.X) ||
                        Quest3SInputManager.GetButtonDown(Quest3SButton.RightTrigger) ||
                        Quest3SInputManager.GetButtonDown(Quest3SButton.LeftTrigger)) return true;
                    break;
            }
        }

        // 2. Direct XR InputDevices fallback
        bool buttonPressedNow = false;

        if (controllerButton == ControllerSpawnButton.AnyPrimaryOrTrigger)
        {
            var allControllers = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, allControllers);
            foreach (var dev in allControllers)
            {
                if ((dev.TryGetFeatureValue(CommonUsages.primaryButton, out bool p) && p) ||
                    (dev.TryGetFeatureValue(CommonUsages.triggerButton, out bool t) && t))
                {
                    buttonPressedNow = true;
                    break;
                }
            }
        }
        else
        {
            InputDeviceCharacteristics handChar = (controllerButton == ControllerSpawnButton.X_Button || controllerButton == ControllerSpawnButton.LeftTrigger)
                ? (InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller)
                : (InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller);

            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(handChar, devices);

            InputFeatureUsage<bool> feature = (controllerButton == ControllerSpawnButton.RightTrigger || controllerButton == ControllerSpawnButton.LeftTrigger)
                ? CommonUsages.triggerButton
                : CommonUsages.primaryButton;

            foreach (var device in devices)
            {
                if (device.TryGetFeatureValue(feature, out bool pressed) && pressed)
                {
                    buttonPressedNow = true;
                    break;
                }
            }
        }

        if (buttonPressedNow)
        {
            if (!m_WasXrButtonPressed)
            {
                m_WasXrButtonPressed = true;
                return true;
            }
        }
        else
        {
            m_WasXrButtonPressed = false;
        }

        return false;
    }

    private void CheckKeyboardShortcuts()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null)
        {
            if (kb.spaceKey.wasPressedThisFrame) SpawnActiveObject();
            if (kb.digit1Key.wasPressedThisFrame) SpawnObject(0);
            if (kb.digit2Key.wasPressedThisFrame) SpawnObject(1);
            if (kb.digit3Key.wasPressedThisFrame) SpawnObject(2);
            if (kb.cKey.wasPressedThisFrame) ClearSpawned();
            return;
        }
#endif
    }

    public void TriggerHaptics(float amplitude = 0.5f, float duration = 0.1f)
    {
        if (!enableHaptics) return;

        // Try Quest 3S controller mapping first
        if (Quest3SInputManager.Instance != null && Quest3SInputManager.Instance.Right != null)
        {
            Quest3SInputManager.Instance.Right.SendHapticImpulse(amplitude, duration);
            return;
        }

        // Direct XR fallback
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, devices);
        foreach (var dev in devices)
        {
            if (dev.TryGetHapticCapabilities(out HapticCapabilities caps) && caps.supportsImpulse)
            {
                dev.SendHapticImpulse(0, amplitude, duration);
            }
        }
    }
}
