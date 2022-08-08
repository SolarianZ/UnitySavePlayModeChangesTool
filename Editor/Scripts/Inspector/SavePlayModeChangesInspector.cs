using UnityEditor;
using UnityEngine;

namespace GBG.SavePlayModeChanges.Editor
{
    /// <summary>
    /// Custom inspector of <see cref="SavePlayModeChanges"/>.
    /// </summary>
    [CustomEditor(typeof(SavePlayModeChanges))]
    internal class SavePlayModeChangesInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(SavePlayModeChanges.Tips, MessageType.Info);

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();

            if (SavePlayModeChanges.CanCollectComponents())
            {
                EditorGUILayout.Space();

                if (GUILayout.Button("Collect All Components"))
                {
                    ((SavePlayModeChanges)target).EditorOnlyCollectAllComponents();
                }
            }
        }
    }
}
