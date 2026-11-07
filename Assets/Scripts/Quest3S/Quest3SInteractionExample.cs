using UnityEngine;

namespace MetaQuest3S.Input
{
    /// <summary>
    /// Practical example showing how to listen to Quest 3S button mapping events and poll inputs in Update.
    /// </summary>
    public class Quest3SInteractionExample : MonoBehaviour
    {
        [Header("Controller Mappings")]
        [SerializeField] private Quest3SControllerMapping m_LeftController;
        [SerializeField] private Quest3SControllerMapping m_RightController;

        [Header("Demo Target Objects")]
        [SerializeField] private GameObject m_MenuCanvas;

        private void Start()
        {
            // Example 1: Registering via C# events
            if (m_LeftController != null)
            {
                // Primary button on Left is 'X'
                m_LeftController.OnPrimaryDown += OnXButtonPressed;
                // Secondary button on Left is 'Y'
                m_LeftController.OnSecondaryDown += OnYButtonPressed;
                // Left Menu button
                m_LeftController.OnMenuDown += OnLeftMenuPressed;
            }

            if (m_RightController != null)
            {
                // Primary button on Right is 'A'
                m_RightController.OnPrimaryDown += OnAButtonPressed;
                // Secondary button on Right is 'B'
                m_RightController.OnSecondaryDown += OnBButtonPressed;
            }
        }

        private void Update()
        {
            // Example 2: Polling state directly in Update()
            if (m_RightController != null && m_RightController.WasTriggerPressedThisFrame)
            {
                Debug.Log("Right Index Trigger clicked! Haptic pulse triggered.");
                m_RightController.SendHapticImpulse(amplitude: 0.6f, duration: 0.08f);
            }

            // Example 3: Polling via Quest3SInputManager static helpers
            if (Quest3SInputManager.GetButtonDown(Quest3SButton.A))
            {
                Debug.Log("A button pressed down via static Quest3SInputManager!");
            }
        }

        private void OnXButtonPressed(ControllerHand hand)
        {
            Debug.Log("[Quest 3S] Left X Button pressed!");
        }

        private void OnYButtonPressed(ControllerHand hand)
        {
            Debug.Log("[Quest 3S] Left Y Button pressed!");
        }

        private void OnAButtonPressed(ControllerHand hand)
        {
            Debug.Log("[Quest 3S] Right A Button pressed! (e.g. Jump / Confirm)");
        }

        private void OnBButtonPressed(ControllerHand hand)
        {
            Debug.Log("[Quest 3S] Right B Button pressed! (e.g. Back / Cancel)");
        }

        private void OnLeftMenuPressed(ControllerHand hand)
        {
            Debug.Log("[Quest 3S] Left Menu Button pressed! Toggling UI...");
            if (m_MenuCanvas != null)
            {
                m_MenuCanvas.SetActive(!m_MenuCanvas.activeSelf);
            }
        }
    }
}
