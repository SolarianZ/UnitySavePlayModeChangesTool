#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private static readonly HashSet<string> _excludePropertyNames = new HashSet<string>
        {
            "useGUILayout", "runInEditMode", "enabled", "hideFlags"
        };


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

                CopyComponentValues(playModeComponent, editModeComponent);

                UDebug.Log($"{_logPrefix} Apply play mode changes to {editModeComponent}.", editModeComponent);
            }

            _componentRegistry.Clear();

            UDebug.Log($"{_logPrefix} Apply play mode changes done.");
        }

        private static void CopyComponentValues(Behaviour source, Behaviour destination)
        {
            Undo.RecordObject(destination, "Apply Play Mode Changes");

            // Get all serialized properties(Unity's extern properties)
            var serializedPropertyInfos = source.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(propertyInfo =>
                {
                    return !_excludePropertyNames.Contains(propertyInfo.Name) && propertyInfo.CanRead && propertyInfo.CanWrite &&
                        ((propertyInfo.SetMethod.MethodImplementationFlags & MethodImplAttributes.InternalCall) != 0);
                });

            foreach (var propertyInfo in serializedPropertyInfos)
            {
                try
                {
                    var sourceValue = propertyInfo.GetValue(source);
                    propertyInfo.SetValue(destination, sourceValue);
                }
                catch (Exception e)
                {
                    UDebug.LogError($"{_logPrefix} Failed to copy component member: {source.GetType().Name}.{propertyInfo.Name}. Exception: {e.Message}", destination);
                }
            }

            // Get all serialized fields
            var serializedFieldInfos = source.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(fieldInfo =>
                {
                    return fieldInfo.IsPublic || fieldInfo.GetCustomAttribute(typeof(SerializeField)) != null ||
                        fieldInfo.GetCustomAttribute(typeof(SerializeReference)) != null;
                });

            foreach (var fieldInfo in serializedFieldInfos)
            {
                var sourceValue = fieldInfo.GetValue(source);
                fieldInfo.SetValue(destination, sourceValue);
            }
        }
    }
}
#endif
