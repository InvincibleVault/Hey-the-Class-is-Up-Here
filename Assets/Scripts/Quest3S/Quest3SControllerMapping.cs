using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace MetaQuest3S.Input
{
    public enum ControllerHand
    {
        Left,
        Right
    }

    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [System.Serializable]
    public class Vector2Event : UnityEvent<Vector2> { }

    /// <summary>
    /// Button and input mapping script designed specifically for Meta Quest 3S (and Quest 3)
    /// Touch Plus Controllers. Supports both Left (X, Y, Menu) and Right (A, B) controllers.
    /// Provides UnityEvents for easy Inspector wiring, public state accessors, and haptic feedback.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Meta Quest 3S/Quest 3S Controller Mapping")]
    public class Quest3SControllerMapping : MonoBehaviour
    {
        [Header("Controller Configuration")]
        [Tooltip("Specify whether this script maps the Left or Right Quest 3S controller.")]
        [SerializeField] private ControllerHand m_Hand = ControllerHand.Right;

        [Header("Threshold Settings")]
        [Range(0.01f, 0.99f)]
        [Tooltip("Threshold at which analog trigger is considered pressed (Down/Up).")]
        [SerializeField] private float m_TriggerPressThreshold = 0.15f;

        [Range(0.01f, 0.99f)]
        [Tooltip("Threshold at which analog grip is considered pressed (Down/Up).")]
        [SerializeField] private float m_GripPressThreshold = 0.15f;

        [Range(0.01f, 0.99f)]
        [Tooltip("Threshold at which thumbstick registers directional flicks.")]
        [SerializeField] private float m_ThumbstickDeadzone = 0.2f;

        // --- Current State Accessors ---
        public ControllerHand Hand => m_Hand;
        public bool IsConnected => m_Device.isValid;
        public string DeviceName => m_Device.name;

        // Primary Button (X on Left, A on Right)
        public string PrimaryButtonLabel => m_Hand == ControllerHand.Left ? "X Button" : "A Button";
        public bool IsPrimaryPressed { get; private set; }
        public bool IsPrimaryTouched { get; private set; }
        public bool WasPrimaryPressedThisFrame { get; private set; }
        public bool WasPrimaryReleasedThisFrame { get; private set; }

        // Secondary Button (Y on Left, B on Right)
        public string SecondaryButtonLabel => m_Hand == ControllerHand.Left ? "Y Button" : "B Button";
        public bool IsSecondaryPressed { get; private set; }
        public bool IsSecondaryTouched { get; private set; }
        public bool WasSecondaryPressedThisFrame { get; private set; }
        public bool WasSecondaryReleasedThisFrame { get; private set; }

        // Menu Button (Left controller has system menu button; Right has Meta/Oculus button)
        public bool IsMenuPressed { get; private set; }
        public bool WasMenuPressedThisFrame { get; private set; }
        public bool WasMenuReleasedThisFrame { get; private set; }

        // Index Trigger
        public float TriggerValue { get; private set; }
        public bool IsTriggerPressed { get; private set; }
        public bool IsTriggerTouched { get; private set; }
        public bool WasTriggerPressedThisFrame { get; private set; }
        public bool WasTriggerReleasedThisFrame { get; private set; }

        // Grip Button (Hand Trigger)
        public float GripValue { get; private set; }
        public bool IsGripPressed { get; private set; }
        public bool WasGripPressedThisFrame { get; private set; }
        public bool WasGripReleasedThisFrame { get; private set; }

        // Thumbstick
        public Vector2 ThumbstickAxis { get; private set; }
        public bool IsThumbstickClicked { get; private set; }
        public bool IsThumbstickTouched { get; private set; }
        public bool WasThumbstickClickedThisFrame { get; private set; }
        public bool WasThumbstickReleasedThisFrame { get; private set; }

        // Thumbrest (Capacitive rest sensor next to buttons)
        public bool IsThumbrestTouched { get; private set; }
        public bool WasThumbrestTouchedThisFrame { get; private set; }
        public bool WasThumbrestUntouchedThisFrame { get; private set; }

        // --- Unity Events (Inspector hookups) ---
        [Header("Primary Button (Left: X, Right: A)")]
        public UnityEvent onPrimaryDown;
        public UnityEvent onPrimaryUp;
        public UnityEvent onPrimaryTouch;
        public UnityEvent onPrimaryUntouch;

        [Header("Secondary Button (Left: Y, Right: B)")]
        public UnityEvent onSecondaryDown;
        public UnityEvent onSecondaryUp;
        public UnityEvent onSecondaryTouch;
        public UnityEvent onSecondaryUntouch;

        [Header("Menu Button (Left Controller Menu)")]
        public UnityEvent onMenuDown;
        public UnityEvent onMenuUp;

        [Header("Index Trigger")]
        public UnityEvent onTriggerDown;
        public UnityEvent onTriggerUp;
        public FloatEvent onTriggerValueChanged;
        public UnityEvent onTriggerTouch;
        public UnityEvent onTriggerUntouch;

        [Header("Grip (Squeeze) Button")]
        public UnityEvent onGripDown;
        public UnityEvent onGripUp;
        public FloatEvent onGripValueChanged;

        [Header("Thumbstick")]
        public UnityEvent onThumbstickClickDown;
        public UnityEvent onThumbstickClickUp;
        public Vector2Event onThumbstickAxisChanged;
        public UnityEvent onThumbstickTouch;
        public UnityEvent onThumbstickUntouch;

        [Header("Thumbstick Directional Flicks")]
        public UnityEvent onThumbstickFlickUp;
        public UnityEvent onThumbstickFlickDown;
        public UnityEvent onThumbstickFlickLeft;
        public UnityEvent onThumbstickFlickRight;

        [Header("Thumbrest (Capacitive)")]
        public UnityEvent onThumbrestTouch;
        public UnityEvent onThumbrestUntouch;

        [Header("Connection Events")]
        public UnityEvent onControllerConnected;
        public UnityEvent onControllerDisconnected;

        // --- C# Action Callbacks (for programmatic subscriptions) ---
        public event Action<ControllerHand> OnPrimaryDown;
        public event Action<ControllerHand> OnPrimaryUp;
        public event Action<ControllerHand> OnSecondaryDown;
        public event Action<ControllerHand> OnSecondaryUp;
        public event Action<ControllerHand> OnMenuDown;
        public event Action<ControllerHand> OnMenuUp;
        public event Action<ControllerHand> OnTriggerDown;
        public event Action<ControllerHand> OnTriggerUp;
        public event Action<ControllerHand, float> OnTriggerAxis;
        public event Action<ControllerHand> OnGripDown;
        public event Action<ControllerHand> OnGripUp;
        public event Action<ControllerHand, float> OnGripAxis;
        public event Action<ControllerHand> OnThumbstickDown;
        public event Action<ControllerHand> OnThumbstickUp;
        public event Action<ControllerHand, Vector2> OnThumbstickAxis;

        // Internal State Tracking
        private InputDevice m_Device;
        private bool m_WasConnected;

        // Previous frames for state delta detection
        private bool m_PrevPrimary;
        private bool m_PrevPrimaryTouch;
        private bool m_PrevSecondary;
        private bool m_PrevSecondaryTouch;
        private bool m_PrevMenu;
        private bool m_PrevTriggerButton;
        private bool m_PrevTriggerTouch;
        private bool m_PrevGripButton;
        private bool m_PrevThumbstickClick;
        private bool m_PrevThumbstickTouch;
        private bool m_PrevThumbrestTouch;
        private Vector2 m_PrevThumbstickAxis;
        private float m_PrevTriggerValue;
        private float m_PrevGripValue;

        // Directional stick flick latching
        private bool m_StickFlickUpLatched;
        private bool m_StickFlickDownLatched;
        private bool m_StickFlickLeftLatched;
        private bool m_StickFlickRightLatched;

        private void OnEnable()
        {
            InitializeDevice();
            InputDevices.deviceConnected += OnDeviceConnected;
            InputDevices.deviceDisconnected += OnDeviceDisconnected;
        }

        private void OnDisable()
        {
            InputDevices.deviceConnected -= OnDeviceConnected;
            InputDevices.deviceDisconnected -= OnDeviceDisconnected;
            ResetAllStates();
        }

        private void InitializeDevice()
        {
            InputDeviceCharacteristics characteristics = InputDeviceCharacteristics.Controller |
                (m_Hand == ControllerHand.Left ? InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right);

            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(characteristics, devices);

            if (devices.Count > 0)
            {
                m_Device = devices[0];
                if (!m_WasConnected)
                {
                    m_WasConnected = true;
                    onControllerConnected?.Invoke();
                }
            }
            else
            {
                m_Device = default;
                if (m_WasConnected)
                {
                    m_WasConnected = false;
                    onControllerDisconnected?.Invoke();
                }
            }
        }

        private void OnDeviceConnected(InputDevice device)
        {
            if (MatchesHand(device))
            {
                m_Device = device;
                m_WasConnected = true;
                onControllerConnected?.Invoke();
            }
        }

        private void OnDeviceDisconnected(InputDevice device)
        {
            if (device == m_Device)
            {
                m_WasConnected = false;
                m_Device = default;
                ResetAllStates();
                onControllerDisconnected?.Invoke();
            }
        }

        private bool MatchesHand(InputDevice device)
        {
            InputDeviceCharacteristics desired = InputDeviceCharacteristics.Controller |
                (m_Hand == ControllerHand.Left ? InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right);
            return (device.characteristics & desired) == desired;
        }

        private void Update()
        {
            if (!m_Device.isValid)
            {
                InitializeDevice();
                if (!m_Device.isValid) return;
            }

            PollInputs();
        }

        private void PollInputs()
        {
            // Reset frame pulse flags
            WasPrimaryPressedThisFrame = false;
            WasPrimaryReleasedThisFrame = false;
            WasSecondaryPressedThisFrame = false;
            WasSecondaryReleasedThisFrame = false;
            WasMenuPressedThisFrame = false;
            WasMenuReleasedThisFrame = false;
            WasTriggerPressedThisFrame = false;
            WasTriggerReleasedThisFrame = false;
            WasGripPressedThisFrame = false;
            WasGripReleasedThisFrame = false;
            WasThumbstickClickedThisFrame = false;
            WasThumbstickReleasedThisFrame = false;
            WasThumbrestTouchedThisFrame = false;
            WasThumbrestUntouchedThisFrame = false;

            // 1. Primary Button (X on Left, A on Right)
            if (m_Device.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryPress))
            {
                IsPrimaryPressed = primaryPress;
                if (primaryPress && !m_PrevPrimary)
                {
                    WasPrimaryPressedThisFrame = true;
                    onPrimaryDown?.Invoke();
                    OnPrimaryDown?.Invoke(m_Hand);
                }
                else if (!primaryPress && m_PrevPrimary)
                {
                    WasPrimaryReleasedThisFrame = true;
                    onPrimaryUp?.Invoke();
                    OnPrimaryUp?.Invoke(m_Hand);
                }
                m_PrevPrimary = primaryPress;
            }

            // Primary Touch
            if (m_Device.TryGetFeatureValue(CommonUsages.primaryTouch, out bool primaryTouch))
            {
                IsPrimaryTouched = primaryTouch;
                if (primaryTouch && !m_PrevPrimaryTouch)
                    onPrimaryTouch?.Invoke();
                else if (!primaryTouch && m_PrevPrimaryTouch)
                    onPrimaryUntouch?.Invoke();
                m_PrevPrimaryTouch = primaryTouch;
            }

            // 2. Secondary Button (Y on Left, B on Right)
            if (m_Device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryPress))
            {
                IsSecondaryPressed = secondaryPress;
                if (secondaryPress && !m_PrevSecondary)
                {
                    WasSecondaryPressedThisFrame = true;
                    onSecondaryDown?.Invoke();
                    OnSecondaryDown?.Invoke(m_Hand);
                }
                else if (!secondaryPress && m_PrevSecondary)
                {
                    WasSecondaryReleasedThisFrame = true;
                    onSecondaryUp?.Invoke();
                    OnSecondaryUp?.Invoke(m_Hand);
                }
                m_PrevSecondary = secondaryPress;
            }

            // Secondary Touch
            if (m_Device.TryGetFeatureValue(CommonUsages.secondaryTouch, out bool secondaryTouch))
            {
                IsSecondaryTouched = secondaryTouch;
                if (secondaryTouch && !m_PrevSecondaryTouch)
                    onSecondaryTouch?.Invoke();
                else if (!secondaryTouch && m_PrevSecondaryTouch)
                    onSecondaryUntouch?.Invoke();
                m_PrevSecondaryTouch = secondaryTouch;
            }

            // 3. Menu Button (Left controller menu)
            if (m_Device.TryGetFeatureValue(CommonUsages.menuButton, out bool menuPress))
            {
                IsMenuPressed = menuPress;
                if (menuPress && !m_PrevMenu)
                {
                    WasMenuPressedThisFrame = true;
                    onMenuDown?.Invoke();
                    OnMenuDown?.Invoke(m_Hand);
                }
                else if (!menuPress && m_PrevMenu)
                {
                    WasMenuReleasedThisFrame = true;
                    onMenuUp?.Invoke();
                    OnMenuUp?.Invoke(m_Hand);
                }
                m_PrevMenu = menuPress;
            }

            // 4. Index Trigger (Analog float + bool click)
            m_Device.TryGetFeatureValue(CommonUsages.trigger, out float triggerVal);
            TriggerValue = triggerVal;

            if (Mathf.Abs(triggerVal - m_PrevTriggerValue) > 0.001f)
            {
                onTriggerValueChanged?.Invoke(triggerVal);
                OnTriggerAxis?.Invoke(m_Hand, triggerVal);
                m_PrevTriggerValue = triggerVal;
            }

            bool triggerPressed = triggerVal >= m_TriggerPressThreshold;
            if (m_Device.TryGetFeatureValue(CommonUsages.triggerButton, out bool explicitTriggerButton))
            {
                triggerPressed = triggerPressed || explicitTriggerButton;
            }
            IsTriggerPressed = triggerPressed;

            if (triggerPressed && !m_PrevTriggerButton)
            {
                WasTriggerPressedThisFrame = true;
                onTriggerDown?.Invoke();
                OnTriggerDown?.Invoke(m_Hand);
            }
            else if (!triggerPressed && m_PrevTriggerButton)
            {
                WasTriggerReleasedThisFrame = true;
                onTriggerUp?.Invoke();
                OnTriggerUp?.Invoke(m_Hand);
            }
            m_PrevTriggerButton = triggerPressed;

            // Trigger Touch (Capacitive)
            if (m_Device.TryGetFeatureValue(new InputFeatureUsage<bool>("TriggerTouch"), out bool triggerTouch))
            {
                IsTriggerTouched = triggerTouch;
                if (triggerTouch && !m_PrevTriggerTouch)
                    onTriggerTouch?.Invoke();
                else if (!triggerTouch && m_PrevTriggerTouch)
                    onTriggerUntouch?.Invoke();
                m_PrevTriggerTouch = triggerTouch;
            }

            // 5. Grip / Squeeze Button (Analog float + bool click)
            m_Device.TryGetFeatureValue(CommonUsages.grip, out float gripVal);
            GripValue = gripVal;

            if (Mathf.Abs(gripVal - m_PrevGripValue) > 0.001f)
            {
                onGripValueChanged?.Invoke(gripVal);
                OnGripAxis?.Invoke(m_Hand, gripVal);
                m_PrevGripValue = gripVal;
            }

            bool gripPressed = gripVal >= m_GripPressThreshold;
            if (m_Device.TryGetFeatureValue(CommonUsages.gripButton, out bool explicitGripButton))
            {
                gripPressed = gripPressed || explicitGripButton;
            }
            IsGripPressed = gripPressed;

            if (gripPressed && !m_PrevGripButton)
            {
                WasGripPressedThisFrame = true;
                onGripDown?.Invoke();
                OnGripDown?.Invoke(m_Hand);
            }
            else if (!gripPressed && m_PrevGripButton)
            {
                WasGripReleasedThisFrame = true;
                onGripUp?.Invoke();
                OnGripUp?.Invoke(m_Hand);
            }
            m_PrevGripButton = gripPressed;

            // 6. Thumbstick (2D Axis + Click + Touch)
            if (m_Device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 stickAxis))
            {
                ThumbstickAxis = stickAxis;
                if ((stickAxis - m_PrevThumbstickAxis).sqrMagnitude > 0.0001f)
                {
                    onThumbstickAxisChanged?.Invoke(stickAxis);
                    OnThumbstickAxis?.Invoke(m_Hand, stickAxis);
                    m_PrevThumbstickAxis = stickAxis;
                }

                // Directional flicks with latching
                PollThumbstickFlicks(stickAxis);
            }

            if (m_Device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool stickClick))
            {
                IsThumbstickClicked = stickClick;
                if (stickClick && !m_PrevThumbstickClick)
                {
                    WasThumbstickClickedThisFrame = true;
                    onThumbstickClickDown?.Invoke();
                    OnThumbstickDown?.Invoke(m_Hand);
                }
                else if (!stickClick && m_PrevThumbstickClick)
                {
                    WasThumbstickReleasedThisFrame = true;
                    onThumbstickClickUp?.Invoke();
                    OnThumbstickUp?.Invoke(m_Hand);
                }
                m_PrevThumbstickClick = stickClick;
            }

            if (m_Device.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out bool stickTouch))
            {
                IsThumbstickTouched = stickTouch;
                if (stickTouch && !m_PrevThumbstickTouch)
                    onThumbstickTouch?.Invoke();
                else if (!stickTouch && m_PrevThumbstickTouch)
                    onThumbstickUntouch?.Invoke();
                m_PrevThumbstickTouch = stickTouch;
            }

            // 7. Thumbrest Capacitive Sensor
            // In Unity OpenXR/Oculus runtime, thumbrest touch is exposed via "ThumbTouch" or secondary2DAxisTouch
            bool thumbrest = false;
            if (m_Device.TryGetFeatureValue(new InputFeatureUsage<bool>("ThumbTouch"), out bool tt))
            {
                thumbrest = tt;
            }
            else if (m_Device.TryGetFeatureValue(CommonUsages.secondary2DAxisTouch, out bool st))
            {
                thumbrest = st;
            }

            IsThumbrestTouched = thumbrest;
            if (thumbrest && !m_PrevThumbrestTouch)
            {
                WasThumbrestTouchedThisFrame = true;
                onThumbrestTouch?.Invoke();
            }
            else if (!thumbrest && m_PrevThumbrestTouch)
            {
                WasThumbrestUntouchedThisFrame = true;
                onThumbrestUntouch?.Invoke();
            }
            m_PrevThumbrestTouch = thumbrest;
        }

        private void PollThumbstickFlicks(Vector2 axis)
        {
            // Up
            if (axis.y > m_ThumbstickDeadzone)
            {
                if (!m_StickFlickUpLatched)
                {
                    m_StickFlickUpLatched = true;
                    onThumbstickFlickUp?.Invoke();
                }
            }
            else if (axis.y < m_ThumbstickDeadzone * 0.5f)
            {
                m_StickFlickUpLatched = false;
            }

            // Down
            if (axis.y < -m_ThumbstickDeadzone)
            {
                if (!m_StickFlickDownLatched)
                {
                    m_StickFlickDownLatched = true;
                    onThumbstickFlickDown?.Invoke();
                }
            }
            else if (axis.y > -m_ThumbstickDeadzone * 0.5f)
            {
                m_StickFlickDownLatched = false;
            }

            // Right
            if (axis.x > m_ThumbstickDeadzone)
            {
                if (!m_StickFlickRightLatched)
                {
                    m_StickFlickRightLatched = true;
                    onThumbstickFlickRight?.Invoke();
                }
            }
            else if (axis.x < m_ThumbstickDeadzone * 0.5f)
            {
                m_StickFlickRightLatched = false;
            }

            // Left
            if (axis.x < -m_ThumbstickDeadzone)
            {
                if (!m_StickFlickLeftLatched)
                {
                    m_StickFlickLeftLatched = true;
                    onThumbstickFlickLeft?.Invoke();
                }
            }
            else if (axis.x > -m_ThumbstickDeadzone * 0.5f)
            {
                m_StickFlickLeftLatched = false;
            }
        }

        /// <summary>
        /// Sends a haptic vibration impulse to this Meta Quest 3S controller.
        /// </summary>
        /// <param name="amplitude">Intensity between 0.0 and 1.0</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="channel">Haptic motor channel (usually 0)</param>
        public void SendHapticImpulse(float amplitude = 0.5f, float duration = 0.1f, uint channel = 0)
        {
            if (m_Device.isValid && m_Device.TryGetHapticCapabilities(out HapticCapabilities caps))
            {
                if (caps.supportsImpulse)
                {
                    m_Device.SendHapticImpulse(channel, Mathf.Clamp01(amplitude), Mathf.Max(0.01f, duration));
                }
            }
        }

        /// <summary>
        /// Stops any currently playing haptic vibration on this controller.
        /// </summary>
        public void StopHaptics()
        {
            if (m_Device.isValid)
            {
                m_Device.StopHaptics();
            }
        }

        private void ResetAllStates()
        {
            IsPrimaryPressed = false;
            IsPrimaryTouched = false;
            IsSecondaryPressed = false;
            IsSecondaryTouched = false;
            IsMenuPressed = false;
            IsTriggerPressed = false;
            IsTriggerTouched = false;
            TriggerValue = 0f;
            IsGripPressed = false;
            GripValue = 0f;
            ThumbstickAxis = Vector2.zero;
            IsThumbstickClicked = false;
            IsThumbstickTouched = false;
            IsThumbrestTouched = false;

            m_PrevPrimary = false;
            m_PrevPrimaryTouch = false;
            m_PrevSecondary = false;
            m_PrevSecondaryTouch = false;
            m_PrevMenu = false;
            m_PrevTriggerButton = false;
            m_PrevTriggerTouch = false;
            m_PrevGripButton = false;
            m_PrevThumbstickClick = false;
            m_PrevThumbstickTouch = false;
            m_PrevThumbrestTouch = false;
            m_PrevThumbstickAxis = Vector2.zero;
            m_PrevTriggerValue = 0f;
            m_PrevGripValue = 0f;

            m_StickFlickUpLatched = false;
            m_StickFlickDownLatched = false;
            m_StickFlickLeftLatched = false;
            m_StickFlickRightLatched = false;
        }

        /// <summary>
        /// Set hand at runtime if needed.
        /// </summary>
        public void SetHand(ControllerHand newHand)
        {
            if (m_Hand != newHand)
            {
                m_Hand = newHand;
                ResetAllStates();
                InitializeDevice();
            }
        }
    }
}
