#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MetaQuest3S.Input
{
    [CustomEditor(typeof(Quest3SControllerMapping))]
    public class Quest3SControllerMappingEditor : Editor
    {
        private SerializedProperty m_HandProp;
        private SerializedProperty m_TriggerPressThresholdProp;
        private SerializedProperty m_GripPressThresholdProp;
        private SerializedProperty m_ThumbstickDeadzoneProp;

        private void OnEnable()
        {
            m_HandProp = serializedObject.FindProperty("m_Hand");
            m_TriggerPressThresholdProp = serializedObject.FindProperty("m_TriggerPressThreshold");
            m_GripPressThresholdProp = serializedObject.FindProperty("m_GripPressThreshold");
            m_ThumbstickDeadzoneProp = serializedObject.FindProperty("m_ThumbstickDeadzone");
        }

        public override void OnInspectorGUI()
        {
            Quest3SControllerMapping mapper = (Quest3SControllerMapping)target;
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Meta Quest 3S / Quest 3 Controller", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                mapper.Hand == ControllerHand.Left
                    ? "Configured for LEFT controller: Physical inputs are X, Y, Menu, Left Trigger, Left Grip, Left Stick, and Thumbrest."
                    : "Configured for RIGHT controller: Physical inputs are A, B, Right Trigger, Right Grip, Right Stick, and Thumbrest.",
                MessageType.Info
            );

            // Live status banner in Play Mode
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUIStyle statusStyle = new GUIStyle(EditorStyles.boldLabel);
                if (mapper.IsConnected)
                {
                    statusStyle.normal.textColor = Color.green;
                    EditorGUILayout.LabelField("● Device Connected (" + mapper.DeviceName + ")", statusStyle);
                    EditorGUILayout.LabelField($"Primary ({(mapper.Hand == ControllerHand.Left ? "X" : "A")}): " + (mapper.IsPrimaryPressed ? "PRESSED" : "Released"));
                    EditorGUILayout.LabelField($"Secondary ({(mapper.Hand == ControllerHand.Left ? "Y" : "B")}): " + (mapper.IsSecondaryPressed ? "PRESSED" : "Released"));
                    EditorGUILayout.LabelField($"Trigger: {mapper.TriggerValue:F2} (Pressed: {mapper.IsTriggerPressed})");
                    EditorGUILayout.LabelField($"Grip: {mapper.GripValue:F2} (Pressed: {mapper.IsGripPressed})");
                    EditorGUILayout.LabelField($"Thumbstick: {mapper.ThumbstickAxis} (Click: {mapper.IsThumbstickClicked})");
                }
                else
                {
                    statusStyle.normal.textColor = Color.yellow;
                    EditorGUILayout.LabelField("○ Searching for Quest 3S Controller...", statusStyle);
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
