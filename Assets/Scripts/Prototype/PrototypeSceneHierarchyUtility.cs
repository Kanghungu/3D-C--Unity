using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Shared helpers for prototype runtime scene roots and cleanup.
    /// Keeps hierarchy naming and rebuild cleanup out of the bootstrapper.
    /// </summary>
    public static class PrototypeSceneHierarchyUtility
    {
        public static Transform GetOrCreateRoot(string rootName)
        {
            foreach (Transform root in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (root.parent == null && (root.name == rootName || root.name.StartsWith(rootName + " (")))
                {
                    root.name = rootName;
                    return root;
                }
            }

            return new GameObject(rootName).transform;
        }

        public static void DestroyRootIfExists(string rootName)
        {
            foreach (Transform root in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (root.parent != null)
                {
                    continue;
                }

                if (root.name == rootName || root.name.StartsWith(rootName + " ("))
                {
                    Object.DestroyImmediate(root.gameObject);
                }
            }
        }

        public static void DestroyByType<T>() where T : Object
        {
            foreach (T instance in Object.FindObjectsByType<T>(FindObjectsSortMode.None))
            {
                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        public static void RefreshUnitRootLabel(Transform root, string baseName, UnitTeam team)
        {
            if (root == null)
            {
                return;
            }

            root.name = $"{baseName} ({PrototypeRuntimeQuery.CountUnits(team)})";
        }
    }
}
