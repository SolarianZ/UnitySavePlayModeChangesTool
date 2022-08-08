#if UNITY_EDITOR
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[assembly: InternalsVisibleTo("GBG.SavePlayModeChanges.Editor")]

namespace GBG.SavePlayModeChanges
{
    /// <summary>
    /// Save changes made to the components during play mode.
    /// </summary>
    [DisallowMultipleComponent]
    internal class SavePlayModeChanges : MonoBehaviour
    {
        public const string Tips = "Only support serialized fields and limited range of properties." +
            "\nProperty limitations:" +
            "\n    SetMethod.MethodImplementationFlags should be contains InternalCall(declare with 'extern').";

        [Tooltip(Tips)]
        [SerializeField]
        private Behaviour[] _componentsToSave;


        private void Start()
        {
            if (_componentsToSave == null)
            {
                return;
            }

            foreach (var component in _componentsToSave)
            {
                if (component)
                {
                    SavePlayModeChangesTool.RegisterComponent(component);
                }
            }
        }


        public static bool CanCollectComponents()
        {
            return Application.isPlaying == false;
        }

        public void EditorOnlyCollectAllComponents()
        {
            Assert.IsTrue(CanCollectComponents());

            var components = GetComponents<Behaviour>();
            var validComponentCount = 0;
            foreach (var component in components)
            {
                if (component == this)
                {
                    continue;
                }

                ++validComponentCount;
            }

            Undo.RecordObject(this, "Collect All Components");
            _componentsToSave = new Behaviour[validComponentCount];
            var componentIndex = 0;
            foreach (var component in components)
            {
                if (component == this)
                {
                    continue;
                }

                _componentsToSave[componentIndex] = component;
                ++componentIndex;
            }
        }
    }
}
#endif
