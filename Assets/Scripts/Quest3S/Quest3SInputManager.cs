using UnityEngine;

namespace MetaQuest3S.Input
{
    public enum Quest3SButton
    {
        // Left Controller
        X,
        Y,
        LeftMenu,
        LeftTrigger,
        LeftGrip,
        LeftThumbstickClick,
        
        // Right Controller
        A,
        B,
        RightTrigger,
        RightGrip,
        RightThumbstickClick
    }

    /// <summary>
    /// Central manager for Meta Quest 3S controllers.
    /// Provides convenient static and singleton access to both Left and Right Quest 3S controller mappings.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [AddComponentMenu("Meta Quest 3S/Quest 3S Input Manager")]
    public class Quest3SInputManager : MonoBehaviour
    {
        private static Quest3SInputManager s_Instance;
        public static Quest3SInputManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindFirstObjectByType<Quest3SInputManager>();
                    if (s_Instance == null)
                    {
                        GameObject go = new GameObject("Quest3S_InputManager");
                        s_Instance = go.AddComponent<Quest3SInputManager>();
                    }
                }
                return s_Instance;
            }
        }

        [Header("Controller Mappings")]
        [SerializeField] private Quest3SControllerMapping m_LeftController;
        [SerializeField] private Quest3SControllerMapping m_RightController;

        [Header("Manager Settings")]
        [Tooltip("Keep manager alive across scene loads.")]
        [SerializeField] private bool m_DontDestroyOnLoad = true;

        public Quest3SControllerMapping Left => m_LeftController;
        public Quest3SControllerMapping Right => m_RightController;

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
            if (m_DontDestroyOnLoad && transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            EnsureControllers();
        }

        private void EnsureControllers()
        {
            if (m_LeftController == null)
            {
                // Look for existing left controller mapping in children or scene
                Quest3SControllerMapping[] mappings = GetComponentsInChildren<Quest3SControllerMapping>();
                foreach (var map in mappings)
                {
                    if (map.Hand == ControllerHand.Left)
                    {
                        m_LeftController = map;
                        break;
                    }
                }

                if (m_LeftController == null)
                {
                    GameObject leftGo = new GameObject("Left_Quest3S_Controller");
                    leftGo.transform.SetParent(transform, false);
                    m_LeftController = leftGo.AddComponent<Quest3SControllerMapping>();
                    m_LeftController.SetHand(ControllerHand.Left);
                }
            }

            if (m_RightController == null)
            {
                // Look for existing right controller mapping in children or scene
                Quest3SControllerMapping[] mappings = GetComponentsInChildren<Quest3SControllerMapping>();
                foreach (var map in mappings)
                {
                    if (map.Hand == ControllerHand.Right)
                    {
                        m_RightController = map;
                        break;
                    }
                }

                if (m_RightController == null)
                {
                    GameObject rightGo = new GameObject("Right_Quest3S_Controller");
                    rightGo.transform.SetParent(transform, false);
                    m_RightController = rightGo.AddComponent<Quest3SControllerMapping>();
                    m_RightController.SetHand(ControllerHand.Right);
                }
            }
        }

        /// <summary>
        /// Check if a specific Meta Quest 3S button is currently held down.
        /// </summary>
        public static bool GetButton(Quest3SButton button)
        {
            if (Instance == null) return false;
            return button switch
            {
                Quest3SButton.X => Instance.Left != null && Instance.Left.IsPrimaryPressed,
                Quest3SButton.Y => Instance.Left != null && Instance.Left.IsSecondaryPressed,
                Quest3SButton.LeftMenu => Instance.Left != null && Instance.Left.IsMenuPressed,
                Quest3SButton.LeftTrigger => Instance.Left != null && Instance.Left.IsTriggerPressed,
                Quest3SButton.LeftGrip => Instance.Left != null && Instance.Left.IsGripPressed,
                Quest3SButton.LeftThumbstickClick => Instance.Left != null && Instance.Left.IsThumbstickClicked,

                Quest3SButton.A => Instance.Right != null && Instance.Right.IsPrimaryPressed,
                Quest3SButton.B => Instance.Right != null && Instance.Right.IsSecondaryPressed,
                Quest3SButton.RightTrigger => Instance.Right != null && Instance.Right.IsTriggerPressed,
                Quest3SButton.RightGrip => Instance.Right != null && Instance.Right.IsGripPressed,
                Quest3SButton.RightThumbstickClick => Instance.Right != null && Instance.Right.IsThumbstickClicked,
                _ => false
            };
        }

        /// <summary>
        /// Check if a specific Meta Quest 3S button was pressed down this frame.
        /// </summary>
        public static bool GetButtonDown(Quest3SButton button)
        {
            if (Instance == null) return false;
            return button switch
            {
                Quest3SButton.X => Instance.Left != null && Instance.Left.WasPrimaryPressedThisFrame,
                Quest3SButton.Y => Instance.Left != null && Instance.Left.WasSecondaryPressedThisFrame,
                Quest3SButton.LeftMenu => Instance.Left != null && Instance.Left.WasMenuPressedThisFrame,
                Quest3SButton.LeftTrigger => Instance.Left != null && Instance.Left.WasTriggerPressedThisFrame,
                Quest3SButton.LeftGrip => Instance.Left != null && Instance.Left.WasGripPressedThisFrame,
                Quest3SButton.LeftThumbstickClick => Instance.Left != null && Instance.Left.WasThumbstickClickedThisFrame,

                Quest3SButton.A => Instance.Right != null && Instance.Right.WasPrimaryPressedThisFrame,
                Quest3SButton.B => Instance.Right != null && Instance.Right.WasSecondaryPressedThisFrame,
                Quest3SButton.RightTrigger => Instance.Right != null && Instance.Right.WasTriggerPressedThisFrame,
                Quest3SButton.RightGrip => Instance.Right != null && Instance.Right.WasGripPressedThisFrame,
                Quest3SButton.RightThumbstickClick => Instance.Right != null && Instance.Right.WasThumbstickClickedThisFrame,
                _ => false
            };
        }

        /// <summary>
        /// Check if a specific Meta Quest 3S button was released this frame.
        /// </summary>
        public static bool GetButtonUp(Quest3SButton button)
        {
            if (Instance == null) return false;
            return button switch
            {
                Quest3SButton.X => Instance.Left != null && Instance.Left.WasPrimaryReleasedThisFrame,
                Quest3SButton.Y => Instance.Left != null && Instance.Left.WasSecondaryReleasedThisFrame,
                Quest3SButton.LeftMenu => Instance.Left != null && Instance.Left.WasMenuReleasedThisFrame,
                Quest3SButton.LeftTrigger => Instance.Left != null && Instance.Left.WasTriggerReleasedThisFrame,
                Quest3SButton.LeftGrip => Instance.Left != null && Instance.Left.WasGripReleasedThisFrame,
                Quest3SButton.LeftThumbstickClick => Instance.Left != null && Instance.Left.WasThumbstickReleasedThisFrame,

                Quest3SButton.A => Instance.Right != null && Instance.Right.WasPrimaryReleasedThisFrame,
                Quest3SButton.B => Instance.Right != null && Instance.Right.WasSecondaryReleasedThisFrame,
                Quest3SButton.RightTrigger => Instance.Right != null && Instance.Right.WasTriggerReleasedThisFrame,
                Quest3SButton.RightGrip => Instance.Right != null && Instance.Right.WasGripReleasedThisFrame,
                Quest3SButton.RightThumbstickClick => Instance.Right != null && Instance.Right.WasThumbstickReleasedThisFrame,
                _ => false
            };
        }

        /// <summary>
        /// Trigger haptic vibration on left, right, or both controllers.
        /// </summary>
        public static void Vibrate(ControllerHand hand, float amplitude = 0.5f, float duration = 0.1f)
        {
            if (Instance == null) return;
            if (hand == ControllerHand.Left && Instance.Left != null)
                Instance.Left.SendHapticImpulse(amplitude, duration);
            else if (hand == ControllerHand.Right && Instance.Right != null)
                Instance.Right.SendHapticImpulse(amplitude, duration);
        }
    }
}
