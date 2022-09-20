#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UDebug = UnityEngine.Debug;

namespace GBG.SavePlayModeChanges
{
    /// <summary>
    /// A tool used for save changes made to the components during play mode.
    /// </summary>
    internal static class SavePlayModeChangesTool
    {
        private static bool _isInitialized;

        private static readonly string _logPrefix = $"[Editor][{nameof(SavePlayModeChangesTool)}]";

        private static readonly HashSet<Behaviour> _componentRegistry = new HashSet<Behaviour>();


        /// <summary>
        /// Register a component so that to save the changes to it.
        /// </summary>
        /// <param name="component"></param>
        public static void RegisterComponent(Behaviour component)
        {
            if (!Application.isPlaying)
            {
                UDebug.LogError($"{_logPrefix} Can't register component during edit mode.", component);
                return;
            }

            if (!component)
            {
                UDebug.LogError($"{_logPrefix} Register component failed: component is null.");
                return;
            }

            Initialize();

            _componentRegistry.Add(component);

            UDebug.Log($"{_logPrefix} Register component: {component}.", component);
        }


        private static void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            UDebug.Log($"{_logPrefix} Initialize {nameof(SavePlayModeChangesTool)}.");

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            _componentRegistry.Clear();

            _isInitialized = true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            _isInitialized = false;

            ApplyChanges();
        }

        private static void ApplyChanges()
        {
            UDebug.Log($"{_logPrefix} Apply play mode changes...");

            foreach (var playModeComponent in _componentRegistry)
            {
                // playModeComponent==null is always true, but you can use ?. operator
                var componentId = playModeComponent?.GetInstanceID();
                if (componentId == null)
                {
                    UDebug.LogError($"{_logPrefix} Skip destroyed play mode component.");
                    continue;
                }

                var editModeComponent = EditorUtility.InstanceIDToObject(playModeComponent.GetInstanceID()) as Behaviour;
                if (!editModeComponent)
                {
                    UDebug.LogError($"{_logPrefix} Skip nonexistent edit mode component: {playModeComponent}.");
                    continue;
                }

                Undo.RecordObject(editModeComponent, "Apply Play Mode Changes");
                EditorUtility.CopySerializedManagedFieldsOnly(playModeComponent, editModeComponent);

                UDebug.Log($"{_logPrefix} Apply play mode changes to {editModeComponent}.", editModeComponent);
            }

            _componentRegistry.Clear();

            UDebug.Log($"{_logPrefix} Apply play mode changes done.");
        }
    }
}
#endif
