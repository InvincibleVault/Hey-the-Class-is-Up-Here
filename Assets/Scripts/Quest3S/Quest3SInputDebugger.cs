using UnityEngine;

namespace MetaQuest3S.Input
{
    /// <summary>
    /// Drop-in visual debugger for testing Meta Quest 3S controllers.
    /// Draws an OnGUI status overlay showing live input values and testing haptic feedback.
    /// </summary>
    [AddComponentMenu("Meta Quest 3S/Quest 3S Input Debugger")]
    public class Quest3SInputDebugger : MonoBehaviour
    {
        [Header("Debugger Settings")]
        [SerializeField] private bool m_ShowGUI = true;
        [SerializeField] private bool m_LogEventsToConsole = false;

        [Header("References (Optional - will find automatically if empty)")]
        [SerializeField] private Quest3SControllerMapping m_LeftController;
        [SerializeField] private Quest3SControllerMapping m_RightController;

        private void Start()
        {
            if (m_LeftController == null || m_RightController == null)
            {
                var mappings = FindObjectsByType<Quest3SControllerMapping>(FindObjectsSortMode.None);
                foreach (var map in mappings)
                {
                    if (map.Hand == ControllerHand.Left && m_LeftController == null)
                        m_LeftController = map;
                    else if (map.Hand == ControllerHand.Right && m_RightController == null)
                        m_RightController = map;
                }
            }

            if (m_LogEventsToConsole)
            {
                SubscribeLogger(m_LeftController, "Left Quest 3S");
                SubscribeLogger(m_RightController, "Right Quest 3S");
            }
        }

        private void SubscribeLogger(Quest3SControllerMapping controller, string prefix)
        {
            if (controller == null) return;

            controller.onPrimaryDown.AddListener(() => Debug.Log($"[{prefix}] {controller.PrimaryButtonLabel} Pressed"));
            controller.onSecondaryDown.AddListener(() => Debug.Log($"[{prefix}] {controller.SecondaryButtonLabel} Pressed"));
            controller.onMenuDown.AddListener(() => Debug.Log($"[{prefix}] Menu Pressed"));
            controller.onTriggerDown.AddListener(() => Debug.Log($"[{prefix}] Trigger Pressed"));
            controller.onGripDown.AddListener(() => Debug.Log($"[{prefix}] Grip Pressed"));
            controller.onThumbstickClickDown.AddListener(() => Debug.Log($"[{prefix}] Thumbstick Clicked"));
            controller.onThumbrestTouch.AddListener(() => Debug.Log($"[{prefix}] Thumbrest Touched"));
        }

        private void OnGUI()
        {
            if (!m_ShowGUI) return;

            GUILayout.BeginArea(new Rect(20, 20, 480, 520), GUI.skin.box);
            GUILayout.Label("<b><size=15>Meta Quest 3S Controller Monitor</size></b>");
            GUILayout.Space(5);

            DrawControllerStatus(m_LeftController, "Left Controller (Touch Plus)");
            GUILayout.Space(10);
            DrawControllerStatus(m_RightController, "Right Controller (Touch Plus)");

            GUILayout.EndArea();
        }

        private void DrawControllerStatus(Quest3SControllerMapping controller, string header)
        {
            GUILayout.Label($"<b>{header}</b>");
            if (controller == null)
            {
                GUILayout.Label("<color=yellow>Not assigned. Add Quest3SControllerMapping to the scene.</color>");
                return;
            }

            if (!controller.IsConnected)
            {
                GUILayout.Label("<color=red>● Disconnected / Searching for device...</color>");
                return;
            }

            GUILayout.Label("<color=lime>● Connected</color> (" + controller.DeviceName + ")");

            if (controller.Hand == ControllerHand.Left)
            {
                GUILayout.Label($"X: {(controller.IsPrimaryPressed ? "<b><color=cyan>[DOWN]</color></b>" : "[UP]")} | Touch: {(controller.IsPrimaryTouched ? "YES" : "NO")}");
                GUILayout.Label($"Y: {(controller.IsSecondaryPressed ? "<b><color=cyan>[DOWN]</color></b>" : "[UP]")} | Touch: {(controller.IsSecondaryTouched ? "YES" : "NO")}");
                GUILayout.Label($"Menu Button: {(controller.IsMenuPressed ? "<b><color=cyan>[DOWN]</color></b>" : "[UP]")}");
            }
            else
            {
                GUILayout.Label($"A: {(controller.IsPrimaryPressed ? "<b><color=cyan>[DOWN]</color></b>" : "[UP]")} | Touch: {(controller.IsPrimaryTouched ? "YES" : "NO")}");
                GUILayout.Label($"B: {(controller.IsSecondaryPressed ? "<b><color=cyan>[DOWN]</color></b>" : "[UP]")} | Touch: {(controller.IsSecondaryTouched ? "YES" : "NO")}");
            }

            GUILayout.Label($"Trigger: {controller.TriggerValue:F2} | Pressed: {(controller.IsTriggerPressed ? "YES" : "NO")} | Touch: {(controller.IsTriggerTouched ? "YES" : "NO")}");
            GUILayout.Label($"Grip: {controller.GripValue:F2} | Squeezed: {(controller.IsGripPressed ? "YES" : "NO")}");
            GUILayout.Label($"Stick Axis: {controller.ThumbstickAxis:F2} | Click: {(controller.IsThumbstickClicked ? "YES" : "NO")} | Touch: {(controller.IsThumbstickTouched ? "YES" : "NO")}");
            GUILayout.Label($"Thumbrest Touch: {(controller.IsThumbrestTouched ? "YES" : "NO")}");

            if (GUILayout.Button("Test Haptics (Vibrate)"))
            {
                controller.SendHapticImpulse(0.8f, 0.2f);
            }
        }
    }
}
